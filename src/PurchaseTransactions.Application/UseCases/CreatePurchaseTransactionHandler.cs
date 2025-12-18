using PurchaseTransactions.Application.DTOs;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Domain.Entities;

namespace PurchaseTransactions.Application.UseCases;

/// <summary>
/// Handler for creating a new purchase transaction.
/// Orchestrates domain logic and persistence.
/// </summary>
public class CreatePurchaseTransactionHandler
{
    private readonly IPurchaseTransactionRepository _repository;

    public CreatePurchaseTransactionHandler(IPurchaseTransactionRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Handles the creation of a new purchase transaction.
    /// </summary>
    public async Task<CreatePurchaseTransactionResponse> HandleAsync(
        CreatePurchaseTransactionRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Domain layer validates and creates the transaction
        // All business rules are enforced here
        var transaction = PurchaseTransaction.Create(
            request.Description,
            request.TransactionDate,
            request.PurchaseAmount);

        // Persist the transaction
        await _repository.SaveAsync(transaction, cancellationToken);

        // Return the response
        return new CreatePurchaseTransactionResponse
        {
            TransactionId = transaction.Id.Value
        };
    }
}
