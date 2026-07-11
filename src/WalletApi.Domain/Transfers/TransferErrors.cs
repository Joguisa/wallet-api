using WalletApi.Domain.Shared;

namespace WalletApi.Domain.Transfers;

public sealed class SelfTransferNotAllowedException : DomainException
{
    public SelfTransferNotAllowedException()
        : base("SELF_TRANSFER_NOT_ALLOWED", "Source and destination wallets must be different.")
    {
    }
}
