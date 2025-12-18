using FluentAssertions;
using PurchaseTransactions.Domain.Entities;
using PurchaseTransactions.Domain.ValueObjects;

namespace PurchaseTransactions.Tests.Domain.Entities;

public class PurchaseTransactionTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var description = "Coffee at Starbucks";
        var date = new DateOnly(2024, 6, 1);
        var amount = 15.50m;

        // Act
        var transaction = PurchaseTransaction.Create(description, date, amount);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Id.Should().NotBeNull();
        transaction.Description.Value.Should().Be(description);
        transaction.TransactionDate.Should().Be(date);
        transaction.Amount.Amount.Should().Be(15.50m);
        transaction.Amount.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var description = new string('a', 51); // 51 characters
        var date = new DateOnly(2024, 6, 1);
        var amount = 15.50m;

        // Act
        Action act = () => PurchaseTransaction.Create(description, date, amount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot exceed 50 characters*");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var description = "Invalid transaction";
        var date = new DateOnly(2024, 6, 1);
        var amount = -10.00m;

        // Act
        Action act = () => PurchaseTransaction.Create(description, date, amount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Create_WithFutureDate_ShouldThrowArgumentException()
    {
        // Arrange
        var description = "Future transaction";
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var amount = 100.00m;

        // Act
        Action act = () => PurchaseTransaction.Create(description, futureDate, amount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be in the future*");
    }

    [Fact]
    public void IsExchangeRateValid_WithRateBeforeTransaction_ShouldReturnTrue()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Test", new DateOnly(2024, 6, 15), 100m);
        var exchangeRate = ExchangeRate.Create(0.85m, "USD", "EUR", new DateOnly(2024, 6, 10));

        // Act
        var isValid = transaction.IsExchangeRateValid(exchangeRate);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsExchangeRateValid_WithRateAfterTransaction_ShouldReturnFalse()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Test", new DateOnly(2024, 6, 15), 100m);
        var exchangeRate = ExchangeRate.Create(0.85m, "USD", "EUR", new DateOnly(2024, 6, 20));

        // Act
        var isValid = transaction.IsExchangeRateValid(exchangeRate);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsExchangeRateValid_WithRateOlderThanSixMonths_ShouldReturnFalse()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Test", new DateOnly(2024, 6, 15), 100m);
        var exchangeRate = ExchangeRate.Create(0.85m, "USD", "EUR", new DateOnly(2023, 12, 1)); // 6+ months before

        // Act
        var isValid = transaction.IsExchangeRateValid(exchangeRate);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsExchangeRateValid_WithRateExactlySixMonthsOld_ShouldReturnTrue()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Test", new DateOnly(2024, 6, 15), 100m);
        var exchangeRate = ExchangeRate.Create(0.85m, "USD", "EUR", new DateOnly(2023, 12, 15)); // Exactly 6 months

        // Act
        var isValid = transaction.IsExchangeRateValid(exchangeRate);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsExchangeRateValid_WithRateOnSameDate_ShouldReturnTrue()
    {
        // Arrange
        var date = new DateOnly(2024, 6, 15);
        var transaction = PurchaseTransaction.Create("Test", date, 100m);
        var exchangeRate = ExchangeRate.Create(0.85m, "USD", "EUR", date);

        // Act
        var isValid = transaction.IsExchangeRateValid(exchangeRate);

        // Assert
        isValid.Should().BeTrue();
    }
}
