using FluentAssertions;
using WalletApi.Domain.Shared;
using WalletApi.Domain.Wallets;

namespace WalletApi.UnitTests.Domain;

public class WalletTests
{
    private static Wallet CreateWallet(decimal balance = 100m) =>
        Wallet.Create(DocumentId.From("0941262000"), "Jonatan Guillen", Money.From(balance));

    [Fact]
    public void Create_sets_all_properties()
    {
        var wallet = Wallet.Create(DocumentId.From("0941262000"), "Jonatan Guillen", Money.From(50m));

        wallet.DocumentId.Value.Should().Be("0941262000");
        wallet.OwnerName.Should().Be("Jonatan Guillen");
        wallet.Balance.Should().Be(Money.From(50m));
        wallet.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        wallet.UpdatedAt.Should().Be(wallet.CreatedAt);
    }

    [Fact]
    public void Create_trims_the_owner_name()
    {
        var wallet = Wallet.Create(DocumentId.From("0941262000"), "  Jonatan Guillen  ", Money.Zero);

        wallet.OwnerName.Should().Be("Jonatan Guillen");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_an_empty_owner_name(string? ownerName)
    {
        var act = () => Wallet.Create(DocumentId.From("0941262000"), ownerName!, Money.Zero);

        act.Should().Throw<InvalidOwnerNameException>()
            .Which.ErrorCode.Should().Be("INVALID_OWNER_NAME");
    }

    [Fact]
    public void Create_rejects_an_owner_name_longer_than_the_maximum()
    {
        var tooLong = new string('a', Wallet.MaxOwnerNameLength + 1);

        var act = () => Wallet.Create(DocumentId.From("0941262000"), tooLong, Money.Zero);

        act.Should().Throw<InvalidOwnerNameException>();
    }

    [Fact]
    public void Debit_reduces_the_balance()
    {
        var wallet = CreateWallet(100m);

        wallet.Debit(Money.From(40.50m));

        wallet.Balance.Should().Be(Money.From(59.50m));
    }

    [Fact]
    public void Debit_of_the_entire_balance_leaves_zero()
    {
        var wallet = CreateWallet(100m);

        wallet.Debit(Money.From(100m));

        wallet.Balance.Should().Be(Money.Zero);
    }

    [Fact]
    public void Debit_beyond_the_balance_is_rejected_and_leaves_the_balance_unchanged()
    {
        var wallet = CreateWallet(50m);

        var act = () => wallet.Debit(Money.From(50.01m));

        act.Should().Throw<InsufficientFundsException>()
            .Which.ErrorCode.Should().Be("INSUFFICIENT_FUNDS");
        wallet.Balance.Should().Be(Money.From(50m));
    }

    [Fact]
    public void Debit_of_zero_is_rejected()
    {
        var wallet = CreateWallet();

        var act = () => wallet.Debit(Money.Zero);

        act.Should().Throw<InvalidAmountException>();
    }

    [Fact]
    public void Credit_increases_the_balance()
    {
        var wallet = CreateWallet(10m);

        wallet.Credit(Money.From(2.25m));

        wallet.Balance.Should().Be(Money.From(12.25m));
    }

    [Fact]
    public void Credit_of_zero_is_rejected()
    {
        var wallet = CreateWallet();

        var act = () => wallet.Credit(Money.Zero);

        act.Should().Throw<InvalidAmountException>();
    }

    [Fact]
    public void Successful_operations_refresh_UpdatedAt()
    {
        var wallet = CreateWallet(100m);
        var createdAt = wallet.CreatedAt;

        wallet.Debit(Money.From(1m));

        wallet.UpdatedAt.Should().BeOnOrAfter(createdAt);
        wallet.CreatedAt.Should().Be(createdAt);
    }
}
