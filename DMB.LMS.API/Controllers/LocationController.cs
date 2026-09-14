using System.Security.Claims;
using Dmb.Lms.Data.Repository.Interface.Auth;
using Dmb.Lms.Api.Filters;
using Dmb.Lms.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dmb.Lms.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly IAuthRepository _auth;
    private readonly ILmsService _lms;

    public LocationController(IAuthRepository auth, ILmsService lms)
    {
        _auth = auth;
        _lms = lms;
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
        return Ok(await _auth.GetUserLocationsAsync(userId, cancellationToken));
    }

    [HttpGet("stats")]
    [ServiceFilter(typeof(LocationContextFilter))]
    public async Task<IActionResult> Stats(CancellationToken cancellationToken)
    {
        var (_, locationId, _) = HttpContext.RequireContext();
        return Ok(await _lms.GetStatsAsync(locationId, cancellationToken));
    }
}
