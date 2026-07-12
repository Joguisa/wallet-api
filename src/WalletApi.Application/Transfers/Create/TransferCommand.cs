namespace WalletApi.Application.Transfers.Create;

public sealed record TransferCommand(
    int SourceWalletId,
    int DestinationWalletId,
    decimal Amount,
    Guid IdempotencyKey);
