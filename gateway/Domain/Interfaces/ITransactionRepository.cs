using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<Transaction> GetByIdAsync(Guid id);
        Task AddAsync(Transaction property);
        Task UpdateAsync(Transaction property);
        Task DeleteAsync(Guid id);
    }
}