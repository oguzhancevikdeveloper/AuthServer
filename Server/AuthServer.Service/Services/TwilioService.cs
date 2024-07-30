using AuthServer.Core.Services;
using AuthServer.Shared.Helpers;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AuthServer.Service.Services;

public class TwilioService : ITwilioService
{
    private readonly TwilioOptions _options;

    public TwilioService(IOptions<TwilioOptions> options)
    {
        _options = options.Value;
        TwilioClient.Init(_options.AccountSid, _options.AuthToken);
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        var messageResource = await MessageResource.CreateAsync(
            body: message,
            from: new PhoneNumber(_options.PhoneNumber),
            to: new PhoneNumber(phoneNumber)
        );
    }
}