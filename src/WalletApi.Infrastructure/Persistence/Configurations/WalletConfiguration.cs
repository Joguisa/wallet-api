using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Wallets;

namespace WalletApi.Infrastructure.Persistence.Configurations;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets", table =>
            table.HasCheckConstraint("CK_Wallets_Balance_NonNegative", "[Balance] >= 0"));

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedOnAdd();

        builder.Property(w => w.DocumentId)
            .HasConversion(documentId => documentId.Value, value => DocumentId.From(value))
            .HasMaxLength(DocumentId.Length)
            .IsRequired();

        builder.HasIndex(w => w.DocumentId)
            .IsUnique()
            .HasDatabaseName("UX_Wallets_DocumentId");

        builder.Property(w => w.OwnerName)
            .HasMaxLength(Wallet.MaxOwnerNameLength)
            .IsRequired();

        builder.Property(w => w.Balance)
            .HasConversion(money => money.Amount, value => Money.From(value))
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.UpdatedAt).IsRequired();

        builder.Property(w => w.RowVersion)
            .IsRowVersion();
    }
}
