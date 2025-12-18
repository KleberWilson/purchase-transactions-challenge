namespace PurchaseTransactions.Application.DTOs;

/// <summary>
/// Response DTO for a created purchase transaction.
/// </summary>
public sealed class CreatePurchaseTransactionResponse
{
    public Guid TransactionId { get; set; }
}
