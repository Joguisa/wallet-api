using WalletApi.Domain.Shared;

namespace WalletApi.Domain.Wallets;

public sealed class Wallet
{
    public const int MaxOwnerNameLength = 100;

    public int Id { get; private set; }
    public DocumentId DocumentId { get; private set; }
    public string OwnerName { get; private set; }
    public Money Balance { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private Wallet()
    {
        DocumentId = null!;
        OwnerName = null!;
        Balance = null!;
    }

    private Wallet(DocumentId documentId, string ownerName, Money initialBalance)
    {
        DocumentId = documentId;
        OwnerName = ownerName;
        Balance = initialBalance;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static Wallet Create(DocumentId documentId, string ownerName, Money initialBalance)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
        {
            throw new InvalidOwnerNameException("Owner name cannot be empty.");
        }

        var trimmed = ownerName.Trim();

        if (trimmed.Length > MaxOwnerNameLength)
        {
            throw new InvalidOwnerNameException(
                $"Owner name cannot exceed {MaxOwnerNameLength} characters.");
        }

        return new Wallet(documentId, trimmed, initialBalance);
    }

    public void Debit(Money amount)
    {
        EnsurePositive(amount);

        if (Balance < amount)
        {
            throw new InsufficientFundsException(Id, Balance, amount);
        }

        Balance = Balance.Subtract(amount);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Credit(Money amount)
    {
        EnsurePositive(amount);

        Balance = Balance.Add(amount);
        UpdatedAt = DateTime.UtcNow;
    }

    private static void EnsurePositive(Money amount)
    {
        if (!amount.IsPositive)
        {
            throw new InvalidAmountException("Operation amount must be greater than zero.");
        }
    }
}
