using PurchaseTransactions.Domain.ValueObjects;

namespace PurchaseTransactions.Domain.Entities;

/// <summary>
/// Aggregate Root representing a Purchase Transaction.
/// Contains all business rules and invariants for transaction creation and management.
/// </summary>
public sealed class PurchaseTransaction
{
    public TransactionId Id { get; }
    public Description Description { get; }
    public DateOnly TransactionDate { get; }
    public Money Amount { get; }

    private PurchaseTransaction(TransactionId id, Description description, DateOnly transactionDate, Money amount)
    {
        Id = id;
        Description = description;
        TransactionDate = transactionDate;
        Amount = amount;
    }

    /// <summary>
    /// Creates a new PurchaseTransaction with validation.
    /// </summary>
    public static PurchaseTransaction Create(string description, DateOnly transactionDate, decimal purchaseAmountInUsd)
    {
        // Validate and create value objects (they handle their own validation)
        var descriptionVO = Description.Create(description);
        var amountVO = Money.Usd(purchaseAmountInUsd);

        // Business rule: Transaction date cannot be in the future
        // Note: Using DateOnly comparison for date-only validation
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (transactionDate > today)
        {
            throw new ArgumentException("Transaction date cannot be in the future.", nameof(transactionDate));
        }

        // Generate unique identifier
        var id = TransactionId.Create();

        return new PurchaseTransaction(id, descriptionVO, transactionDate, amountVO);
    }

    /// <summary>
    /// Reconstitutes a PurchaseTransaction from stored data (for repository).
    /// </summary>
    public static PurchaseTransaction Reconstitute(Guid id, string description, DateOnly transactionDate, decimal amount)
    {
        var transactionId = TransactionId.From(id);
        var descriptionVO = Description.Create(description);
        var amountVO = Money.Usd(amount);

        return new PurchaseTransaction(transactionId, descriptionVO, transactionDate, amountVO);
    }

    /// <summary>
    /// Validates if an exchange rate can be used for this transaction.
    /// Business rules: rate date must be <= transaction date and within 6 months.
    /// </summary>
    public bool IsExchangeRateValid(ExchangeRate exchangeRate)
    {
        // Rule 1: Exchange rate date must be on or before transaction date
        if (exchangeRate.EffectiveDate > TransactionDate)
        {
            return false;
        }

        // Rule 2: Exchange rate must be within 6 months of transaction date
        var sixMonthsBeforeTransaction = TransactionDate.AddMonths(-6);
        if (exchangeRate.EffectiveDate < sixMonthsBeforeTransaction)
        {
            return false;
        }

        return true;
    }

    public override string ToString() => 
        $"Transaction {Id}: {Description} on {TransactionDate} for {Amount}";
}
