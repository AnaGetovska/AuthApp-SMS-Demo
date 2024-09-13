using Vonage.Messaging;
using Vonage.Request;
using Vonage;
using Vonage.Verify;
using Microsoft.AspNetCore.Mvc;

namespace Sms2FAuthApp.Services
{
    public class SmsService
    {
        private readonly IConfiguration _configuration;

        public SmsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<VerifyResponse> SendVerification(string phoneNumber)
        {
            var apiKey = _configuration["Vonage:ApiKey"];
            var apiSecret = _configuration["Vonage:ApiSecret"];
            var senderName = _configuration["Vonage:SenderName"] ?? "Demo App";

            var credentials = Credentials.FromApiKeyAndSecret(apiKey, apiSecret);
            var client = new VonageClient(credentials);

            var request = new VerifyRequest
            {
                Number = phoneNumber,
                Brand = senderName,
                CodeLength = 4,
                WorkflowId = VerifyRequest.Workflow.SMS_TTS_TTS
            };

            var response = await client.VerifyClient.VerifyRequestAsync(request);
            return response;
        }

        public async Task<VerifyCheckResponse> CheckVerification(string requestId, string code)
        {
            var apiKey = _configuration["Vonage:ApiKey"];
            var apiSecret = _configuration["Vonage:ApiSecret"];

            var credentials = Credentials.FromApiKeyAndSecret(apiKey, apiSecret);
            var client = new VonageClient(credentials);

            var response = await client.VerifyClient.VerifyCheckAsync(new VerifyCheckRequest
            {
                RequestId = requestId, // The request ID returned from the verification request
                Code = code // The code entered by the user
            });

            return response;
        }
    }
}
