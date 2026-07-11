using FluentAssertions;
using WalletApi.Domain.Shared;

namespace WalletApi.UnitTests.Domain;

public class MoneyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(10.5)]
    [InlineData(10.55)]
    public void From_accepts_valid_amounts(double amount)
    {
        var money = Money.From((decimal)amount);

        money.Amount.Should().Be((decimal)amount);
    }

    [Fact]
    public void From_rejects_negative_amounts()
    {
        var act = () => Money.From(-0.01m);

        act.Should().Throw<InvalidAmountException>()
            .Which.ErrorCode.Should().Be("INVALID_AMOUNT");
    }

    [Fact]
    public void From_rejects_more_than_two_decimal_places()
    {
        var act = () => Money.From(10.555m);

        act.Should().Throw<InvalidAmountException>();
    }

    [Fact]
    public void Add_returns_the_sum()
    {
        var result = Money.From(10.25m).Add(Money.From(5.75m));

        result.Should().Be(Money.From(16m));
    }

    [Fact]
    public void Subtract_returns_the_difference()
    {
        var result = Money.From(10m).Subtract(Money.From(2.5m));

        result.Should().Be(Money.From(7.5m));
    }

    [Fact]
    public void Subtract_rejects_a_larger_amount()
    {
        var act = () => Money.From(10m).Subtract(Money.From(10.01m));

        act.Should().Throw<InvalidAmountException>();
    }

    [Fact]
    public void Money_with_the_same_amount_are_equal()
    {
        Money.From(10.50m).Should().Be(Money.From(10.50m));
        Money.From(10.50m).Should().NotBe(Money.From(10.51m));
    }

    [Fact]
    public void Comparison_operators_compare_amounts()
    {
        (Money.From(5m) < Money.From(10m)).Should().BeTrue();
        (Money.From(10m) > Money.From(5m)).Should().BeTrue();
        (Money.From(10m) <= Money.From(10m)).Should().BeTrue();
        (Money.From(10m) >= Money.From(10m)).Should().BeTrue();
    }

    [Fact]
    public void Zero_is_not_positive()
    {
        Money.Zero.IsPositive.Should().BeFalse();
        Money.From(0.01m).IsPositive.Should().BeTrue();
    }
}
