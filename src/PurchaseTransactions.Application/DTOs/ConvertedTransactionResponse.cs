namespace PurchaseTransactions.Application.DTOs;

/// <summary>
/// Response DTO for a converted transaction.
/// Contains all conversion details as per requirements.
/// </summary>
public sealed class ConvertedTransactionResponse
{
    public Guid TransactionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public decimal OriginalAmount { get; set; }
    public string OriginalCurrency { get; set; } = string.Empty;
    public string TargetCurrency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal ConvertedAmount { get; set; }
}
