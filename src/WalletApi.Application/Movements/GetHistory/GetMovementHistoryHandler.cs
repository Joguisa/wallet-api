using FluentValidation;
using WalletApi.Application.Abstractions;
using WalletApi.Domain.Wallets;

namespace WalletApi.Application.Movements.GetHistory;

public sealed class GetMovementHistoryHandler
    : IRequestHandler<GetMovementHistoryQuery, MovementHistoryResponse>
{
    private readonly IValidator<GetMovementHistoryQuery> _validator;
    private readonly IWalletRepository _walletRepository;
    private readonly IMovementQueries _movementQueries;

    public GetMovementHistoryHandler(
        IValidator<GetMovementHistoryQuery> validator,
        IWalletRepository walletRepository,
        IMovementQueries movementQueries)
    {
        _validator = validator;
        _walletRepository = walletRepository;
        _movementQueries = movementQueries;
    }

    public async Task<MovementHistoryResponse> HandleAsync(
        GetMovementHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        _ = await _walletRepository.GetByIdAsync(query.WalletId, cancellationToken)
            ?? throw new WalletNotFoundException(query.WalletId);

        var (items, totalCount) = await _movementQueries.GetHistoryAsync(
            query.WalletId, query.Page, query.PageSize, cancellationToken);

        return new MovementHistoryResponse(items, query.Page, query.PageSize, totalCount);
    }
}
