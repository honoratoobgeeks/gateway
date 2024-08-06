using Domain.Entities;
using Domain.Interfaces;
using Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infra.Data.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly MediatorDbContext _context;

        public TransactionRepository(MediatorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transaction.ToListAsync();
        }

        public async Task<Transaction> GetByIdAsync(Guid id)
        {
            return await _context.Transaction.FindAsync(id);
        }

        public async Task AddAsync(Transaction Transaction)
        {
            await _context.Transaction.AddAsync(Transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Transaction Transaction)
        {
            _context.Transaction.Update(Transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var Transaction = await GetByIdAsync(id);
            _context.Transaction.Remove(Transaction);
            await _context.SaveChangesAsync();
        }
    }
}