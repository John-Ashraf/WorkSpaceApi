using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using WorkSpaceApi.Helpers;

namespace WorkSpaceApi.Services
{
    public class SMSService : ISMSService
    {
        private readonly TwillioSettings _settings;
        public SMSService(IOptions<TwillioSettings> twillioSettings)
        {
            _settings= twillioSettings.Value;
        }
        public MessageResource Send(string mobileNumber, string body)
        {
            TwilioClient.Init(_settings.AccountSID, _settings.AuthToken);

            var result=MessageResource.Create(
                body:body,
                from:new Twilio.Types.PhoneNumber(_settings.TwilioPhoneNumber),
                to:mobileNumber
                );
            return result; 
        }
    }
}
