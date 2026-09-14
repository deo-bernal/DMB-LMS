using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dmb.Lms.Data.Context;
using Dmb.Lms.Data.Entities;
using Dmb.Lms.Data.Repository.Interface.Auth;
using Dmb.Lms.Model;
using Dmb.Lms.Model.Abstractions;
using Dmb.Lms.Model.Dtos.Auth;
using Dmb.Lms.Service.Interface.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dmb.Lms.Service.Implementation.Auth;

public class ExternalAuthService : IExternalAuthService
{
    private static readonly HashSet<string> Providers = new(StringComparer.OrdinalIgnoreCase)
    {
        "google", "linkedin", "facebook"
    };

    private readonly LmsContext _db;
    private readonly IAuthRepository _authRepository;
    private readonly IExternalLoginEmailSender _emailService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalAuthService> _logger;

    public ExternalAuthService(
        LmsContext db,
        IAuthRepository authRepository,
        IExternalLoginEmailSender emailService,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<ExternalAuthService> logger)
    {
        _db = db;
        _authRepository = authRepository;
        _emailService = emailService;
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public ExternalAuthStartResult Start(string provider, string? redirect, string? role, string callbackUrl)
    {
        var normalized = NormalizeProvider(provider);
        if (normalized is null)
        {
            return new ExternalAuthStartResult { ErrorMessage = "Unknown sign-in provider." };
        }

        if (!TryGetClientId(normalized, out var clientId))
        {
            return new ExternalAuthStartResult
            {
                RedirectUrl = ErrorRedirect(redirect, $"{Title(normalized)} sign-in is not configured yet.")
            };
        }

        var state = CreateState(redirect, NormalizeRole(role));
        var authUrl = normalized switch
        {
            "google" =>
                "https://accounts.google.com/o/oauth2/v2/auth"
                + "?response_type=code"
                + "&scope=" + Uri.EscapeDataString("openid email profile")
                + "&client_id=" + Uri.EscapeDataString(clientId)
                + "&redirect_uri=" + Uri.EscapeDataString(callbackUrl)
                + "&state=" + Uri.EscapeDataString(state)
                + "&access_type=online"
                + "&prompt=select_account",
            "linkedin" =>
                "https://www.linkedin.com/oauth/v2/authorization"
                + "?response_type=code"
                + "&scope=" + Uri.EscapeDataString("openid profile email")
                + "&client_id=" + Uri.EscapeDataString(clientId)
                + "&redirect_uri=" + Uri.EscapeDataString(callbackUrl)
                + "&state=" + Uri.EscapeDataString(state),
            _ =>
                "https://www.facebook.com/v21.0/dialog/oauth"
                + "?response_type=code"
                + "&scope=" + Uri.EscapeDataString("public_profile,email")
                + "&client_id=" + Uri.EscapeDataString(clientId)
                + "&redirect_uri=" + Uri.EscapeDataString(callbackUrl)
                + "&state=" + Uri.EscapeDataString(state)
        };

        return new ExternalAuthStartResult { RedirectUrl = authUrl };
    }

    public async Task<string> HandleCallbackAsync(
        string provider,
        string? code,
        string? state,
        string? error,
        string? errorDescription,
        string callbackUrl,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeProvider(provider);
        var parsedState = ParseState(state);
        var returnPath = parsedState?.ReturnPath;
        var role = NormalizeRole(parsedState?.Role);

        if (normalized is null)
        {
            return ErrorRedirect(returnPath, "Unknown sign-in provider.");
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            return ErrorRedirect(returnPath, DescribeProviderError(normalized, error, errorDescription));
        }

        if (parsedState is null)
        {
            return ErrorRedirect(returnPath, "Sign-in session expired. Try again.");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return ErrorRedirect(returnPath, $"{Title(normalized)} did not return an authorization code.");
        }

        if (!TryGetClientId(normalized, out var clientId))
        {
            return ErrorRedirect(returnPath, $"{Title(normalized)} sign-in is missing ClientId on dmb-lms-api.");
        }

        if (!TryGetProviderConfig(normalized, out _, out var clientSecret))
        {
            return ErrorRedirect(returnPath, $"{Title(normalized)} sign-in is missing ClientSecret on dmb-lms-api.");
        }

        OAuthProfile profile;
        try
        {
            profile = await ExchangeCodeAsync(normalized, code, callbackUrl, clientId, clientSecret, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OAuth token exchange failed for {Provider}.", normalized);
            var detail = (ex.Message ?? "").Trim();
            if (detail.StartsWith("Facebook:", StringComparison.OrdinalIgnoreCase)
                || detail.StartsWith("LinkedIn:", StringComparison.OrdinalIgnoreCase)
                || detail.StartsWith("Google:", StringComparison.OrdinalIgnoreCase))
            {
                return ErrorRedirect(returnPath, detail);
            }

            return ErrorRedirect(returnPath, $"Could not complete {Title(normalized)} sign-in. Try again.");
        }

        if (string.IsNullOrWhiteSpace(profile.ProviderUserId))
        {
            return ErrorRedirect(returnPath, $"{Title(normalized)} did not return a user id.");
        }

        var existingLink = await _db.ExternalLogins
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l => l.Provider == normalized && l.ProviderUserId == profile.ProviderUserId,
                cancellationToken);

        if (existingLink is not null)
        {
            return await IssueAndRedirectAsync(existingLink.UserId, returnPath, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(profile.Email))
        {
            var email = profile.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.Email.ToLower() == email || u.Username.ToLower() == email,
                cancellationToken);

            if (user is not null)
            {
                await AttachExternalLoginAsync(user.Id, normalized, profile.ProviderUserId, cancellationToken);
                if (!user.Activated)
                {
                    user.Activated = true;
                    user.UpdatedAt = DateTimeOffset.UtcNow;
                    await _db.SaveChangesAsync(cancellationToken);
                }

                await EnsureLocationMembershipAsync(user.Id, user.AgencyId, null, cancellationToken);
                return await IssueAndRedirectAsync(user.Id, returnPath, cancellationToken);
            }

            var created = await CreateSocialUserAsync(profile, email, role, cancellationToken);
            await AttachExternalLoginAsync(created.Id, normalized, profile.ProviderUserId, cancellationToken);
            return await IssueAndRedirectAsync(created.Id, returnPath, cancellationToken);
        }

        var ticket = CreateTicket();
        _db.PendingExternalLogins.Add(new PendingExternalLogin
        {
            Id = Guid.NewGuid(),
            Ticket = ticket,
            Provider = normalized,
            ProviderUserId = profile.ProviderUserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Phone = ClipPhone(profile.Phone),
            Client = PackClient(role),
            ReturnPath = returnPath,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
        return FrontendUrl() + "/auth/complete?ticket=" + Uri.EscapeDataString(ticket);
    }

    public async Task<(bool Ok, string Message, int StatusCode)> CompleteAsync(
        ExternalAuthCompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        var pending = await GetValidPendingAsync(request.Ticket, cancellationToken);
        if (pending is null)
        {
            return (false, "This sign-up session expired. Start again with the social button.", 400);
        }

        var email = request.Email?.Trim().ToLowerInvariant() ?? "";
        if (email.Length < 5 || !email.Contains('@', StringComparison.Ordinal))
        {
            return (false, "Enter a valid email address.", 400);
        }

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        pending.Email = email;
        pending.Phone = ClipPhone(request.Phone) ?? pending.Phone;
        pending.CodeHash = HashToken(code);
        pending.CodeExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        pending.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendExternalLoginCodeEmailAsync(email, code, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SSO verification code to {Email}.", email);
            return (false, "Unable to send the verification email. Please try again later.", 503);
        }

        return (true, "We sent a 6-digit code to that email.", 200);
    }

    public async Task<ExternalAuthVerifyResult> VerifyAsync(
        ExternalAuthVerifyRequest request,
        CancellationToken cancellationToken = default)
    {
        var pending = await GetValidPendingAsync(request.Ticket, cancellationToken);
        if (pending is null)
        {
            return Fail("This sign-up session expired. Start again with the social button.", 400);
        }

        if (string.IsNullOrWhiteSpace(pending.Email)
            || string.IsNullOrWhiteSpace(pending.CodeHash)
            || pending.CodeExpiresAt is null
            || pending.CodeExpiresAt < DateTimeOffset.UtcNow)
        {
            return Fail("Request a new code first.", 400);
        }

        var submitted = (request.Code ?? "").Trim();
        if (submitted.Length != 6 || HashToken(submitted) != pending.CodeHash)
        {
            return Fail("That code is incorrect.", 400);
        }

        var email = pending.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == email || u.Username.ToLower() == email,
            cancellationToken);

        if (user is null)
        {
            user = await CreateSocialUserAsync(
                new OAuthProfile
                {
                    ProviderUserId = pending.ProviderUserId,
                    Email = email,
                    FirstName = pending.FirstName,
                    LastName = pending.LastName,
                    Phone = pending.Phone
                },
                email,
                UnpackRole(pending.Client),
                cancellationToken);
        }
        else if (!user.Activated)
        {
            user.Activated = true;
            user.UpdatedAt = DateTimeOffset.UtcNow;
        }

        if (!string.IsNullOrWhiteSpace(pending.Phone) && string.IsNullOrWhiteSpace(user.ContactNo))
        {
            user.ContactNo = ClipPhone(pending.Phone);
        }

        await AttachExternalLoginAsync(user.Id, pending.Provider, pending.ProviderUserId, cancellationToken);
        await EnsureLocationMembershipAsync(user.Id, user.AgencyId, null, cancellationToken);
        _db.PendingExternalLogins.Remove(pending);
        await _db.SaveChangesAsync(cancellationToken);

        var tokens = await _authRepository.IssueJwtForUserAsync(user.Id, cancellationToken);
        if (tokens.Status != AuthTokenLoginStatus.Success)
        {
            return Fail(tokens.BlockReason ?? "Could not sign you in.", 403);
        }

        return new ExternalAuthVerifyResult
        {
            Success = true,
            AccessToken = tokens.AccessToken,
            Locations = tokens.Locations,
            CurrentLocationId = tokens.CurrentLocationId,
            FirstName = tokens.FirstName
        };
    }

    private async Task<string> IssueAndRedirectAsync(Guid userId, string? returnPath, CancellationToken cancellationToken)
    {
        var jwt = await _authRepository.IssueJwtForUserAsync(userId, cancellationToken);
        if (jwt.Status != AuthTokenLoginStatus.Success || string.IsNullOrWhiteSpace(jwt.AccessToken))
        {
            return ErrorRedirect(returnPath, jwt.BlockReason ?? "Could not sign you in.");
        }

        var url = FrontendUrl() + "/auth/callback?token=" + Uri.EscapeDataString(jwt.AccessToken);
        var webPath = GetSafeRedirectPath(returnPath);
        if (!string.IsNullOrWhiteSpace(webPath))
        {
            url += "&redirect=" + Uri.EscapeDataString(webPath);
        }

        return url;
    }

    private async Task AttachExternalLoginAsync(Guid userId, string provider, string providerUserId, CancellationToken cancellationToken)
    {
        var exists = await _db.ExternalLogins.AnyAsync(
            l => l.Provider == provider && l.ProviderUserId == providerUserId,
            cancellationToken);
        if (exists)
        {
            return;
        }

        var sameProvider = await _db.ExternalLogins.AnyAsync(
            l => l.UserId == userId && l.Provider == provider,
            cancellationToken);
        if (sameProvider)
        {
            return;
        }

        _db.ExternalLogins.Add(new ExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<LmsUser> CreateSocialUserAsync(OAuthProfile profile, string email, string role, CancellationToken cancellationToken)
    {
        var agency = await _db.Agencies.FirstOrDefaultAsync(a => a.Slug == "dmb", cancellationToken)
            ?? throw new InvalidOperationException("Default LMS agency was not found.");

        var isFirstUser = !await _db.Users.AnyAsync(u => u.AgencyId == agency.Id, cancellationToken);
        var assignedRole = isFirstUser
            ? Roles.Owner
            : (role is Roles.Tutor or Roles.Parent ? role : Roles.Parent);
        var randomPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        var (hash, salt) = _authRepository.CreatePasswordHash(randomPassword);
        var now = DateTimeOffset.UtcNow;
        var user = new LmsUser
        {
            Id = Guid.NewGuid(),
            AgencyId = agency.Id,
            Username = email,
            Email = email,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            PasswordHash = hash,
            PasswordSalt = salt,
            ContactNo = ClipPhone(profile.Phone),
            Activated = true,
            IsSuperAdmin = isFirstUser,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        await EnsureLocationMembershipAsync(user.Id, user.AgencyId, assignedRole, cancellationToken);
        return user;
    }

    private async Task EnsureLocationMembershipAsync(Guid userId, Guid agencyId, string? role, CancellationToken cancellationToken)
    {
        var hasLocation = await _db.UserLocations.AnyAsync(ul => ul.UserId == userId, cancellationToken);
        if (hasLocation)
        {
            return;
        }

        var location = await _db.Locations
            .Where(l => l.AgencyId == agencyId && l.IsActive)
            .OrderBy(l => l.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (location is null)
        {
            return;
        }

        var assignedRole = role is Roles.Tutor or Roles.Parent or Roles.Owner or Roles.Admin
            ? role
            : Roles.Parent;
        var now = DateTimeOffset.UtcNow;
        _db.UserLocations.Add(new UserLocation
        {
            UserId = userId,
            LocationId = location.Id,
            Role = assignedRole,
            CreatedAt = now
        });

        if (assignedRole == Roles.Tutor
            && !await _db.TutorProfiles.AnyAsync(t => t.LocationId == location.Id && t.UserId == userId, cancellationToken))
        {
            _db.TutorProfiles.Add(new TutorProfile
            {
                Id = Guid.NewGuid(),
                LocationId = location.Id,
                UserId = userId,
                Headline = "New tutor",
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<PendingExternalLogin?> GetValidPendingAsync(string? ticket, CancellationToken cancellationToken)
    {
        var value = ticket?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var pending = await _db.PendingExternalLogins.FirstOrDefaultAsync(p => p.Ticket == value, cancellationToken);
        if (pending is null || pending.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return null;
        }

        return pending;
    }

    private async Task<OAuthProfile> ExchangeCodeAsync(
        string provider,
        string code,
        string callbackUrl,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        return provider switch
        {
            "google" => await ExchangeGoogleAsync(code, callbackUrl, clientId, clientSecret, cancellationToken),
            "linkedin" => await ExchangeLinkedInAsync(code, callbackUrl, clientId, clientSecret, cancellationToken),
            _ => await ExchangeFacebookAsync(code, callbackUrl, clientId, clientSecret, cancellationToken)
        };
    }

    private async Task<OAuthProfile> ExchangeGoogleAsync(
        string code, string callbackUrl, string clientId, string clientSecret, CancellationToken cancellationToken)
    {
        using var tokenResponse = await _httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = callbackUrl,
                ["grant_type"] = "authorization_code"
            }),
            cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<OAuthTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Google token response was empty.");

        using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        using var userResponse = await _httpClient.SendAsync(userRequest, cancellationToken);
        userResponse.EnsureSuccessStatusCode();
        var user = await userResponse.Content.ReadFromJsonAsync<GoogleUserInfo>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Google profile was empty.");

        var (first, last) = SplitName(user.GivenName, user.FamilyName, user.Name);
        return new OAuthProfile
        {
            ProviderUserId = user.Sub ?? "",
            Email = string.IsNullOrWhiteSpace(user.Email) || user.EmailVerified == false ? null : user.Email,
            FirstName = first,
            LastName = last
        };
    }

    private async Task<OAuthProfile> ExchangeLinkedInAsync(
        string code, string callbackUrl, string clientId, string clientSecret, CancellationToken cancellationToken)
    {
        var redirectUris = new[]
        {
            callbackUrl,
            "https://www.dmbwebsolutions.com/api/auth/external/linkedin/callback",
            "https://dmbportfolio-api.onrender.com/api/auth/external/linkedin/callback"
        }.Distinct(StringComparer.Ordinal).ToArray();

        OAuthTokenResponse? token = null;
        string? lastError = null;
        foreach (var redirectUri in redirectUris)
        {
            using var tokenResponse = await _httpClient.PostAsync(
                "https://www.linkedin.com/oauth/v2/accessToken",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["redirect_uri"] = redirectUri,
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret
                }),
                cancellationToken);
            var tokenBody = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            if (tokenResponse.IsSuccessStatusCode)
            {
                token = JsonSerializer.Deserialize<OAuthTokenResponse>(tokenBody);
                break;
            }

            lastError = ParseOAuthError(tokenBody) ?? $"LinkedIn token exchange failed ({(int)tokenResponse.StatusCode}).";
            if (LooksLikeInvalidClient(lastError))
            {
                throw new InvalidOperationException(
                    "LinkedIn: The LinkedIn Client Secret on this API does not match this app. Copy Authentication__LinkedIn__ClientSecret from dmbportfolio-api onto this service.");
            }

            if (!LooksLikeRedirectMismatch(lastError) && !LooksLikeInvalidGrant(lastError))
            {
                throw new InvalidOperationException("LinkedIn: " + lastError);
            }
        }

        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("LinkedIn: " + (lastError ?? "LinkedIn token response was empty."));
        }

        using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        using var userResponse = await _httpClient.SendAsync(userRequest, cancellationToken);
        userResponse.EnsureSuccessStatusCode();
        var user = await userResponse.Content.ReadFromJsonAsync<LinkedInUserInfo>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("LinkedIn profile was empty.");

        var (first, last) = SplitName(user.GivenName, user.FamilyName, user.Name);
        return new OAuthProfile
        {
            ProviderUserId = user.Sub ?? "",
            Email = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email,
            FirstName = first,
            LastName = last
        };
    }

    private async Task<OAuthProfile> ExchangeFacebookAsync(
        string code, string callbackUrl, string clientId, string clientSecret, CancellationToken cancellationToken)
    {
        var redirectUris = new[]
        {
            callbackUrl,
            "https://www.dmbwebsolutions.com/api/auth/external/facebook/callback",
            "https://www.dmbwebsolutions.com/lms/api/auth/external/facebook/callback"
        }.Distinct(StringComparer.Ordinal).ToArray();

        OAuthTokenResponse? token = null;
        string? lastError = null;
        foreach (var redirectUri in redirectUris)
        {
            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://graph.facebook.com/v21.0/oauth/access_token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["redirect_uri"] = redirectUri,
                    ["code"] = code
                })
            };
            using var tokenResponse = await _httpClient.SendAsync(tokenRequest, cancellationToken);
            var tokenBody = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            if (tokenResponse.IsSuccessStatusCode)
            {
                token = JsonSerializer.Deserialize<OAuthTokenResponse>(tokenBody);
                break;
            }

            lastError = ParseFacebookError(tokenBody) ?? $"Facebook token exchange failed ({(int)tokenResponse.StatusCode}).";
            if (LooksLikeInvalidClient(lastError))
            {
                throw new InvalidOperationException(
                    "Facebook: The Facebook App Secret on this API does not match this app. Copy Authentication__Facebook__ClientSecret from dmbportfolio-api onto this service.");
            }

            if (!LooksLikeRedirectMismatch(lastError))
            {
                throw new InvalidOperationException("Facebook: " + lastError);
            }
        }

        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("Facebook: " + (lastError ?? "Facebook token response was empty."));
        }

        var profileUrl =
            "https://graph.facebook.com/me"
            + "?fields=id,first_name,last_name,name,email"
            + "&access_token=" + Uri.EscapeDataString(token.AccessToken ?? "");
        using var userResponse = await _httpClient.GetAsync(profileUrl, cancellationToken);
        var profileBody = await userResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!userResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Facebook: " + (ParseFacebookError(profileBody) ?? "Facebook profile request failed."));
        }

        var user = JsonSerializer.Deserialize<FacebookUserInfo>(profileBody)
            ?? throw new InvalidOperationException("Facebook profile was empty.");

        var (first, last) = SplitName(user.FirstName, user.LastName, user.Name);
        return new OAuthProfile
        {
            ProviderUserId = user.Id ?? "",
            Email = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email,
            FirstName = first,
            LastName = last
        };
    }

