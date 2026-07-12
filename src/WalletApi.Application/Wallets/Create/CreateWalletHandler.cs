using FluentValidation;
using WalletApi.Application.Abstractions;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Wallets;

namespace WalletApi.Application.Wallets.Create;

public sealed class CreateWalletHandler : IRequestHandler<CreateWalletCommand, WalletResponse>
{
    private readonly IValidator<CreateWalletCommand> _validator;
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWalletHandler(
        IValidator<CreateWalletCommand> validator,
        IWalletRepository walletRepository,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WalletResponse> HandleAsync(
        CreateWalletCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var documentId = DocumentId.From(command.DocumentId);

        if (await _walletRepository.ExistsByDocumentIdAsync(documentId, cancellationToken))
        {
            throw new DuplicateDocumentIdException(documentId.Value);
        }

        var wallet = Wallet.Create(documentId, command.OwnerName, Money.From(command.InitialBalance));

        _walletRepository.Add(wallet);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WalletResponse.FromDomain(wallet);
    }
}
