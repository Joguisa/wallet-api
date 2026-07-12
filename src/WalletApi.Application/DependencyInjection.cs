using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WalletApi.Application.Abstractions;
using WalletApi.Application.Authentication.Login;
using WalletApi.Application.Movements.GetHistory;
using WalletApi.Application.Transfers;
using WalletApi.Application.Transfers.Create;
using WalletApi.Application.Wallets;
using WalletApi.Application.Wallets.Create;
using WalletApi.Application.Wallets.GetById;
using WalletApi.Domain.Transfers;

namespace WalletApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddSingleton<TransferDomainService>();

        services.AddScoped<IRequestHandler<CreateWalletCommand, WalletResponse>, CreateWalletHandler>();
        services.AddScoped<IRequestHandler<GetWalletQuery, WalletResponse>, GetWalletHandler>();
        services.AddScoped<IRequestHandler<TransferCommand, TransferResponse>, TransferHandler>();
        services.AddScoped<IRequestHandler<GetMovementHistoryQuery, MovementHistoryResponse>, GetMovementHistoryHandler>();
        services.AddScoped<IRequestHandler<LoginCommand, LoginResponse>, LoginHandler>();

        return services;
    }
}
