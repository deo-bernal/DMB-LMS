using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dmb.Lms.Model.Dtos.Auth;
using Dmb.Lms.Service.Interface.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dmb.Lms.Api.Controllers.AuthControllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginWithJwtAsync(model, cancellationToken);
        return result.Status switch
        {
            AuthTokenLoginStatus.AccountBlocked => StatusCode(StatusCodes.Status403Forbidden, new { message = result.BlockReason }),
            AuthTokenLoginStatus.Success => Ok(new
            {
                token = result.AccessToken,
                locations = result.Locations,
                currentLocationId = result.CurrentLocationId,
                firstName = result.FirstName
            }),
            _ => Unauthorized()
        };
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
        var user = await _authService.GetLoggedInUserAsync(userId, cancellationToken);
        return user is null ? Unauthorized() : Ok(new { firstName = user.FirstName, lastName = user.LastName, email = user.Email });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Guid? userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed) ? parsed : null;
        var workflow = await _authService.LogoutAsync(
            User.Identity?.Name,
            User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value,
            User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value,
            userId, cancellationToken);
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        return Ok(new { message = workflow.Message, username = workflow.Username });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request, CancellationToken cancellationToken)
    {
        var status = await _authService.RequestPasswordResetAsync(request, cancellationToken);
        if (status == ForgotPasswordRequestStatus.EmailServiceUnavailable)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Unable to send the reset email. Please try again later." });
        }

        return Ok(new { message = "If that email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request, CancellationToken cancellationToken)
    {
        var status = await _authService.CompletePasswordResetAsync(request, cancellationToken);
        return status switch
        {
            PasswordResetCompletionStatus.PasswordMismatch => BadRequest(new { message = "Password and confirmation do not match." }),
            PasswordResetCompletionStatus.InvalidOrExpiredToken => BadRequest(new { message = "Reset link is invalid or has expired." }),
            _ => Ok(new { message = "Password reset successfully." })
        };
    }
}
