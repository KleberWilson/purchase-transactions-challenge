namespace PurchaseTransactions.Domain.ValueObjects;

/// <summary>
/// Value Object representing a transaction description.
/// Immutable and self-validating with max 50 characters constraint.
/// </summary>
public sealed class Description : IEquatable<Description>
{
    private const int MaxLength = 50;
    
    public string Value { get; }

    private Description(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Description instance with validation.
    /// </summary>
    public static Description Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Description cannot be null or empty.", nameof(value));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MaxLength)
        {
            throw new ArgumentException($"Description cannot exceed {MaxLength} characters.", nameof(value));
        }

        return new Description(trimmedValue);
    }

    public bool Equals(Description? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => obj is Description other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static implicit operator string(Description description) => description.Value;

    public static bool operator ==(Description? left, Description? right) => Equals(left, right);
    public static bool operator !=(Description? left, Description? right) => !Equals(left, right);
}
