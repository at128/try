using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using practise.Domain.Models;

namespace practise.Infrastructure.Data.Configuration;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Amount).IsRequired();
        builder.Property(t => t.TrasferTo).IsRequired();
        builder.Property(t => t.BankName).IsRequired();
        builder.Property(t => t.Date).IsRequired();
        builder.HasIndex(t=>t.Date);
    }
}