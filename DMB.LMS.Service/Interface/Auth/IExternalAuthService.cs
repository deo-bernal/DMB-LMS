using Dmb.Lms.Model.Dtos.Auth;

namespace Dmb.Lms.Service.Interface.Auth;

public interface IExternalAuthService
{
    ExternalAuthStartResult Start(string provider, string? redirect, string? role, string callbackUrl);

    Task<string> HandleCallbackAsync(
        string provider,
        string? code,
        string? state,
        string? error,
        string? errorDescription,
        string callbackUrl,
        CancellationToken cancellationToken = default);

    Task<(bool Ok, string Message, int StatusCode)> CompleteAsync(
        ExternalAuthCompleteRequest request,
        CancellationToken cancellationToken = default);

    Task<ExternalAuthVerifyResult> VerifyAsync(
        ExternalAuthVerifyRequest request,
        CancellationToken cancellationToken = default);
}
