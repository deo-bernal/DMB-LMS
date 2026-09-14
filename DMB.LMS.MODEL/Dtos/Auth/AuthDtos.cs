namespace Dmb.Lms.Model.Dtos.Auth;

public enum AuthTokenLoginStatus { InvalidCredentials, AccountBlocked, Success }
public enum ForgotPasswordRequestStatus { Ok, EmailServiceUnavailable }
public enum PasswordResetCompletionStatus { Success, PasswordMismatch, InvalidOrExpiredToken }
public enum RegisterWithActivationOutcome { Success, DuplicateEmail, AgencyNotFound, ActivationEmailSendFailed }
public enum ActivateAccountOutcome { Success, InvalidOrExpiredToken }

public class LoginDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class RegisterDto
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }
    public string? ContactNumber { get; set; }
    public string AgencySlug { get; set; } = "dmb";
    public string Role { get; set; } = "parent";
}

public class ActivateAccountDto
{
    public required string Token { get; set; }
}

public class ForgotPasswordDto
{
    public required string Email { get; set; }
}

public class ResetPasswordDto
{
    public required string Token { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class AuthTokenLoginResult
{
    public AuthTokenLoginStatus Status { get; set; }
    public string? AccessToken { get; set; }
    public string? BlockReason { get; set; }
    public IReadOnlyList<LocationMembershipDto> Locations { get; set; } = [];
    public Guid? CurrentLocationId { get; set; }
    public string? FirstName { get; set; }
}

public class LogoutWorkflowResult
{
    public string? Username { get; set; }
    public string Message { get; set; } = "Signed out.";
}

public class LocationMembershipDto
{
    public Guid LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "parent";
}

public class LoggedInUserDto
{
    public Guid UserId { get; set; }
    public Guid AgencyId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Activated { get; set; }
    public bool IsSuperAdmin { get; set; }
}
