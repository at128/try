using Microsoft.EntityFrameworkCore;
using practise.Application.Interfaces;
using practise.Infrastructure.Data;

namespace practise.Infrastructure.Repsitories;

public class ClientRepository:IClientRepository
{
    private readonly WalletDbContext _context;

    public ClientRepository(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddBalanceAsync(string accountNumber, decimal amount)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);
        if (client == null)
        {
            return false;
        }

        client.Balance += amount;
        await _context.SaveChangesAsync();
        return true;
    }
}