    private bool TryGetClientId(string provider, out string clientId)
    {
        clientId = (ProviderSection(provider, "ClientId") ?? "").Trim();
        return clientId.Length > 0;
    }

    private bool TryGetProviderConfig(string provider, out string clientId, out string clientSecret)
    {
        clientId = (ProviderSection(provider, "ClientId") ?? "").Trim();
        clientSecret = (ProviderSection(provider, "ClientSecret") ?? "").Trim();
        return clientId.Length > 0 && clientSecret.Length > 0;
    }

    private string? ProviderSection(string provider, string key)
    {
        var section = provider switch
        {
            "google" => "Authentication:Google",
            "linkedin" => "Authentication:LinkedIn",
            _ => "Authentication:Facebook"
        };
        return _configuration[$"{section}:{key}"];
    }

    private string CreateState(string? redirect, string role)
    {
        var payload = JsonSerializer.Serialize(new OAuthStatePayload
        {
            Nonce = Guid.NewGuid().ToString("N"),
            ReturnPath = GetSafeRedirectPath(redirect),
            Role = role,
            Exp = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()
        });
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var signature = HMACSHA256.HashData(StateKey(), payloadBytes);
        return "lms." + ToBase64Url(payloadBytes) + "." + ToBase64Url(signature);
    }

