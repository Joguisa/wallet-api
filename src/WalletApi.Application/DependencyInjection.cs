using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WalletApi.Application.Abstractions;
using WalletApi.Application.Wallets;
using WalletApi.Application.Wallets.Create;
using WalletApi.Application.Wallets.GetById;

namespace WalletApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddScoped<IRequestHandler<CreateWalletCommand, WalletResponse>, CreateWalletHandler>();
        services.AddScoped<IRequestHandler<GetWalletQuery, WalletResponse>, GetWalletHandler>();

        return services;
    }
}
