using WalletApi.Domain.Movements;

namespace WalletApi.Application.Movements;

public sealed record MovementResponse(
    int Id,
    int WalletId,
    int TransferId,
    decimal Amount,
    string Type,
    DateTime CreatedAt)
{
    public static MovementResponse FromDomain(Movement movement) => new(
        movement.Id,
        movement.WalletId,
        movement.TransferId,
        movement.Amount.Amount,
        movement.Type.ToString(),
        movement.CreatedAt);
}
