using FluentValidation;

namespace WalletApi.Application.Transfers.Create;

public sealed class TransferValidator : AbstractValidator<TransferCommand>
{
    public TransferValidator()
    {
        RuleFor(c => c.SourceWalletId).GreaterThan(0);

        RuleFor(c => c.DestinationWalletId)
            .GreaterThan(0)
            .NotEqual(c => c.SourceWalletId)
            .WithMessage("Source and destination wallets must be different.");

        RuleFor(c => c.Amount)
            .GreaterThan(0)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Amount cannot have more than two decimal places.");

        RuleFor(c => c.IdempotencyKey)
            .NotEmpty()
            .WithMessage("The Idempotency-Key header is required and must be a valid GUID.");
    }
}
