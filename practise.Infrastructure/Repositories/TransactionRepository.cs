using Microsoft.EntityFrameworkCore;
using practise.Application.Interfaces;
using practise.Domain.Models;
using practise.Infrastructure.Data;

namespace practise.Infrastructure.Repsitories;

public class TransactionRepository(ShardResolver shardResolver) : ITransactionRepository
{
    private readonly ShardResolver _shardResolver = shardResolver;

    public async Task AddTransactionAsync(Transaction transaction)
    {
        var connectionString = _shardResolver.GetConnectionStringForKey(transaction.Date);
        using var _context = new TransactionDbContext(new DbContextOptionsBuilder<TransactionDbContext>().UseSqlServer(connectionString).Options);
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> IsTransactionExistsAsync(string referenceNumber, DateTime date)
    {
        var connectionString = _shardResolver.GetConnectionStringForKey(date);
        using var _context = new TransactionDbContext(new DbContextOptionsBuilder<TransactionDbContext>().UseSqlServer(connectionString).Options);
        return await _context.Transactions.AnyAsync(t => t.Id == referenceNumber);
    }

    public async Task<bool> MarkTransactionAsProcessedAsync(string referenceNumber, DateTime date)
    {
        var connectionString = _shardResolver.GetConnectionStringForKey(date);
        using var _context = new TransactionDbContext(new DbContextOptionsBuilder<TransactionDbContext>().UseSqlServer(connectionString).Options);
        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == referenceNumber);
        if (transaction != null)
        {
            transaction.IsProcessed = true;
            transaction.DateOfProcessing = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> IsTransactionProcessedAsync(string referenceNumber, DateTime date)
    {
        var connectionString = _shardResolver.GetConnectionStringForKey(date);
        using var _context = new TransactionDbContext(new DbContextOptionsBuilder<TransactionDbContext>().UseSqlServer(connectionString).Options);
        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == referenceNumber);
        return transaction?.IsProcessed ?? false;
    }

    
}