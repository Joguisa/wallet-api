# Wallet API

REST API for managing wallets and balance transfers, built with .NET 8, Clean Architecture and DDD.

> Work in progress — sections are filled in as each phase is completed.

## Tech stack

- .NET 8 (Minimal APIs)
- Entity Framework Core + SQL Server
- xUnit, FluentAssertions, NSubstitute (unit tests)
- JWT Bearer authentication

## Solution structure

```
src/
  WalletApi.Domain           Entities, value objects, domain services, domain errors. No dependencies.
  WalletApi.Application      Use cases (command/query handlers), validation, abstractions.
  WalletApi.Infrastructure   EF Core persistence, repositories, JWT generation.
  WalletApi.Api              Minimal API endpoints, middleware, composition root.
tests/
  WalletApi.UnitTests        Domain and application unit tests.
  WalletApi.IntegrationTests End-to-end API tests.
```

## Getting started

Prerequisites: .NET 8 SDK and Docker.

```bash
# 1. Start SQL Server (waits until healthy)
docker compose up -d

# 2. Run the API — applies EF Core migrations automatically in Development
dotnet run --project src/WalletApi.Api
```

Swagger UI is available at the URL printed in the console (`/swagger`).

To regenerate migrations, restore the local tools first: `dotnet tool restore`, then use `dotnet ef`.

> The SQL Server password in `docker-compose.yml` and `appsettings.Development.json` is for local development only; production would use a secret store and run migrations as a deployment step.

## API endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/api/wallets` | Creates a wallet. Returns 201 with a Location header |
| GET | `/api/wallets/{id}` | Gets a wallet by id |
| POST | `/api/transfers` | Transfers balance between wallets (atomic debit + credit). Requires an `Idempotency-Key` header (GUID); repeating a request with the same key returns the original transfer without executing again |
| GET | `/api/wallets/{walletId}/movements?page=1&pageSize=20` | Paginated movement history, newest first (`pageSize` ≤ 100) |

Errors follow [RFC 7807 ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807) with an `errorCode` extension, e.g.:

```json
{
  "title": "Business rule violated",
  "status": 422,
  "detail": "Wallet 1 has balance 50.00, debit of 75.00 was rejected.",
  "errorCode": "INSUFFICIENT_FUNDS",
  "traceId": "..."
}
```

Sample requests for every endpoint live in [`src/WalletApi.Api/WalletApi.Api.http`](src/WalletApi.Api/WalletApi.Api.http).

## Design decisions

- **Minimal APIs + Vertical Slice Architecture:** Endpoints only handle HTTP concerns, while business logic lives in application handlers.
- **No MediatR:** Use cases implement a lightweight `IRequestHandler<TRequest, TResponse>` abstraction resolved from DI. This keeps the architecture simple while preserving an easy migration path to MediatR if needed.
- **Lean domain model:** Wallets are USD-only, so `Money` does not model a currency. `Movement` is kept outside the `Wallet` aggregate to avoid loading transaction history for balance updates, while `Transfer` models the business event and idempotency by linking the debit and credit movements.
- **Centralized error handling:** Typed `DomainException`s are translated into RFC 7807 responses through a single `IExceptionHandler`.
- **Defense in depth:** Balance integrity is enforced through domain rules, optimistic concurrency (`RowVersion`), and a database `CHECK` constraint.

## Deliberate omissions

_TBD — documented as phases are completed._
