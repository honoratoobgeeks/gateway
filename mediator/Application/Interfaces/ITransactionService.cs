using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITransactionService
    {
        Task<Guid> CreateTransactionAsync(TransactionDTO transactionDto);
        Task HandleWebhookAsync(Guid transactionId, string webhookData, string sourceIp, string eventType, Dictionary<string, string> headers);

        Task<List<TransactionDTO>> SearchTransactionsAsync(string query);
       
    }
}
