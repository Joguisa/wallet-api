namespace WalletApi.Domain.Shared;

public sealed record Money
{
    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    public static Money Zero { get; } = new(0m);

    public static Money From(decimal amount)
    {
        if (amount < 0m)
        {
            throw new InvalidAmountException($"Amount cannot be negative, got {amount}.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new InvalidAmountException($"Amount cannot have more than two decimal places, got {amount}.");
        }

        return new Money(amount);
    }

    public bool IsPositive => Amount > 0m;

    public Money Add(Money other) => new(Amount + other.Amount);

    public Money Subtract(Money other)
    {
        if (other.Amount > Amount)
        {
            throw new InvalidAmountException(
                $"Cannot subtract {other.Amount} from {Amount}: the result would be negative.");
        }

        return new(Amount - other.Amount);
    }

    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;
    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;
    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;
    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;

    public override string ToString() => Amount.ToString("0.00");
}
