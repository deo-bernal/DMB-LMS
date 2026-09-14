using Dmb.Lms.Model.Dtos.Auth;
using Dmb.Lms.Service.Interface.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dmb.Lms.Api.Controllers.AuthControllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    public RegistrationController(IRegistrationService registrationService) => _registrationService = registrationService;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto request, CancellationToken cancellationToken)
    {
        var outcome = await _registrationService.RegisterWithActivationAsync(request, cancellationToken);
        return outcome switch
        {
            RegisterWithActivationOutcome.DuplicateEmail => BadRequest(new { message = "An account with this email already exists." }),
            RegisterWithActivationOutcome.AgencyNotFound => BadRequest(new { message = "Agency was not found. Apply the SQL seed first." }),
            RegisterWithActivationOutcome.ActivationEmailSendFailed => StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Unable to send the activation email. Please try again later." }),
            _ => Ok(new { message = "Registration successful. The first agency user can sign in immediately; later users must activate by email." })
        };
    }

    [HttpPost("activate")]
    [AllowAnonymous]
    public async Task<IActionResult> Activate([FromBody] ActivateAccountDto request, CancellationToken cancellationToken)
    {
        var outcome = await _registrationService.ActivateAccountAsync(request.Token, cancellationToken);
        return outcome == ActivateAccountOutcome.Success
            ? Ok(new { message = "Your account has been activated. You can sign in now." })
            : BadRequest(new { message = "Activation link is invalid or has expired." });
    }
}
