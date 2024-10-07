namespace Application.DTOs
{
    public class SmsDTO
    {
        public Guid Id { get; set; }
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public string Message { get; set; } = "";
        public static string Exchanger { get; set; } = "SmsException";
        public string Log { get; set; } = "";
        public DateTime SentAt { get; set; }


    }
}
