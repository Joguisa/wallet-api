using WalletApi.Domain.Shared;

namespace WalletApi.Domain.Movements;

public sealed class Movement
{
    public int Id { get; private set; }
    public int WalletId { get; private set; }
    public int TransferId { get; private set; }
    public Money Amount { get; private set; }
    public MovementType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Movement()
    {
        Amount = null!;
    }

    internal Movement(int walletId, MovementType type, Money amount, DateTime createdAt)
    {
        WalletId = walletId;
        Type = type;
        Amount = amount;
        CreatedAt = createdAt;
    }
}
