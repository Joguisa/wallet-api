using Microsoft.EntityFrameworkCore;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Wallets;

namespace WalletApi.Infrastructure.Persistence.Repositories;

internal sealed class WalletRepository : IWalletRepository
{
    private readonly WalletDbContext _context;

    public WalletRepository(WalletDbContext context)
    {
        _context = context;
    }

    public Task<Wallet?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Wallets.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public Task<bool> ExistsByDocumentIdAsync(DocumentId documentId, CancellationToken cancellationToken = default) =>
        _context.Wallets.AnyAsync(w => w.DocumentId == documentId, cancellationToken);

    public void Add(Wallet wallet) => _context.Wallets.Add(wallet);
}
