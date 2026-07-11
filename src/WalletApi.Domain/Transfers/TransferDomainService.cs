using WalletApi.Domain.Shared;
using WalletApi.Domain.Wallets;

namespace WalletApi.Domain.Transfers;

public sealed class TransferDomainService
{
    public Transfer Execute(Wallet source, Wallet destination, Money amount, Guid idempotencyKey)
    {
        var transfer = Transfer.Create(source, destination, amount, idempotencyKey);

        source.Debit(amount);
        destination.Credit(amount);

        return transfer;
    }
}
