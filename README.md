# Purchase Transactions API

A production-ready .NET 8 application for managing purchase transactions with currency conversion capabilities, built following Clean Architecture, Domain-Driven Design (DDD), and SOLID principles.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Running Tests](#running-tests)
- [Design Decisions](#design-decisions)
- [Technical Highlights](#technical-highlights)

## Overview

This application allows users to:
- Store purchase transactions in USD
- Retrieve transactions converted to other currencies using real-time exchange rates from the U.S. Treasury Reporting Rates of Exchange API

## Architecture

The solution follows **Clean Architecture** (Hexagonal Architecture) principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                     API Layer                            │
│            (ASP.NET Core Minimal APIs)                   │
└───────────────────┬─────────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────────┐
│              Application Layer                           │
│     (Use Cases, DTOs, Interface Definitions)            │
└───────────────────┬─────────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────────┐
│               Domain Layer                               │
│   (Entities, Value Objects, Domain Services)            │
│          *** No External Dependencies ***                │
└──────────────────────────────────────────────────────────┘
                    ▲
┌───────────────────┴─────────────────────────────────────┐
│            Infrastructure Layer                          │
│  (Repositories, External APIs, Persistence)             │
└──────────────────────────────────────────────────────────┘
```

### Dependency Flow

- **Domain**: Pure business logic with zero external dependencies
- **Application**: Depends on Domain, defines interfaces (ports)
- **Infrastructure**: Implements Application interfaces (adapters)
- **API**: Thin layer orchestrating requests through Application handlers

## Project Structure

```
PurchaseTransactions/
├── src/
│   ├── PurchaseTransactions.Domain/
│   │   ├── Entities/              # Aggregate roots and entities
│   │   ├── ValueObjects/          # Immutable value objects
│   │   └── Services/              # Domain services
│   │
│   ├── PurchaseTransactions.Application/
│   │   ├── DTOs/                  # Data transfer objects
│   │   ├── Interfaces/            # Port definitions
│   │   └── UseCases/              # Command/Query handlers
│   │
│   ├── PurchaseTransactions.Infrastructure/
│   │   ├── Persistence/           # Repository implementations
│   │   └── ExternalServices/      # Treasury API client
│   │
│   └── PurchaseTransactions.Api/
│       └── Program.cs             # API endpoints and DI setup
│
└── tests/
    └── PurchaseTransactions.Tests/
        ├── Domain/                # Domain logic tests
        └── Application/           # Use case tests with mocks
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Internet connection (for Treasury API access)

### Installation

1. **Clone or navigate to the project directory**
   ```bash
   cd PurchaseTransactions
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/PurchaseTransactions.Api
   ```

   The API will start at `https://localhost:5001` (or as configured)

5. **Access Swagger UI**
   
   Navigate to `https://localhost:5001/swagger` in your browser to explore the API interactively.

## API Endpoints

### 1. Create Transaction

**POST** `/api/transactions`

Creates a new purchase transaction in USD.

**Request Body:**
```json
{
  "description": "Coffee at Starbucks",
  "transactionDate": "2024-06-15",
  "purchaseAmount": 15.50
}
```

**Response (201 Created):**
```json
{
  "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Validations:**
- Description: max 50 characters
- Transaction date: cannot be in the future
- Purchase amount: must be positive, rounded to 2 decimals

### 2. Get Converted Transaction

**GET** `/api/transactions/{id}/converted?currency={currency}`

Retrieves a transaction converted to the specified currency.

**Example:**
```
GET /api/transactions/3fa85f64-5717-4562-b3fc-2c963f66afa6/converted?currency=EUR
```

**Response (200 OK):**
```json
{
  "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Coffee at Starbucks",
  "transactionDate": "2024-06-15",
  "originalAmount": 15.50,
  "originalCurrency": "USD",
  "targetCurrency": "EUR",
  "exchangeRate": 0.85,
  "convertedAmount": 13.18
}
```

**Error Responses:**
- `404 Not Found`: Transaction doesn't exist
- `400 Bad Request`: No valid exchange rate available (within 6 months)

**Exchange Rate Rules:**
- Rate must be dated on or before the transaction date
- Rate must be within 6 months of the transaction date
- Uses most recent available rate from U.S. Treasury API

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage

The test suite covers:
- ✅ **Domain Validation**: Value objects, entities, business rules
- ✅ **Transaction Creation**: Valid and invalid scenarios
- ✅ **Currency Conversion**: Successful conversions and error cases
- ✅ **Exchange Rate Validation**: Date range checks, 6-month rule
- ✅ **Use Case Handlers**: With mocked dependencies

**Test Frameworks:**
- xUnit for test execution
- FluentAssertions for readable assertions
- Moq for mocking external dependencies

## Design Decisions

### 1. Value Objects Over Primitives

**Decision:** Wrapped primitives in value objects (Money, Description, TransactionId, etc.)

**Rationale:**
- Encapsulates validation logic
- Prevents invalid state at compile time
- Makes the domain model expressive and self-documenting
- Eliminates primitive obsession anti-pattern

**Trade-off:** More classes, but significantly better type safety and clarity

### 2. Domain Service for Currency Conversion

**Decision:** Created `CurrencyConversionService` as a domain service

**Rationale:**
- Conversion involves multiple entities (Transaction + ExchangeRate)
- Business rules (6-month window, date validation) belong in the domain
- Not a natural fit for any single entity

**Trade-off:** Could be in Application layer, but domain rules should stay in Domain

### 3. CQRS-lite Pattern

**Decision:** Separate command (CreateTransaction) and query (GetConvertedTransaction) handlers

**Rationale:**
- Clear separation of read vs write concerns
- Easier to test and maintain
- Scales better for complex scenarios

**Trade-off:** More code upfront, but better long-term maintainability

### 4. Repository Pattern

**Decision:** Abstracted persistence behind `IPurchaseTransactionRepository`

**Rationale:**
- Enables easy testing with mock implementations
- Allows switching persistence strategies without changing domain/application code
- Follows Dependency Inversion Principle

**Trade-off:** Extra abstraction layer, but crucial for testability

### 5. In-Memory Storage

**Decision:** Used `ConcurrentDictionary` for thread-safe in-memory storage

**Rationale:**
- Meets requirements (no external database)
- Production-ready with thread safety
- Simple and fast

**Trade-off:** Data lost on restart, but acceptable per requirements

### 6. HttpClient for External API

**Decision:** Used `IHttpClientFactory` for Treasury API calls

**Rationale:**
- Best practice for HTTP calls in .NET
- Handles connection pooling and disposal
- Easy to mock for testing

**Trade-off:** Requires network calls in production

### 7. No MediatR Initially

**Decision:** Direct handler invocation instead of MediatR

**Rationale:**
- Keeps solution simpler
- Fewer dependencies
- Easier to understand for code review

**Trade-off:** Could add later if cross-cutting concerns emerge

### 8. Minimal APIs

**Decision:** Used ASP.NET Core Minimal APIs instead of Controllers

**Rationale:**
- Modern, lightweight approach
- Less ceremony for simple CRUD operations
- Built-in OpenAPI support

**Trade-off:** Less structure than Controllers, but sufficient for this use case

## Technical Highlights

### Domain-Driven Design Elements

1. **Aggregates**: `PurchaseTransaction` as aggregate root
2. **Value Objects**: Immutable, self-validating (`Money`, `Description`, `TransactionId`, `ExchangeRate`)
3. **Entities**: Rich domain models with behavior, not anemic data bags
4. **Domain Services**: Business logic spanning multiple entities
5. **Ubiquitous Language**: Code reflects business concepts

### SOLID Principles Applied

- **S**ingle Responsibility: Each class has one reason to change
- **O**pen/Closed: Value objects and entities are closed for modification
- **L**iskov Substitution: Interface implementations are substitutable
- **I**nterface Segregation: Small, focused interfaces
- **D**ependency Inversion: Depend on abstractions, not concretions

### Clean Architecture Benefits

✅ **Testability**: Easy to test with mocked dependencies  
✅ **Maintainability**: Clear boundaries and responsibilities  
✅ **Flexibility**: Can swap implementations without touching business logic  
✅ **Independence**: Domain logic has zero external dependencies  
✅ **Scalability**: Easy to add new features without breaking existing code  

### Production Readiness

- Thread-safe in-memory repository
- Proper error handling and validation
- Comprehensive test coverage
- Swagger/OpenAPI documentation
- Dependency injection configured correctly
- No business logic in controllers/endpoints

## Key Architectural Patterns

1. **Ports and Adapters** (Hexagonal Architecture)
2. **Repository Pattern** (for data access abstraction)
3. **Command/Query Separation** (CQRS-lite)
4. **Value Object Pattern** (DDD)
5. **Domain Service Pattern** (DDD)
6. **Dependency Injection** (IoC)

## Future Enhancements

If this were to evolve, consider:
- Persistent database (PostgreSQL, SQL Server)
- Caching for exchange rates
- MediatR for cross-cutting concerns (logging, validation)
- API versioning
- Rate limiting
- Authentication/Authorization
- Distributed tracing
- Health checks
- Metrics and monitoring

## License

This is a demonstration project for educational purposes.

## Author

Kleber W A S Oliveira
