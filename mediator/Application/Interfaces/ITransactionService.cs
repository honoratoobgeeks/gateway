using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITransactionService
    {
        Task<Guid> CreateTransactionAsync(TransactionDTO transactionDto);
        Task HandleWebhookAsync(Guid transactionId, string webhookData);

        Task<List<TransactionDTO>> SearchTransactionsAsync(string query);
       
    }
}
