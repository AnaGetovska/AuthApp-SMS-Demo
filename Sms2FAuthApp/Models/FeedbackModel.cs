namespace Sms2FAuthApp.Models
{
    public class FeedbackModel
    {
        public string AlertType { get; set; } = string.Empty;
        public string AlertMessage { get; set; } = string.Empty;
        public bool HasFeedback { get; set; }
    }
}
