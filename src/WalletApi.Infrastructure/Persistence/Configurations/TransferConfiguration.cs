using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Transfers;
using WalletApi.Domain.Wallets;

namespace WalletApi.Infrastructure.Persistence.Configurations;

internal sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfers", table =>
            table.HasCheckConstraint("CK_Transfers_Amount_Positive", "[Amount] > 0"));

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.Amount)
            .HasConversion(money => money.Amount, value => Money.From(value))
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.IdempotencyKey).IsRequired();

        builder.HasIndex(t => t.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("UX_Transfers_IdempotencyKey");

        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasOne<Wallet>()
            .WithMany()
            .HasForeignKey(t => t.SourceWalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Wallet>()
            .WithMany()
            .HasForeignKey(t => t.DestinationWalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Movements)
            .WithOne()
            .HasForeignKey(m => m.TransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Movements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
