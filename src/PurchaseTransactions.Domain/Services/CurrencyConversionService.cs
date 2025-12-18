using PurchaseTransactions.Domain.Entities;
using PurchaseTransactions.Domain.ValueObjects;

namespace PurchaseTransactions.Domain.Services;

/// <summary>
/// Domain Service responsible for currency conversion business logic.
/// Encapsulates complex business rules that don't belong to a single entity.
/// </summary>
public class CurrencyConversionService
{
    /// <summary>
    /// Converts a transaction amount to target currency using the provided exchange rate.
    /// Validates business rules before performing conversion.
    /// </summary>
    public ConvertedTransaction ConvertTransaction(
        PurchaseTransaction transaction, 
        ExchangeRate exchangeRate)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (exchangeRate == null)
        {
            throw new ArgumentNullException(nameof(exchangeRate));
        }

        // Business rule validation: exchange rate must be valid for this transaction
        if (!transaction.IsExchangeRateValid(exchangeRate))
        {
            throw new InvalidOperationException(
                $"Exchange rate dated {exchangeRate.EffectiveDate} is not valid for transaction dated {transaction.TransactionDate}. " +
                $"Rate must be on or before transaction date and within 6 months.");
        }

        // Perform the conversion
        var convertedAmount = exchangeRate.Convert(transaction.Amount);

        return new ConvertedTransaction(
            transaction.Id,
            transaction.Description,
            transaction.TransactionDate,
            transaction.Amount,
            convertedAmount,
            exchangeRate.Rate);
    }
}

/// <summary>
/// Value Object representing the result of a currency conversion.
/// Contains all data needed for the response.
/// </summary>
public sealed class ConvertedTransaction
{
    public TransactionId TransactionId { get; }
    public Description Description { get; }
    public DateOnly TransactionDate { get; }
    public Money OriginalAmount { get; }
    public Money ConvertedAmount { get; }
    public decimal ExchangeRate { get; }

    public ConvertedTransaction(
        TransactionId transactionId,
        Description description,
        DateOnly transactionDate,
        Money originalAmount,
        Money convertedAmount,
        decimal exchangeRate)
    {
        TransactionId = transactionId ?? throw new ArgumentNullException(nameof(transactionId));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        TransactionDate = transactionDate;
        OriginalAmount = originalAmount ?? throw new ArgumentNullException(nameof(originalAmount));
        ConvertedAmount = convertedAmount ?? throw new ArgumentNullException(nameof(convertedAmount));
        ExchangeRate = exchangeRate;
    }
}
