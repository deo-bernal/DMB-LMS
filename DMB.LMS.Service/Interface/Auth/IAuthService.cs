using Dmb.Lms.Model.Dtos.Auth;

namespace Dmb.Lms.Service.Interface.Auth;

public interface IAuthService
{
    Task<AuthTokenLoginResult> LoginWithJwtAsync(LoginDto model, CancellationToken cancellationToken = default);
    Task<LogoutWorkflowResult> LogoutAsync(string? username, string? jti, string? expClaim, Guid? userId, CancellationToken cancellationToken = default);
    Task<ForgotPasswordRequestStatus> RequestPasswordResetAsync(ForgotPasswordDto request, CancellationToken cancellationToken = default);
    Task<PasswordResetCompletionStatus> CompletePasswordResetAsync(ResetPasswordDto request, CancellationToken cancellationToken = default);
    Task<bool> IsJtiRevokedAsync(string jti, CancellationToken cancellationToken = default);
    Task<LoggedInUserDto?> GetLoggedInUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(LoggedInUserDto? User, string? Error)> UpdateOwnProfileAsync(Guid userId, UpdateOwnProfileDto dto, CancellationToken cancellationToken = default);
}

public interface IRegistrationService
{
    Task<RegisterWithActivationOutcome> RegisterWithActivationAsync(RegisterDto request, CancellationToken cancellationToken = default);
    Task<ActivateAccountOutcome> ActivateAccountAsync(string? token, CancellationToken cancellationToken = default);
}
