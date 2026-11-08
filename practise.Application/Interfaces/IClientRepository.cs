namespace practise.Application.Interfaces;

public interface IClientRepository
{
    Task<bool> AddBalanceAsync(string accountNumber, decimal amount);
}