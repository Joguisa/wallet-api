using WalletApi.Domain.Wallets;

namespace WalletApi.Application.Wallets;

public sealed record WalletResponse(
    int Id,
    string DocumentId,
    string OwnerName,
    decimal Balance,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static WalletResponse FromDomain(Wallet wallet) => new(
        wallet.Id,
        wallet.DocumentId.Value,
        wallet.OwnerName,
        wallet.Balance.Amount,
        wallet.CreatedAt,
        wallet.UpdatedAt);
}
