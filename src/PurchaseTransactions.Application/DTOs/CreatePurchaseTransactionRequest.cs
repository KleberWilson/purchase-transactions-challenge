namespace PurchaseTransactions.Application.DTOs;

/// <summary>
/// Request DTO for creating a new purchase transaction.
/// </summary>
public sealed class CreatePurchaseTransactionRequest
{
    public string Description { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public decimal PurchaseAmount { get; set; }
}
