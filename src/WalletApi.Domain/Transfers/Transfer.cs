using WalletApi.Domain.Movements;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Wallets;

namespace WalletApi.Domain.Transfers;

public sealed class Transfer
{
    private readonly List<Movement> _movements = new();

    public int Id { get; private set; }
    public int SourceWalletId { get; private set; }
    public int DestinationWalletId { get; private set; }
    public Money Amount { get; private set; }
    public Guid IdempotencyKey { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Movement> Movements => _movements.AsReadOnly();

    private Transfer()
    {
        Amount = null!;
    }

    private Transfer(Wallet source, Wallet destination, Money amount, Guid idempotencyKey)
    {
        SourceWalletId = source.Id;
        DestinationWalletId = destination.Id;
        Amount = amount;
        IdempotencyKey = idempotencyKey;
        CreatedAt = DateTime.UtcNow;

        _movements.Add(new Movement(source.Id, MovementType.Debit, amount, CreatedAt));
        _movements.Add(new Movement(destination.Id, MovementType.Credit, amount, CreatedAt));
    }

    public static Transfer Create(Wallet source, Wallet destination, Money amount, Guid idempotencyKey)
    {
        // Unsaved wallets both have Id 0, so identity is checked by reference as well.
        if (ReferenceEquals(source, destination) || (source.Id != 0 && source.Id == destination.Id))
        {
            throw new SelfTransferNotAllowedException();
        }

        if (!amount.IsPositive)
        {
            throw new InvalidAmountException("Transfer amount must be greater than zero.");
        }

        return new Transfer(source, destination, amount, idempotencyKey);
    }
}
