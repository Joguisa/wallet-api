using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletApi.Domain.Movements;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Wallets;

namespace WalletApi.Infrastructure.Persistence.Configurations;

internal sealed class MovementConfiguration : IEntityTypeConfiguration<Movement>
{
    public void Configure(EntityTypeBuilder<Movement> builder)
    {
        builder.ToTable("Movements", table =>
            table.HasCheckConstraint("CK_Movements_Amount_Positive", "[Amount] > 0"));

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();

        builder.Property(m => m.Amount)
            .HasConversion(money => money.Amount, value => Money.From(value))
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(m => m.Type)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(m => m.CreatedAt).IsRequired();

        builder.HasOne<Wallet>()
            .WithMany()
            .HasForeignKey(m => m.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.WalletId, m.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Movements_WalletId_CreatedAt");
    }
}
