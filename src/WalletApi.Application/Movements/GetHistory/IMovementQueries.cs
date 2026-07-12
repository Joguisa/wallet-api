namespace WalletApi.Application.Movements.GetHistory;

public interface IMovementQueries
{
    Task<(IReadOnlyList<MovementResponse> Items, int TotalCount)> GetHistoryAsync(
        int walletId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