    private OAuthStatePayload? ParseState(string? state)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            return null;
        }

        if (state.StartsWith("lms.", StringComparison.Ordinal))
        {
            state = state[4..];
        }

        var parts = state.Split('.', 2);
        if (parts.Length != 2)
        {
            return null;
        }

        try
        {
            var payloadBytes = FromBase64Url(parts[0]);
            var signature = FromBase64Url(parts[1]);
            var expected = HMACSHA256.HashData(StateKey(), payloadBytes);
            if (!CryptographicOperations.FixedTimeEquals(signature, expected))
            {
                return null;
            }

            var payload = JsonSerializer.Deserialize<OAuthStatePayload>(payloadBytes);
            if (payload is null || payload.Exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return null;
            }

            return payload;
        }
        catch
        {
            return null;
        }
    }

    private byte[] StateKey()
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        return Encoding.UTF8.GetBytes(secret);
    }

    private string FrontendUrl()
    {
        var url = (_configuration["App:FrontendUrl"] ?? "").Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(url)
            || url.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            || url.Contains("onrender.com", StringComparison.OrdinalIgnoreCase))
        {
            return "https://www.dmbwebsolutions.com/lms";
        }

        return url;
    }

    private string ErrorRedirect(string? returnPath, string message)
    {
        var login = FrontendUrl() + "/login?ssoError=" + Uri.EscapeDataString(message);
        var path = GetSafeRedirectPath(returnPath);
        if (!string.IsNullOrWhiteSpace(path))
        {
            login += "&redirect=" + Uri.EscapeDataString(path);
        }

        return login;
    }

    private static string? NormalizeProvider(string? provider)
    {
        var value = provider?.Trim().ToLowerInvariant();
        return value is not null && Providers.Contains(value) ? value : null;
    }

    private static string Title(string provider) => provider switch
    {
        "google" => "Google",
        "linkedin" => "LinkedIn",
        "facebook" => "Facebook",
        _ => provider
    };

    private static string NormalizeRole(string? role) =>
        string.Equals(role?.Trim(), Roles.Tutor, StringComparison.OrdinalIgnoreCase) ? Roles.Tutor : Roles.Parent;

    private static string PackClient(string role) =>
        role == Roles.Tutor ? "web.tutor" : "web";

    private static string UnpackRole(string? client) =>
        string.Equals(client, "web.tutor", StringComparison.OrdinalIgnoreCase) ? Roles.Tutor : Roles.Parent;

    private static bool LooksLikeRedirectMismatch(string message) =>
        message.Contains("redirect_uri", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeInvalidGrant(string message) =>
        message.Contains("invalid_grant", StringComparison.OrdinalIgnoreCase)
        || message.Contains("authorization code", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeInvalidClient(string message) =>
        message.Contains("invalid_client", StringComparison.OrdinalIgnoreCase)
        || message.Contains("client secret", StringComparison.OrdinalIgnoreCase)
        || message.Contains("client_secret", StringComparison.OrdinalIgnoreCase);

    private static string? ParseOAuthError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error_description", out var description))
            {
                var text = description.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.Length > 220 ? text[..220] : text;
                }
            }

            if (doc.RootElement.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
            {
                return error.GetString();
            }
        }
        catch (JsonException)
        {
            // Fall through to a short raw snippet.
        }

        var trimmed = body.Trim();
        return trimmed.Length > 180 ? trimmed[..180] : trimmed;
    }

    private static string? ParseFacebookError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message))
            {
                var text = message.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.Length > 220 ? text[..220] : text;
                }
            }
        }
        catch (JsonException)
        {
            // Fall through to a short raw snippet.
        }

        var trimmed = body.Trim();
        return trimmed.Length > 180 ? trimmed[..180] : trimmed;
    }

    private static string DescribeProviderError(string provider, string error, string? errorDescription)
    {
        if (string.Equals(error, "access_denied", StringComparison.OrdinalIgnoreCase))
        {
            return $"{Title(provider)} sign-in was cancelled.";
        }

        var detail = (errorDescription ?? "").Trim();
        if (detail.Length > 180)
        {
            detail = detail[..180];
        }

        return string.IsNullOrWhiteSpace(detail)
            ? $"{Title(provider)} sign-in failed ({error})."
            : $"{Title(provider)} sign-in failed: {detail}";
    }

    private static string? GetSafeRedirectPath(string? value)
    {
        var redirect = value?.Trim();
        if (string.IsNullOrWhiteSpace(redirect) || !redirect.StartsWith('/') || redirect.StartsWith("//", StringComparison.Ordinal))
        {
            return null;
        }

        return redirect.Length > 500 ? redirect[..500] : redirect;
    }

    private static (string First, string Last) SplitName(string? given, string? family, string? full)
    {
        var first = (given ?? "").Trim();
        var last = (family ?? "").Trim();
        if (first.Length == 0 || last.Length == 0)
        {
            var parts = (full ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (first.Length == 0)
            {
                first = parts.Length > 0 ? parts[0] : "Member";
            }

            if (last.Length == 0)
            {
                last = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "Account";
            }
        }

        return (Clip(first, 100), Clip(last, 100));
    }

    private static string Clip(string value, int max)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }

    private static string? ClipPhone(string? phone)
    {
        var value = phone?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Length <= 30 ? value : value[..30];
    }

    private static string CreateTicket() => ToBase64Url(RandomNumberGenerator.GetBytes(32));

    private static string HashToken(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static ExternalAuthVerifyResult Fail(string message, int status) => new()
    {
        Success = false,
        ErrorMessage = message,
        StatusCode = status
    };

    private static string ToBase64Url(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromBase64Url(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        return Convert.FromBase64String(padded);
    }

    private sealed class OAuthProfile
    {
        public string ProviderUserId { get; init; } = "";
        public string? Email { get; init; }
        public string FirstName { get; init; } = "Member";
        public string LastName { get; init; } = "Account";
        public string? Phone { get; init; }
    }

    private sealed class OAuthStatePayload
    {
        [JsonPropertyName("n")]
        public string Nonce { get; set; } = "";

        [JsonPropertyName("r")]
        public string? ReturnPath { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("exp")]
        public long Exp { get; set; }
    }

    private sealed class OAuthTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private sealed class GoogleUserInfo
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("email_verified")]
        public bool? EmailVerified { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class LinkedInUserInfo
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class FacebookUserInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
