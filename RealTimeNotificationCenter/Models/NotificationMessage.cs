namespace RealTimeNotificationCenter.Models
{
    public class NotificationMessage 
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? SenderUserName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
