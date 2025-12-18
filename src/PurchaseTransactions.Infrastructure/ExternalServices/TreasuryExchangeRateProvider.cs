using System.Text.Json;
using System.Text.Json.Serialization;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Domain.ValueObjects;

namespace PurchaseTransactions.Infrastructure.ExternalServices;

/// <summary>
/// Implementation of IExchangeRateProvider that retrieves rates from the U.S. Treasury API.
/// https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange
/// </summary>
public class TreasuryExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.fiscaldata.treasury.gov/services/api/fiscal_service";
    private const string Endpoint = "/v1/accounting/od/rates_of_exchange";

    public TreasuryExchangeRateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ExchangeRate?> GetExchangeRateAsync(
        string targetCurrency,
        DateOnly transactionDate,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetCurrency))
        {
            throw new ArgumentException("Target currency cannot be null or empty.", nameof(targetCurrency));
        }

        // Calculate date range: 6 months before transaction date to transaction date
        var sixMonthsBefore = transactionDate.AddMonths(-6);
        
        // Build the API URL with filters
        var url = $"{BaseUrl}{Endpoint}" +
                  $"?filter=currency:eq:{targetCurrency.ToUpperInvariant()}" +
                  $",record_date:gte:{sixMonthsBefore:yyyy-MM-dd}" +
                  $",record_date:lte:{transactionDate:yyyy-MM-dd}" +
                  $"&sort=-record_date" + // Sort by most recent first
                  $"&page[size]=1"; // We only need the most recent rate

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Log error in production, for now return null
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<TreasuryApiResponse>(content);

            if (apiResponse?.Data == null || apiResponse.Data.Count == 0)
            {
                return null; // No exchange rate found
            }

            var rateData = apiResponse.Data[0];

            // Parse the exchange rate value
            if (!decimal.TryParse(rateData.ExchangeRate, out var rate))
            {
                return null; // Invalid rate format
            }

            // Parse the effective date
            if (!DateOnly.TryParse(rateData.RecordDate, out var effectiveDate))
            {
                return null; // Invalid date format
            }

            // Create and return the ExchangeRate value object
            return ExchangeRate.Create(rate, "USD", targetCurrency, effectiveDate);
        }
        catch (HttpRequestException)
        {
            // Network error - in production, log this
            return null;
        }
        catch (JsonException)
        {
            // JSON parsing error - in production, log this
            return null;
        }
    }
}

/// <summary>
/// Response model for the Treasury API.
/// </summary>
internal class TreasuryApiResponse
{
    [JsonPropertyName("data")]
    public List<TreasuryRateData> Data { get; set; } = new();
}

/// <summary>
/// Data model for a single exchange rate record from the Treasury API.
/// </summary>
internal class TreasuryRateData
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("exchange_rate")]
    public string ExchangeRate { get; set; } = string.Empty;

    [JsonPropertyName("record_date")]
    public string RecordDate { get; set; } = string.Empty;
}
