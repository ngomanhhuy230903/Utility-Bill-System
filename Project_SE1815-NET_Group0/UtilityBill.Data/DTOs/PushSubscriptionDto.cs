namespace UtilityBill.Data.DTOs
{
    public class PushSubscriptionDto
    {
        public string Endpoint { get; set; } = null!;
        public string P256Dh { get; set; } = null!;
        public string Auth { get; set; } = null!;
    }

    public class PushNotificationDto
    {
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string? Icon { get; set; }
        public string? Badge { get; set; }
        public string? Tag { get; set; }
        public string? Data { get; set; }
        public List<string>? UserIds { get; set; } // If null, send to all users
    }

    public class NotificationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
    }
} 