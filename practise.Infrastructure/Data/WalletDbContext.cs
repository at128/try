using Microsoft.EntityFrameworkCore;
using practise.Domain.Models;

namespace practise.Infrastructure.Data;

public class WalletDbContext(DbContextOptions<WalletDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new Configuration.ClientConfiguration());
    }
}