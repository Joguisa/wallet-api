using WalletApi.Application.Abstractions;
using WalletApi.Application.Movements.GetHistory;

namespace WalletApi.Api.Endpoints;

public static class MovementEndpoints
{
    public static void MapMovementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/wallets/{walletId:int}/movements").WithTags("Movements");

        group.MapGet("/", GetHistory)
            .WithSummary("Gets the paginated movement history of a wallet (newest first)")
            .Produces<MovementHistoryResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetHistory(
        int walletId,
        IRequestHandler<GetMovementHistoryQuery, MovementHistoryResponse> handler,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = GetMovementHistoryQuery.DefaultPageSize)
    {
        var history = await handler.HandleAsync(
            new GetMovementHistoryQuery(walletId, page, pageSize), cancellationToken);

        return Results.Ok(history);
    }
}
