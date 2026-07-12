using Microsoft.AspNetCore.Mvc;
using WalletApi.Application.Abstractions;
using WalletApi.Application.Wallets;
using WalletApi.Application.Wallets.Create;
using WalletApi.Application.Wallets.GetById;

namespace WalletApi.Api.Endpoints;

public static class WalletEndpoints
{
    public static void MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wallets").WithTags("Wallets");

        group.MapPost("/", CreateWallet)
            .WithSummary("Creates a wallet")
            .Produces<WalletResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}", GetWallet)
            .WithName("GetWallet")
            .WithSummary("Gets a wallet by id")
            .Produces<WalletResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateWallet(
        [FromBody] CreateWalletCommand command,
        IRequestHandler<CreateWalletCommand, WalletResponse> handler,
        CancellationToken cancellationToken)
    {
        var wallet = await handler.HandleAsync(command, cancellationToken);

        return Results.CreatedAtRoute("GetWallet", new { id = wallet.Id }, wallet);
    }

    private static async Task<IResult> GetWallet(
        int id,
        IRequestHandler<GetWalletQuery, WalletResponse> handler,
        CancellationToken cancellationToken)
    {
        var wallet = await handler.HandleAsync(new GetWalletQuery(id), cancellationToken);

        return Results.Ok(wallet);
    }
}
