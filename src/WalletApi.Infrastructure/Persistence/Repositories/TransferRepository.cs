using Microsoft.EntityFrameworkCore;
using WalletApi.Domain.Transfers;

namespace WalletApi.Infrastructure.Persistence.Repositories;

internal sealed class TransferRepository : ITransferRepository
{
    private readonly WalletDbContext _context;

    public TransferRepository(WalletDbContext context)
    {
        _context = context;
    }

    public Task<Transfer?> GetByIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default) =>
        _context.Transfers.FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);

    public void Add(Transfer transfer) => _context.Transfers.Add(transfer);
}
