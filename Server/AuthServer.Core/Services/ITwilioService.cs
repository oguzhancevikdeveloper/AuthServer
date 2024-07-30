namespace AuthServer.Core.Services;

public interface ITwilioService
{
    Task SendSmsAsync(string phoneNumber, string message);
}
