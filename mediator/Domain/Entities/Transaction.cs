using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; }
        public string Data { get; set; }
        public string Endpoint { get; set; }
        public string EndpointType { get; set; }
        public string Exchanger { get; set; }
    }
}
