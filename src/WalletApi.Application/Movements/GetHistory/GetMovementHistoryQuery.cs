namespace WalletApi.Application.Movements.GetHistory;

public sealed record GetMovementHistoryQuery(
    int WalletId,
    int Page = 1,
    int PageSize = GetMovementHistoryQuery.DefaultPageSize)
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}
