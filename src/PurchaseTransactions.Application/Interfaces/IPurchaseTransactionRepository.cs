using PurchaseTransactions.Domain.Entities;
using PurchaseTransactions.Domain.ValueObjects;

namespace PurchaseTransactions.Application.Interfaces;

/// <summary>
/// Repository interface for PurchaseTransaction aggregate.
/// Defines persistence operations abstracted from implementation details.
/// </summary>
public interface IPurchaseTransactionRepository
{
    /// <summary>
    /// Saves a new purchase transaction.
    /// </summary>
    Task<PurchaseTransaction> SaveAsync(PurchaseTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a purchase transaction by its unique identifier.
    /// </summary>
    /// <returns>The transaction if found, null otherwise.</returns>
    Task<PurchaseTransaction?> GetByIdAsync(TransactionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a transaction exists with the given identifier.
    /// </summary>
    Task<bool> ExistsAsync(TransactionId id, CancellationToken cancellationToken = default);
}
