using FluentAssertions;
using PurchaseTransactions.Domain.ValueObjects;

namespace PurchaseTransactions.Tests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldSucceed()
    {
        // Arrange
        var amount = 100.50m;
        var currency = "USD";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var amount = -10.00m;
        var currency = "USD";

        // Act
        Action act = () => Money.Create(amount, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Create_WithNullCurrency_ShouldThrowArgumentException()
    {
        // Arrange
        var amount = 100.00m;
        string currency = null!;

        // Act
        Action act = () => Money.Create(amount, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void Create_WithInvalidCurrencyLength_ShouldThrowArgumentException()
    {
        // Arrange
        var amount = 100.00m;
        var currency = "US"; // Only 2 characters

        // Act
        Action act = () => Money.Create(amount, currency);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be 3 characters*");
    }

    [Fact]
    public void Create_ShouldRoundToTwoDecimalPlaces()
    {
        // Arrange
        var amount = 100.555m; // More than 2 decimals

        // Act
        var money = Money.Create(amount, "USD");

        // Assert
        money.Amount.Should().Be(100.56m);
    }

    [Fact]
    public void Create_ShouldUpperCaseCurrency()
    {
        // Arrange
        var amount = 100.00m;
        var currency = "usd";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Usd_ShouldCreateMoneyWithUSDCurrency()
    {
        // Arrange
        var amount = 50.00m;

        // Act
        var money = Money.Usd(amount);

        // Assert
        money.Amount.Should().Be(50.00m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.Create(100.00m, "USD");
        var money2 = Money.Create(100.00m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentAmounts_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.Create(100.00m, "USD");
        var money2 = Money.Create(200.00m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.Create(1234.56m, "EUR");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().MatchRegex(@"1234[.,]56 EUR"); // Accept both . and , as decimal separator
    }
}
