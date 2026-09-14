namespace Dmb.Lms.Model.Abstractions;

public interface IActivationEmailSender
{
    Task SendAccountActivationEmailAsync(string toEmail, string activationLink, CancellationToken cancellationToken = default);
}

public interface IPasswordResetEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
}

public interface IExternalLoginEmailSender
{
    Task SendExternalLoginCodeEmailAsync(string toEmail, string code, CancellationToken cancellationToken = default);
}
