using FluentAssertions;
using Moq;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Application.UseCases;
using PurchaseTransactions.Domain.Entities;
using PurchaseTransactions.Domain.Services;
using PurchaseTransactions.Domain.ValueObjects;

namespace PurchaseTransactions.Tests.Application.UseCases;

public class GetConvertedTransactionHandlerTests
{
    private readonly Mock<IPurchaseTransactionRepository> _repositoryMock;
    private readonly Mock<IExchangeRateProvider> _exchangeRateProviderMock;
    private readonly CurrencyConversionService _conversionService;
    private readonly GetConvertedTransactionHandler _handler;

    public GetConvertedTransactionHandlerTests()
    {
        _repositoryMock = new Mock<IPurchaseTransactionRepository>();
        _exchangeRateProviderMock = new Mock<IExchangeRateProvider>();
        _conversionService = new CurrencyConversionService();
        _handler = new GetConvertedTransactionHandler(
            _repositoryMock.Object,
            _exchangeRateProviderMock.Object,
            _conversionService);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldReturnConvertedTransaction()
    {
        // Arrange
        var transactionDate = new DateOnly(2024, 6, 15);
        var transaction = PurchaseTransaction.Create("Test purchase", transactionDate, 100.00m);
        var transactionId = transaction.Id.Value;
        var targetCurrency = "EUR";
        var exchangeRate = ExchangeRate.Create(0.85m, "USD", "EUR", new DateOnly(2024, 6, 10));

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TransactionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        _exchangeRateProviderMock
            .Setup(p => p.GetExchangeRateAsync(targetCurrency, transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exchangeRate);

        // Act
        var result = await _handler.HandleAsync(transactionId, targetCurrency, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TransactionId.Should().Be(transactionId);
        result.Description.Should().Be("Test purchase");
        result.OriginalAmount.Should().Be(100.00m);
        result.OriginalCurrency.Should().Be("USD");
        result.TargetCurrency.Should().Be("EUR");
        result.ExchangeRate.Should().Be(0.85m);
        result.ConvertedAmount.Should().Be(85.00m);
    }

    [Fact]
    public async Task HandleAsync_WhenTransactionNotFound_ShouldThrowTransactionNotFoundException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var targetCurrency = "EUR";

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TransactionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseTransaction?)null);

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(transactionId, targetCurrency, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TransactionNotFoundException>()
            .WithMessage($"*{transactionId}*");
    }

    [Fact]
    public async Task HandleAsync_WhenExchangeRateNotFound_ShouldThrowExchangeRateNotFoundException()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Test", new DateOnly(2024, 6, 15), 100.00m);
        var transactionId = transaction.Id.Value;
        var targetCurrency = "XYZ";

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TransactionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        _exchangeRateProviderMock
            .Setup(p => p.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExchangeRate?)null);

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(transactionId, targetCurrency, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ExchangeRateNotFoundException>()
            .WithMessage($"*{targetCurrency}*6 months*");
    }

    [Fact]
    public async Task HandleAsync_WhenExchangeRateTooOld_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transactionDate = new DateOnly(2024, 6, 15);
        var transaction = PurchaseTransaction.Create("Test", transactionDate, 100.00m);
        var transactionId = transaction.Id.Value;
        var targetCurrency = "EUR";
        
        // Exchange rate is more than 6 months old
        var oldExchangeRate = ExchangeRate.Create(0.85m, "USD", "EUR", new DateOnly(2023, 12, 1));

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TransactionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        _exchangeRateProviderMock
            .Setup(p => p.GetExchangeRateAsync(targetCurrency, transactionDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldExchangeRate);

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(transactionId, targetCurrency, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not valid*");
    }
}
