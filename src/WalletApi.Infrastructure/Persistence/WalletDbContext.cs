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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WalletDbContext).Assembly);
    }
}
