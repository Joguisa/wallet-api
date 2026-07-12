using FluentValidation;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Wallets;

namespace WalletApi.Application.Wallets.Create;

public sealed class CreateWalletValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletValidator()
    {
        RuleFor(c => c.DocumentId)
            .NotEmpty()
            .Matches($"^[0-9]{{{DocumentId.Length}}}$")
            .WithMessage($"Document id must be exactly {DocumentId.Length} numeric digits.");

        RuleFor(c => c.OwnerName)
            .NotEmpty()
            .MaximumLength(Wallet.MaxOwnerNameLength);

        RuleFor(c => c.InitialBalance)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Initial balance cannot have more than two decimal places.");
    }
}
