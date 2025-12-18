using System.Collections.Concurrent;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Domain.Entities;
using PurchaseTransactions.Domain.ValueObjects;

namespace PurchaseTransactions.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of the purchase transaction repository.
/// Thread-safe using ConcurrentDictionary for production readiness.
/// </summary>
public class InMemoryPurchaseTransactionRepository : IPurchaseTransactionRepository
{
    private readonly ConcurrentDictionary<Guid, PurchaseTransaction> _transactions = new();

    public Task<PurchaseTransaction> SaveAsync(PurchaseTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        _transactions[transaction.Id.Value] = transaction;
        return Task.FromResult(transaction);
    }

    public Task<PurchaseTransaction?> GetByIdAsync(TransactionId id, CancellationToken cancellationToken = default)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        _transactions.TryGetValue(id.Value, out var transaction);
        return Task.FromResult(transaction);
    }

    public Task<bool> ExistsAsync(TransactionId id, CancellationToken cancellationToken = default)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return Task.FromResult(_transactions.ContainsKey(id.Value));
    }
}
