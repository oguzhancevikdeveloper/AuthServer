namespace AuthServer.Core.Services;

public interface IMailService
{
    Task SendMailAsync(string to, string subject, string body, bool isBodyHtml = true);
    Task SendMailAsync(string[] tos, string subject, string body, bool isBodyHtml = true);


    Task SendEmailConfirmationLinkAsync(string to, string userId, string confirmToken);
    Task SendPasswordResetMailAsync(string to, string userId, string resetToken);
}
