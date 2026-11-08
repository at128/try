using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using practise.Domain.Models;

namespace practise.Infrastructure.Data.Configuration;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c=> c.AccountNumber).IsRequired();
        builder.Property(c=> c.Name).IsRequired();
    }
}