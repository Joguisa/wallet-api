using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WalletApi.Application.Abstractions;
using WalletApi.Domain.Transfers;
using WalletApi.Domain.Wallets;
using WalletApi.Infrastructure.Persistence;
using WalletApi.Infrastructure.Persistence.Repositories;

namespace WalletApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<WalletDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Database")));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<WalletDbContext>());
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();

        return services;
    }
}
