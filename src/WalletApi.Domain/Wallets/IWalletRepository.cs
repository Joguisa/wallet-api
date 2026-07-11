using WalletApi.Domain.Shared;

namespace WalletApi.Domain.Wallets;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByDocumentIdAsync(DocumentId documentId, CancellationToken cancellationToken = default);
    void Add(Wallet wallet);
}
