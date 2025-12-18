namespace PurchaseTransactions.Domain.ValueObjects;

/// <summary>
/// Value Object representing an exchange rate between two currencies.
/// Immutable with metadata about the rate's effective date.
/// </summary>
public sealed class ExchangeRate : IEquatable<ExchangeRate>
{
    public decimal Rate { get; }
    public string SourceCurrency { get; }
    public string TargetCurrency { get; }
    public DateOnly EffectiveDate { get; }

    private ExchangeRate(decimal rate, string sourceCurrency, string targetCurrency, DateOnly effectiveDate)
    {
        Rate = rate;
        SourceCurrency = sourceCurrency;
        TargetCurrency = targetCurrency;
        EffectiveDate = effectiveDate;
    }

    /// <summary>
    /// Creates a new ExchangeRate instance with validation.
    /// </summary>
    public static ExchangeRate Create(decimal rate, string sourceCurrency, string targetCurrency, DateOnly effectiveDate)
    {
        if (rate <= 0)
        {
            throw new ArgumentException("Exchange rate must be positive.", nameof(rate));
        }

        if (string.IsNullOrWhiteSpace(sourceCurrency))
        {
            throw new ArgumentException("Source currency cannot be null or empty.", nameof(sourceCurrency));
        }

        if (string.IsNullOrWhiteSpace(targetCurrency))
        {
            throw new ArgumentException("Target currency cannot be null or empty.", nameof(targetCurrency));
        }

        if (sourceCurrency.Length != 3 || targetCurrency.Length != 3)
        {
            throw new ArgumentException("Currency codes must be 3 characters (ISO 4217).");
        }

        return new ExchangeRate(rate, sourceCurrency.ToUpperInvariant(), targetCurrency.ToUpperInvariant(), effectiveDate);
    }

    /// <summary>
    /// Converts an amount using this exchange rate.
    /// </summary>
    public Money Convert(Money amount)
    {
        if (amount.Currency != SourceCurrency)
        {
            throw new InvalidOperationException(
                $"Cannot convert {amount.Currency} with exchange rate from {SourceCurrency} to {TargetCurrency}.");
        }

        var convertedAmount = amount.Amount * Rate;
        return Money.Create(convertedAmount, TargetCurrency);
    }

    public bool Equals(ExchangeRate? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Rate == other.Rate 
               && SourceCurrency == other.SourceCurrency 
               && TargetCurrency == other.TargetCurrency 
               && EffectiveDate.Equals(other.EffectiveDate);
    }

    public override bool Equals(object? obj) => obj is ExchangeRate other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Rate, SourceCurrency, TargetCurrency, EffectiveDate);

    public override string ToString() => $"1 {SourceCurrency} = {Rate:F4} {TargetCurrency} (effective {EffectiveDate})";

    public static bool operator ==(ExchangeRate? left, ExchangeRate? right) => Equals(left, right);
    public static bool operator !=(ExchangeRate? left, ExchangeRate? right) => !Equals(left, right);
}
