using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoGarageManager.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TwilioSmsService> _logger;

        public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var accountSid = _configuration["Twilio:AccountSid"];
                var authToken = _configuration["Twilio:AuthToken"];
                var fromNumber = _configuration["Twilio:PhoneNumber"];

                // If Twilio credentials are not configured, log the SMS instead (demo mode)
                if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
                {
                    _logger.LogInformation($"[SMS DEMO MODE] To: {phoneNumber}, Message: {message}");
                    return true;
                }

                // Initialize Twilio client
                Twilio.TwilioClient.Init(accountSid, authToken);

                // Send SMS
                var result = await Twilio.Rest.Api.V2010.Account.MessageResource.CreateAsync(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(fromNumber),
                    to: new Twilio.Types.PhoneNumber(phoneNumber)
                );

                _logger.LogInformation($"SMS sent successfully. SID: {result.Sid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending SMS to {phoneNumber}: {ex.Message}");
                return false;
            }
        }
    }
}
