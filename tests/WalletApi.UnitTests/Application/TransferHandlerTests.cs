using FluentAssertions;
using FluentValidation;
using NSubstitute;
using WalletApi.Application.Abstractions;
using WalletApi.Application.Transfers.Create;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Transfers;
using WalletApi.Domain.Wallets;

namespace WalletApi.UnitTests.Application;

public class TransferHandlerTests
{
    private readonly IWalletRepository _walletRepository = Substitute.For<IWalletRepository>();
    private readonly ITransferRepository _transferRepository = Substitute.For<ITransferRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TransferHandler _handler;

    public TransferHandlerTests()
    {
        _handler = new TransferHandler(
            new TransferValidator(),
            _walletRepository,
            _transferRepository,
            new TransferDomainService(),
            _unitOfWork);
    }

    private static TransferCommand ValidCommand(decimal amount = 30m) =>
        new(SourceWalletId: 1, DestinationWalletId: 2, amount, Guid.NewGuid());

    private static Wallet CreateWallet(string documentId, decimal balance) =>
        Wallet.Create(DocumentId.From(documentId), "Owner " + documentId, Money.From(balance));

    [Fact]
    public async Task Handle_executes_the_transfer_and_saves_atomically()
    {
        var source = CreateWallet("0941262000", 100m);
        var destination = CreateWallet("0912345678", 20m);
        _walletRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(source);
        _walletRepository.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(destination);

        var response = await _handler.HandleAsync(ValidCommand(30m));

        source.Balance.Should().Be(Money.From(70m));
        destination.Balance.Should().Be(Money.From(50m));
        response.Amount.Should().Be(30m);
        _transferRepository.Received(1).Add(Arg.Is<Transfer>(t => t.Movements.Count == 2));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_returns_the_original_transfer_when_the_key_was_already_used()
    {
        var command = ValidCommand();
        var original = Transfer.Create(
            CreateWallet("0941262000", 100m), CreateWallet("0912345678", 0m),
            Money.From(30m), command.IdempotencyKey);
        _transferRepository.GetByIdempotencyKeyAsync(command.IdempotencyKey, Arg.Any<CancellationToken>())
            .Returns(original);

        var response = await _handler.HandleAsync(command);

        response.Amount.Should().Be(30m);
        response.CreatedAt.Should().Be(original.CreatedAt);
        await _walletRepository.DidNotReceiveWithAnyArgs().GetByIdAsync(default!, default);
        _transferRepository.DidNotReceiveWithAnyArgs().Add(default!);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_throws_when_the_source_wallet_does_not_exist()
    {
        _walletRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Wallet?)null);
        _walletRepository.GetByIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(CreateWallet("0912345678", 20m));

        var act = () => _handler.HandleAsync(ValidCommand());

        (await act.Should().ThrowAsync<WalletNotFoundException>())
            .Which.Message.Should().Contain("Wallet 1");
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_throws_when_the_destination_wallet_does_not_exist()
    {
        _walletRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(CreateWallet("0941262000", 100m));
        _walletRepository.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns((Wallet?)null);

        var act = () => _handler.HandleAsync(ValidCommand());

        (await act.Should().ThrowAsync<WalletNotFoundException>())
            .Which.Message.Should().Contain("Wallet 2");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Handle_rejects_non_positive_amounts_before_touching_repositories(decimal amount)
    {
        var act = () => _handler.HandleAsync(ValidCommand(amount));

        await act.Should().ThrowAsync<ValidationException>();
        await _walletRepository.DidNotReceiveWithAnyArgs().GetByIdAsync(default!, default);
    }

    [Fact]
    public async Task Handle_rejects_a_missing_idempotency_key()
    {
        var command = new TransferCommand(1, 2, 30m, Guid.Empty);

        var act = () => _handler.HandleAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_returns_the_winning_transfer_after_losing_a_key_race()
    {
        var command = ValidCommand(30m);
        var source = CreateWallet("0941262000", 100m);
        var destination = CreateWallet("0912345678", 20m);
        var winner = Transfer.Create(
            CreateWallet("0999999999", 100m), CreateWallet("0888888888", 0m),
            Money.From(30m), command.IdempotencyKey);

        _walletRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(source);
        _walletRepository.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(destination);
        
        _transferRepository.GetByIdempotencyKeyAsync(command.IdempotencyKey, Arg.Any<CancellationToken>())
            .Returns((Transfer?)null, winner);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<Task<int>>(_ => throw new DuplicateIdempotencyKeyException(command.IdempotencyKey));

        var response = await _handler.HandleAsync(command);

        response.CreatedAt.Should().Be(winner.CreatedAt);
        response.Amount.Should().Be(30m);
    }
}
