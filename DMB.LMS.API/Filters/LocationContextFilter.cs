using System.Security.Claims;
using Dmb.Lms.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dmb.Lms.Api.Filters;

public sealed class LocationContextFilter : IAsyncActionFilter
{
    public const string HeaderName = "X-Location-Id";
    public const string ItemKey = "LmsLocationContext";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdValue = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var header = context.HttpContext.Request.Headers[HeaderName].FirstOrDefault();
        if (!Guid.TryParse(header, out var locationId))
        {
            context.Result = new BadRequestObjectResult(new { message = "X-Location-Id header is required." });
            return;
        }

        var lms = context.HttpContext.RequestServices.GetRequiredService<ILmsService>();
        var membership = await lms.GetMembershipAsync(userId, locationId, context.HttpContext.RequestAborted);
        if (membership is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        context.HttpContext.Items[ItemKey] = membership;
        context.HttpContext.Items["LmsUserId"] = userId;
        await next();
    }
}

public static class LocationContextExtensions
{
    public static (Guid UserId, Guid LocationId, string Role) RequireContext(this HttpContext http)
    {
        var membership = http.Items[LocationContextFilter.ItemKey] as Model.Dtos.Auth.LocationMembershipDto
            ?? throw new InvalidOperationException("Location context missing.");
        var userId = (Guid)http.Items["LmsUserId"]!;
        return (userId, membership.LocationId, membership.Role);
    }
}
