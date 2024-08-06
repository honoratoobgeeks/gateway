using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infra.Data.Context
{
    public class MediatorDbContext : DbContext
    {
        public DbSet<Transaction> Transaction { get; set; }

        public MediatorDbContext(DbContextOptions<MediatorDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .HasKey(p => p.Id);

        }
    }
}
