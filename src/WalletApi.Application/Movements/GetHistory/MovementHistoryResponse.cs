namespace WalletApi.Application.Movements.GetHistory;

public sealed record MovementHistoryResponse(
    IReadOnlyList<MovementResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
