namespace PurchaseTransactions.Domain.ValueObjects;

/// <summary>
/// Value Object representing a unique transaction identifier.
/// Strongly-typed wrapper around Guid.
/// </summary>
public sealed class TransactionId : IEquatable<TransactionId>
{
    public Guid Value { get; }

    private TransactionId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new unique TransactionId.
    /// </summary>
    public static TransactionId Create() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a TransactionId from an existing Guid value.
    /// </summary>
    public static TransactionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TransactionId cannot be empty.", nameof(value));
        }

        return new TransactionId(value);
    }

    /// <summary>
    /// Parses a string representation of a TransactionId.
    /// </summary>
    public static TransactionId Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("TransactionId string cannot be null or empty.", nameof(value));
        }

        if (!Guid.TryParse(value, out var guid))
        {
            throw new ArgumentException($"Invalid TransactionId format: {value}", nameof(value));
        }

        return From(guid);
    }

    public bool Equals(TransactionId? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj) => obj is TransactionId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(TransactionId id) => id.Value;

    public static bool operator ==(TransactionId? left, TransactionId? right) => Equals(left, right);
    public static bool operator !=(TransactionId? left, TransactionId? right) => !Equals(left, right);
}
