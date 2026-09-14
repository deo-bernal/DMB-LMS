using Dmb.Lms.Data.Context;
using Dmb.Lms.Data.Entities;
using Dmb.Lms.Data.Repository.Interface.Auth;
using Dmb.Lms.Data.Security;
using Dmb.Lms.Model.Abstractions;
using Dmb.Lms.Model.Dtos.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dmb.Lms.Data.Repository.Implementation.Auth;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly LmsContext _db;
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordResetEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PasswordResetRepository> _logger;

    public PasswordResetRepository(
        LmsContext db,
        IAuthRepository authRepository,
        IPasswordResetEmailSender emailSender,
        IConfiguration configuration,
        ILogger<PasswordResetRepository> logger)
    {
        _db = db;
        _authRepository = authRepository;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ForgotPasswordRequestStatus> RequestPasswordResetAsync(ForgotPasswordDto request, CancellationToken cancellationToken = default)
    {
        var normalized = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalized, cancellationToken);
        if (user is null) return ForgotPasswordRequestStatus.Ok;

        var existing = await _db.PasswordResetTokens.Where(t => t.UserId == user.Id).ToListAsync(cancellationToken);
        if (existing.Count > 0) _db.PasswordResetTokens.RemoveRange(existing);

        var rawToken = TokenHasher.CreateRawToken();
        var row = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.PasswordResetTokens.Add(row);
        await _db.SaveChangesAsync(cancellationToken);

        var frontendUrl = (_configuration["App:FrontendUrl"] ?? string.Empty).TrimEnd('/');
        try
        {
            await _emailSender.SendPasswordResetEmailAsync(
                user.Email, $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}.", user.Email);
            _db.PasswordResetTokens.Remove(row);
            await _db.SaveChangesAsync(cancellationToken);
            return ForgotPasswordRequestStatus.EmailServiceUnavailable;
        }

        return ForgotPasswordRequestStatus.Ok;
    }

    public async Task<PasswordResetCompletionStatus> CompletePasswordResetAsync(ResetPasswordDto request, CancellationToken cancellationToken = default)
    {
        if (request.Password != request.ConfirmPassword) return PasswordResetCompletionStatus.PasswordMismatch;
        var hash = TokenHasher.Hash(request.Token);
        var row = await _db.PasswordResetTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (row is null || row.UsedAt is not null || row.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return PasswordResetCompletionStatus.InvalidOrExpiredToken;
        }

        var (passwordHash, passwordSalt) = _authRepository.CreatePasswordHash(request.Password);
        row.UsedAt = DateTimeOffset.UtcNow;
        row.User.PasswordHash = passwordHash;
        row.User.PasswordSalt = passwordSalt;
        row.User.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return PasswordResetCompletionStatus.Success;
    }
}
