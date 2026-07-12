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

_TBD_

## Deliberate omissions

_TBD — documented as phases are completed._
