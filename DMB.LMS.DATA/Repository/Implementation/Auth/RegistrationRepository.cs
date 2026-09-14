using Dmb.Lms.Data.Context;
using Dmb.Lms.Data.Entities;
using Dmb.Lms.Data.Repository.Interface.Auth;
using Dmb.Lms.Data.Security;
using Dmb.Lms.Model;
using Dmb.Lms.Model.Abstractions;
using Dmb.Lms.Model.Dtos.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dmb.Lms.Data.Repository.Implementation.Auth;

public class RegistrationRepository : IRegistrationRepository
{
    private readonly LmsContext _db;
    private readonly IAuthRepository _authRepository;
    private readonly IActivationEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegistrationRepository> _logger;

    public RegistrationRepository(
        LmsContext db,
        IAuthRepository authRepository,
        IActivationEmailSender emailSender,
        IConfiguration configuration,
        ILogger<RegistrationRepository> logger)
    {
        _db = db;
        _authRepository = authRepository;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RegisterWithActivationOutcome> RegisterWithActivationAsync(
        RegisterDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var slug = string.IsNullOrWhiteSpace(request.AgencySlug) ? "dmb" : request.AgencySlug.Trim().ToLowerInvariant();
        var agency = await _db.Agencies.FirstOrDefaultAsync(a => a.Slug == slug, cancellationToken);
        if (agency is null) return RegisterWithActivationOutcome.AgencyNotFound;

        if (await _db.Users.AnyAsync(u => u.AgencyId == agency.Id && (u.Email.ToLower() == email || u.Username.ToLower() == email), cancellationToken))
        {
            return RegisterWithActivationOutcome.DuplicateEmail;
        }

        var isFirstUser = !await _db.Users.AnyAsync(u => u.AgencyId == agency.Id, cancellationToken);
        var (passwordHash, passwordSalt) = _authRepository.CreatePasswordHash(request.Password);
        var now = DateTimeOffset.UtcNow;
        var role = isFirstUser
            ? Roles.Owner
            : (request.Role is Roles.Tutor or Roles.Parent ? request.Role : Roles.Parent);

        var user = new LmsUser
        {
            Id = Guid.NewGuid(),
            AgencyId = agency.Id,
            Username = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            ContactNo = request.ContactNumber?.Trim(),
            Activated = isFirstUser,
            IsSuperAdmin = isFirstUser,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Users.Add(user);

        var location = await _db.Locations.Where(l => l.AgencyId == agency.Id).OrderBy(l => l.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (location is not null)
        {
            _db.UserLocations.Add(new UserLocation
            {
                UserId = user.Id,
                LocationId = location.Id,
                Role = role,
                CreatedAt = now
            });

            if (role == Roles.Tutor)
            {
                _db.TutorProfiles.Add(new TutorProfile
                {
                    Id = Guid.NewGuid(),
                    LocationId = location.Id,
                    UserId = user.Id,
                    Headline = "New tutor",
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }

        var rawToken = TokenHasher.CreateRawToken();
        _db.AccountActivationTokens.Add(new AccountActivationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawToken),
            ExpiresAt = now.AddDays(2),
            CreatedAt = now
        });
        await _db.SaveChangesAsync(cancellationToken);

        if (!isFirstUser)
        {
            var frontendUrl = (_configuration["App:FrontendUrl"] ?? string.Empty).TrimEnd('/');
            if (string.IsNullOrWhiteSpace(frontendUrl))
            {
                throw new InvalidOperationException("App:FrontendUrl is not configured.");
            }

            try
            {
                await _emailSender.SendAccountActivationEmailAsync(
                    email, $"{frontendUrl}/activate?token={Uri.EscapeDataString(rawToken)}", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send activation email to {Email}.", email);
                _db.AccountActivationTokens.RemoveRange(_db.AccountActivationTokens.Where(t => t.UserId == user.Id));
                _db.Users.Remove(user);
                await _db.SaveChangesAsync(cancellationToken);
                return RegisterWithActivationOutcome.ActivationEmailSendFailed;
            }
        }

        return RegisterWithActivationOutcome.Success;
    }

    public async Task<ActivateAccountOutcome> CompleteAccountActivationAsync(string? token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return ActivateAccountOutcome.InvalidOrExpiredToken;
        var hash = TokenHasher.Hash(token);
        var row = await _db.AccountActivationTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (row is null || row.UsedAt is not null || row.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return ActivateAccountOutcome.InvalidOrExpiredToken;
        }

        row.UsedAt = DateTimeOffset.UtcNow;
        row.User.Activated = true;
        row.User.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ActivateAccountOutcome.Success;
    }
}
