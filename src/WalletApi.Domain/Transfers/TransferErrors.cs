using WalletApi.Domain.Shared;

namespace WalletApi.Domain.Transfers;

public sealed class SelfTransferNotAllowedException : DomainException
{
    public SelfTransferNotAllowedException()
        : base("SELF_TRANSFER_NOT_ALLOWED", "Source and destination wallets must be different.")
    {
    }
}

public sealed class DuplicateIdempotencyKeyException : DomainException
{
    public DuplicateIdempotencyKeyException(Guid idempotencyKey)
        : base("DUPLICATE_IDEMPOTENCY_KEY", $"A transfer with idempotency key '{idempotencyKey}' already exists.")
    {
    }
}
