using Microsoft.EntityFrameworkCore;
using WalletApi.Application.Abstractions;
using WalletApi.Domain.Movements;
using WalletApi.Domain.Transfers;
using WalletApi.Domain.Wallets;

namespace WalletApi.Infrastructure.Persistence;

public sealed class WalletDbContext : DbContext, IUnitOfWork
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options)
        : base(options)
    {
    }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Movement> Movements => Set<Movement>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException?.Message.Contains("UX_Transfers_IdempotencyKey") == true)
        {
            var duplicateKey = ChangeTracker.Entries<Transfer>()
                .Select(e => e.Entity.IdempotencyKey)
                .FirstOrDefault();

            throw new DuplicateIdempotencyKeyException(duplicateKey);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WalletDbContext).Assembly);
    }
}
