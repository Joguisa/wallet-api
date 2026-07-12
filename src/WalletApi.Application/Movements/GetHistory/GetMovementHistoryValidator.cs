using FluentValidation;

namespace WalletApi.Application.Movements.GetHistory;

public sealed class GetMovementHistoryValidator : AbstractValidator<GetMovementHistoryQuery>
{
    public GetMovementHistoryValidator()
    {
        RuleFor(q => q.WalletId).GreaterThan(0);

        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, GetMovementHistoryQuery.MaxPageSize);
    }
}
