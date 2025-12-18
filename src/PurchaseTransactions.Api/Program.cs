using PurchaseTransactions.Application.DTOs;
using PurchaseTransactions.Application.Interfaces;
using PurchaseTransactions.Application.UseCases;
using PurchaseTransactions.Domain.Services;
using PurchaseTransactions.Infrastructure.ExternalServices;
using PurchaseTransactions.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Purchase Transactions API", Version = "v1" });
});

// Register Domain Services
builder.Services.AddScoped<CurrencyConversionService>();

// Register Application Handlers
builder.Services.AddScoped<CreatePurchaseTransactionHandler>();
builder.Services.AddScoped<GetConvertedTransactionHandler>();

// Register Infrastructure - Persistence
builder.Services.AddSingleton<IPurchaseTransactionRepository, InMemoryPurchaseTransactionRepository>();

// Register Infrastructure - External Services
builder.Services.AddHttpClient<IExchangeRateProvider, TreasuryExchangeRateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// API Endpoints

/// <summary>
/// POST /api/transactions - Create a new purchase transaction
/// </summary>
app.MapPost("/api/transactions", async (
    CreatePurchaseTransactionRequest request,
    CreatePurchaseTransactionHandler handler,
    CancellationToken cancellationToken) =>
{
    try
    {
        var response = await handler.HandleAsync(request, cancellationToken);
        return Results.Created($"/api/transactions/{response.TransactionId}", response);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "An error occurred while creating the transaction");
    }
})
.WithName("CreateTransaction")
.WithOpenApi()
.Produces<CreatePurchaseTransactionResponse>(StatusCodes.Status201Created)
.Produces<object>(StatusCodes.Status400BadRequest)
.Produces<object>(StatusCodes.Status500InternalServerError);

/// <summary>
/// GET /api/transactions/{id}/converted?currency={currency} - Get transaction converted to target currency
/// </summary>
app.MapGet("/api/transactions/{id:guid}/converted", async (
    Guid id,
    string currency,
    GetConvertedTransactionHandler handler,
    CancellationToken cancellationToken) =>
{
    try
    {
        var response = await handler.HandleAsync(id, currency, cancellationToken);
        return Results.Ok(response);
    }
    catch (TransactionNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (ExchangeRateNotFoundException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "An error occurred while retrieving the transaction");
    }
})
.WithName("GetConvertedTransaction")
.WithOpenApi()
.Produces<ConvertedTransactionResponse>(StatusCodes.Status200OK)
.Produces<object>(StatusCodes.Status400BadRequest)
.Produces<object>(StatusCodes.Status404NotFound)
.Produces<object>(StatusCodes.Status500InternalServerError);

app.Run();
