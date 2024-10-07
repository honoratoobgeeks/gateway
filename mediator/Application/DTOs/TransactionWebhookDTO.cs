namespace Application.DTOs
{
    public class TransactionWebhookDTO
    {
        public static string Exchanger { get; set; } = "TransactionWebhook";
        public string Data { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Guid TransactionId { get; set; }
        public string EventType { get; set; } = "";
        public string SourceIP { get; set; } = "";
        public Dictionary<string, string> Headers { get; set; }
        public string Signature { get; set; } = "";
        public int Retries { get; set; } = 0;
    }
}