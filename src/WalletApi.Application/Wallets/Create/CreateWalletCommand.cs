namespace WalletApi.Application.Wallets.Create;

public sealed record CreateWalletCommand(
    string DocumentId,
    string OwnerName,
    decimal InitialBalance);
