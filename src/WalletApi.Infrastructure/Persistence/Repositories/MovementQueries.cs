using Microsoft.EntityFrameworkCore;
using WalletApi.Application.Movements;
using WalletApi.Application.Movements.GetHistory;

namespace WalletApi.Infrastructure.Persistence.Repositories;

internal sealed class MovementQueries : IMovementQueries
{
    private readonly WalletDbContext _context;

    public MovementQueries(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<MovementResponse> Items, int TotalCount)> GetHistoryAsync(
        int walletId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Movements
            .AsNoTracking()
            .Where(m => m.WalletId == walletId);

        var totalCount = await query.CountAsync(cancellationToken);

        var movements = await query
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = movements.Select(MovementResponse.FromDomain).ToList();

        return (items, totalCount);
    }
}
