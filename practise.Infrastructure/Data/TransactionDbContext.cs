
using Microsoft.EntityFrameworkCore;
using practise.Domain.Models;

namespace practise.Infrastructure.Data;

public class TransactionDbContext(DbContextOptions<TransactionDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new Configuration.TransactionConfiguration());
    }
}