using practise.Domain.Models;

namespace practise.Application.Interfaces;

public interface ITransactionRepository
{
    Task AddTransactionAsync(Transaction transaction);
    Task<bool> IsTransactionExistsAsync(string referenceNumber, DateTime date);
    Task<bool> MarkTransactionAsProcessedAsync(string referenceNumber, DateTime date);
    Task<bool> IsTransactionProcessedAsync(string referenceNumber, DateTime date);
}
