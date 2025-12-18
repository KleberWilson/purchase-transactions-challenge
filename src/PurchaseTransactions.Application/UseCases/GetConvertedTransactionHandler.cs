using PurchaseTransactions.Application.DTOs;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Domain.Services;
using PurchaseTransactions.Domain.ValueObjects;

namespace PurchaseTransactions.Application.UseCases;

/// <summary>
/// Handler for retrieving a purchase transaction converted to a target currency.
/// Orchestrates retrieval, exchange rate lookup, and conversion.
/// </summary>
public class GetConvertedTransactionHandler
{
    private readonly IPurchaseTransactionRepository _repository;
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly CurrencyConversionService _conversionService;

    public GetConvertedTransactionHandler(
        IPurchaseTransactionRepository repository,
        IExchangeRateProvider exchangeRateProvider,
        CurrencyConversionService conversionService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _exchangeRateProvider = exchangeRateProvider ?? throw new ArgumentNullException(nameof(exchangeRateProvider));
        _conversionService = conversionService ?? throw new ArgumentNullException(nameof(conversionService));
    }

    /// <summary>
    /// Handles retrieving and converting a transaction.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="targetCurrency">The target currency (e.g., "EUR", "GBP").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The converted transaction details.</returns>
    /// <exception cref="TransactionNotFoundException">When transaction is not found.</exception>
    /// <exception cref="ExchangeRateNotFoundException">When no valid exchange rate is available.</exception>
    public async Task<ConvertedTransactionResponse> HandleAsync(
        Guid transactionId,
        string targetCurrency,
        CancellationToken cancellationToken = default)
    {
        // Retrieve the transaction
        var transactionIdVO = TransactionId.From(transactionId);
        var transaction = await _repository.GetByIdAsync(transactionIdVO, cancellationToken);

        if (transaction == null)
        {
            throw new TransactionNotFoundException($"Transaction with ID {transactionId} not found.");
        }

        // Get exchange rate from provider
        var exchangeRate = await _exchangeRateProvider.GetExchangeRateAsync(
            targetCurrency,
            transaction.TransactionDate,
            cancellationToken);

        if (exchangeRate == null)
        {
            throw new ExchangeRateNotFoundException(
                $"No exchange rate available for {targetCurrency} within 6 months of transaction date {transaction.TransactionDate}.");
        }

        // Perform conversion using domain service
        var convertedTransaction = _conversionService.ConvertTransaction(transaction, exchangeRate);

        // Map to response DTO
        return new ConvertedTransactionResponse
        {
            TransactionId = convertedTransaction.TransactionId.Value,
            Description = convertedTransaction.Description.Value,
            TransactionDate = convertedTransaction.TransactionDate,
            OriginalAmount = convertedTransaction.OriginalAmount.Amount,
            OriginalCurrency = convertedTransaction.OriginalAmount.Currency,
            TargetCurrency = convertedTransaction.ConvertedAmount.Currency,
            ExchangeRate = convertedTransaction.ExchangeRate,
            ConvertedAmount = convertedTransaction.ConvertedAmount.Amount
        };
    }
}

/// <summary>
/// Exception thrown when a transaction is not found.
/// </summary>
public class TransactionNotFoundException : Exception
{
    public TransactionNotFoundException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when no valid exchange rate is available.
/// </summary>
public class ExchangeRateNotFoundException : Exception
{
    public ExchangeRateNotFoundException(string message) : base(message) { }
}
