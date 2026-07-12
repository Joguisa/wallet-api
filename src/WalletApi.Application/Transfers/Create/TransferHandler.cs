using FluentValidation;
using WalletApi.Application.Abstractions;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Transfers;
using WalletApi.Domain.Wallets;

namespace WalletApi.Application.Transfers.Create;

public sealed class TransferHandler : IRequestHandler<TransferCommand, TransferResponse>
{
    private readonly IValidator<TransferCommand> _validator;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransferRepository _transferRepository;
    private readonly TransferDomainService _transferDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public TransferHandler(
        IValidator<TransferCommand> validator,
        IWalletRepository walletRepository,
        ITransferRepository transferRepository,
        TransferDomainService transferDomainService,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _walletRepository = walletRepository;
        _transferRepository = transferRepository;
        _transferDomainService = transferDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransferResponse> HandleAsync(
        TransferCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var existingTransfer = await _transferRepository
            .GetByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);

        if (existingTransfer is not null)
        {
            return TransferResponse.FromDomain(existingTransfer);
        }

        var (source, destination) = await LoadWalletsAsync(command, cancellationToken);

        var transfer = _transferDomainService.Execute(
            source, destination, Money.From(command.Amount), command.IdempotencyKey);

        _transferRepository.Add(transfer);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DuplicateIdempotencyKeyException)
        {
            var winner = await _transferRepository
                .GetByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);

            if (winner is null)
            {
                throw;
            }

            return TransferResponse.FromDomain(winner);
        }

        return TransferResponse.FromDomain(transfer);
    }

    private async Task<(Wallet Source, Wallet Destination)> LoadWalletsAsync(
        TransferCommand command,
        CancellationToken cancellationToken)
    {
        var lowerId = Math.Min(command.SourceWalletId, command.DestinationWalletId);
        var higherId = Math.Max(command.SourceWalletId, command.DestinationWalletId);

        var lower = await _walletRepository.GetByIdAsync(lowerId, cancellationToken)
            ?? throw new WalletNotFoundException(lowerId);
        var higher = await _walletRepository.GetByIdAsync(higherId, cancellationToken)
            ?? throw new WalletNotFoundException(higherId);

        return command.SourceWalletId == lowerId ? (lower, higher) : (higher, lower);
    }
}
