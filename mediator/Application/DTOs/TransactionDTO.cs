namespace Application.DTOs
{
    public class TransactionDTO
    {
        public string Type { get; set; } = "";
        public string Data { get; set; } = "";
        public string Endpoint { get; set; } = "";
        public string EndpointType { get; set; } = "";
        public static string Exchanger { get; set; } = "TransactionException";
        public string Log { get; set; } = "";

    }
}
