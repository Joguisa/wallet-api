using Microsoft.AspNetCore.Mvc;
using WalletApi.Application.Abstractions;
using WalletApi.Application.Transfers;
using WalletApi.Application.Transfers.Create;

namespace WalletApi.Api.Endpoints;

public static class TransferEndpoints
{
    public static void MapTransferEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transfers")
            .WithTags("Transfers")
            .RequireAuthorization();

        group.MapPost("/", Transfer)
            .WithSummary("Transfers balance between wallets (atomic debit + credit)")
            .WithDescription("Requires an Idempotency-Key header (GUID). Repeating a request with the same key returns the original transfer without executing again.")
            .Produces<TransferResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> Transfer(
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey,
        [FromBody] TransferRequest request,
        IRequestHandler<TransferCommand, TransferResponse> handler,
        CancellationToken cancellationToken)
    {
        var command = new TransferCommand(
            request.SourceWalletId,
            request.DestinationWalletId,
            request.Amount,
            idempotencyKey ?? Guid.Empty);

        var transfer = await handler.HandleAsync(command, cancellationToken);

        return Results.Ok(transfer);
    }
}
