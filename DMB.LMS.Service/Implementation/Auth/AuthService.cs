using Dmb.Lms.Data.Repository.Interface.Auth;
using Dmb.Lms.Model.Dtos.Auth;
using Dmb.Lms.Service.Interface.Auth;

namespace Dmb.Lms.Service.Implementation.Auth;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordResetRepository _passwordResetRepository;

    public AuthService(IAuthRepository authRepository, IPasswordResetRepository passwordResetRepository)
    {
        _authRepository = authRepository;
        _passwordResetRepository = passwordResetRepository;
    }

    public Task<AuthTokenLoginResult> LoginWithJwtAsync(LoginDto model, CancellationToken cancellationToken = default)
        => _authRepository.LoginAndIssueJwtAsync(model, cancellationToken);

    public Task<LogoutWorkflowResult> LogoutAsync(string? username, string? jti, string? expClaim, Guid? userId, CancellationToken cancellationToken = default)
        => _authRepository.LogoutAsync(username, jti, expClaim, userId, cancellationToken);

    public Task<ForgotPasswordRequestStatus> RequestPasswordResetAsync(ForgotPasswordDto request, CancellationToken cancellationToken = default)
        => _passwordResetRepository.RequestPasswordResetAsync(request, cancellationToken);

    public Task<PasswordResetCompletionStatus> CompletePasswordResetAsync(ResetPasswordDto request, CancellationToken cancellationToken = default)
        => _passwordResetRepository.CompletePasswordResetAsync(request, cancellationToken);

    public Task<bool> IsJtiRevokedAsync(string jti, CancellationToken cancellationToken = default)
        => _authRepository.IsJtiRevokedAsync(jti, cancellationToken);

    public Task<LoggedInUserDto?> GetLoggedInUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => _authRepository.GetLoggedInUserAsync(userId, cancellationToken);
}

public class RegistrationService : IRegistrationService
{
    private readonly IRegistrationRepository _repository;
    public RegistrationService(IRegistrationRepository repository) => _repository = repository;
    public Task<RegisterWithActivationOutcome> RegisterWithActivationAsync(RegisterDto request, CancellationToken cancellationToken = default)
        => _repository.RegisterWithActivationAsync(request, cancellationToken);
    public Task<ActivateAccountOutcome> ActivateAccountAsync(string? token, CancellationToken cancellationToken = default)
        => _repository.CompleteAccountActivationAsync(token, cancellationToken);
}
