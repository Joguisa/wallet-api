using WalletApi.Domain.Transfers;

namespace WalletApi.Application.Transfers;

public sealed record TransferResponse(
    int TransferId,
    int SourceWalletId,
    int DestinationWalletId,
    decimal Amount,
    DateTime CreatedAt)
{
    public static TransferResponse FromDomain(Transfer transfer) => new(
        transfer.Id,
        transfer.SourceWalletId,
        transfer.DestinationWalletId,
        transfer.Amount.Amount,
        transfer.CreatedAt);
}
