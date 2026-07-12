using Microsoft.AspNetCore.Mvc;
using WalletApi.Application.Abstractions;
using WalletApi.Application.Authentication.Login;

namespace WalletApi.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", Login)
            .WithSummary("Authenticates the demo user and returns a JWT")
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Login(
        [FromBody] LoginCommand command,
        IRequestHandler<LoginCommand, LoginResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(command, cancellationToken);

        return Results.Ok(response);
    }
}
