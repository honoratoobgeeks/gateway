namespace Application.DTOs
{
    public class SmsWebhookDTO
    {
        public static string Exchanger { get; set; } = "SmsWebhook";
        public string Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; }
        public string SourceIP { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Signature { get; set; }
        public int Retries { get; set; } = 0;
    }
}