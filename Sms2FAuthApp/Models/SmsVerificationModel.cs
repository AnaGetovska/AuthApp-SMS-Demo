namespace Sms2FAuthApp.Models
{
    public record SmsVerificationModel
    {
        public string RequestId { get; set; }
        public string Code { get; set; }
    }
}
