using FluentAssertions;
using WalletApi.Domain.Movements;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Transfers;
using WalletApi.Domain.Wallets;

namespace WalletApi.UnitTests.Domain;

public class TransferDomainServiceTests
{
    private readonly TransferDomainService _service = new();

    private static Wallet CreateWallet(string documentId, decimal balance) =>
        Wallet.Create(DocumentId.From(documentId), "Owner " + documentId, Money.From(balance));

    [Fact]
    public void Execute_debits_the_source_and_credits_the_destination()
    {
        var source = CreateWallet("0941262000", 100m);
        var destination = CreateWallet("0912345678", 20m);

        _service.Execute(source, destination, Money.From(30m), Guid.NewGuid());

        source.Balance.Should().Be(Money.From(70m));
        destination.Balance.Should().Be(Money.From(50m));
    }

    [Fact]
    public void Execute_returns_a_transfer_with_one_debit_and_one_credit_movement()
    {
        var source = CreateWallet("0941262000", 100m);
        var destination = CreateWallet("0912345678", 0m);
        var amount = Money.From(25.50m);

        var transfer = _service.Execute(source, destination, amount, Guid.NewGuid());

        transfer.Amount.Should().Be(amount);
        transfer.Movements.Should().HaveCount(2);
        transfer.Movements.Should().ContainSingle(m => m.Type == MovementType.Debit);
        transfer.Movements.Should().ContainSingle(m => m.Type == MovementType.Credit);
        transfer.Movements.Should().OnlyContain(m => m.Amount == amount);
        transfer.Movements.Should().OnlyContain(m => m.CreatedAt == transfer.CreatedAt);
    }

    [Fact]
    public void Execute_stores_the_idempotency_key()
    {
        var key = Guid.NewGuid();

        var transfer = _service.Execute(
            CreateWallet("0941262000", 100m), CreateWallet("0912345678", 0m), Money.From(1m), key);

        transfer.IdempotencyKey.Should().Be(key);
    }

    [Fact]
    public void Execute_rejects_a_transfer_to_the_same_wallet()
    {
        var wallet = CreateWallet("0941262000", 100m);

        var act = () => _service.Execute(wallet, wallet, Money.From(10m), Guid.NewGuid());

        act.Should().Throw<SelfTransferNotAllowedException>()
            .Which.ErrorCode.Should().Be("SELF_TRANSFER_NOT_ALLOWED");
        wallet.Balance.Should().Be(Money.From(100m));
    }

    [Fact]
    public void Execute_rejects_a_zero_amount_without_touching_balances()
    {
        var source = CreateWallet("0941262000", 100m);
        var destination = CreateWallet("0912345678", 20m);

        var act = () => _service.Execute(source, destination, Money.Zero, Guid.NewGuid());

        act.Should().Throw<InvalidAmountException>();
        source.Balance.Should().Be(Money.From(100m));
        destination.Balance.Should().Be(Money.From(20m));
    }

    [Fact]
    public void Execute_with_insufficient_funds_throws_and_leaves_the_destination_unchanged()
    {
        var source = CreateWallet("0941262000", 10m);
        var destination = CreateWallet("0912345678", 20m);

        var act = () => _service.Execute(source, destination, Money.From(10.01m), Guid.NewGuid());

        act.Should().Throw<InsufficientFundsException>();
        source.Balance.Should().Be(Money.From(10m));
        destination.Balance.Should().Be(Money.From(20m));
    }

    [Fact]
    public void Execute_can_transfer_the_exact_balance()
    {
        var source = CreateWallet("0941262000", 42.42m);
        var destination = CreateWallet("0912345678", 0m);

        _service.Execute(source, destination, Money.From(42.42m), Guid.NewGuid());

        source.Balance.Should().Be(Money.Zero);
        destination.Balance.Should().Be(Money.From(42.42m));
    }
}
