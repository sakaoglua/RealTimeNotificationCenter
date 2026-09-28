namespace RealTimeNotificationCenter.Models
{
    public class PrivateNotificationRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        //public string SenderUserName { get; set; } = string.Empty;
    }
}
