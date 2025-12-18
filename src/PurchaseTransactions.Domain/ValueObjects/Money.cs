namespace PurchaseTransactions.Domain.ValueObjects;

/// <summary>
/// Value Object representing a monetary amount with currency.
/// Immutable and self-validating.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a new Money instance with validation.
    /// </summary>
    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency cannot be null or empty.", nameof(currency));
        }

        if (currency.Length != 3)
        {
            throw new ArgumentException("Currency code must be 3 characters (ISO 4217).", nameof(currency));
        }

        // Round to 2 decimal places as per requirements
        var roundedAmount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

        return new Money(roundedAmount, currency.ToUpperInvariant());
    }

    /// <summary>
    /// Creates a Money instance specifically for USD.
    /// </summary>
    public static Money Usd(decimal amount) => Create(amount, "USD");

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => obj is Money other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => $"{Amount:F2} {Currency}";

    public static bool operator ==(Money? left, Money? right) => Equals(left, right);
    public static bool operator !=(Money? left, Money? right) => !Equals(left, right);
}
