namespace WalletApi.Application.Transfers.Create;

public sealed record TransferRequest(
    int SourceWalletId,
    int DestinationWalletId,
    decimal Amount);
