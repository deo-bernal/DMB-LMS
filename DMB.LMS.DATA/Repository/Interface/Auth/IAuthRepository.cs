using Dmb.Lms.Model.Dtos.Auth;

namespace Dmb.Lms.Data.Repository.Interface.Auth;

public interface IAuthRepository
{
    Task<AuthTokenLoginResult> LoginAndIssueJwtAsync(LoginDto model, CancellationToken cancellationToken = default);
    Task<LoggedInUserDto?> GetLoggedInUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LogoutWorkflowResult> LogoutAsync(string? username, string? jti, string? expClaim, Guid? userId, CancellationToken cancellationToken = default);
    Task<bool> IsJtiRevokedAsync(string jti, CancellationToken cancellationToken = default);
    (string PasswordHash, string PasswordSalt) CreatePasswordHash(string password);
    Task<IReadOnlyList<LocationMembershipDto>> GetUserLocationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AuthTokenLoginResult> IssueJwtForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(LoggedInUserDto? User, string? Error)> UpdateOwnProfileAsync(Guid userId, UpdateOwnProfileDto dto, CancellationToken cancellationToken = default);
}

public interface IRegistrationRepository
{
    Task<RegisterWithActivationOutcome> RegisterWithActivationAsync(RegisterDto request, CancellationToken cancellationToken = default);
    Task<ActivateAccountOutcome> CompleteAccountActivationAsync(string? token, CancellationToken cancellationToken = default);
}

public interface IPasswordResetRepository
{
    Task<ForgotPasswordRequestStatus> RequestPasswordResetAsync(ForgotPasswordDto request, CancellationToken cancellationToken = default);
    Task<PasswordResetCompletionStatus> CompletePasswordResetAsync(ResetPasswordDto request, CancellationToken cancellationToken = default);
}
