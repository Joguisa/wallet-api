using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WalletApi.Application.Abstractions;
using WalletApi.Application.Movements.GetHistory;
using WalletApi.Domain.Transfers;
using WalletApi.Domain.Wallets;
using WalletApi.Infrastructure.Authentication;
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
        services.AddScoped<IMovementQueries, MovementQueries>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<DemoUserOptions>(configuration.GetSection(DemoUserOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<ICredentialVerifier, CredentialVerifier>();

        return services;
    }
}
