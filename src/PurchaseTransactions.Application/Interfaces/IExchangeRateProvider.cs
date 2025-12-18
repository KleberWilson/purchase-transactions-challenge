using PurchaseTransactions.Domain.ValueObjects;

namespace PurchaseTransactions.Application.Interfaces;

/// <summary>
/// Interface for retrieving exchange rates from external sources.
/// Abstracts the Treasury API or any other exchange rate provider.
/// </summary>
public interface IExchangeRateProvider
{
    /// <summary>
    /// Retrieves the most recent exchange rate for the specified currency,
    /// valid for the given transaction date and within the last 6 months.
    /// </summary>
    /// <param name="targetCurrency">The target currency to convert to (e.g., "EUR", "GBP").</param>
    /// <param name="transactionDate">The transaction date for which the rate should be valid.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The exchange rate if found, null if no valid rate is available.</returns>
    Task<ExchangeRate?> GetExchangeRateAsync(
        string targetCurrency, 
        DateOnly transactionDate, 
        CancellationToken cancellationToken = default);
}
