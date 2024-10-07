using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISmsService
    {
        Task SendSmsAsync(string toPhoneNumber, string message);
        Task<Guid> PublishSmsAsync(SmsDTO smsDto);        
        Task<List<SmsDTO>> SearchSmsAsync(string query);
        Task HandleWebhookAsync(string webhookData, string sourceIp, string eventType, Dictionary<string, string> headers);
       
    }
}
