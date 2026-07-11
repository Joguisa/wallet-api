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

_TBD — docker-compose and setup instructions will be added with the persistence phase._

## API endpoints

_TBD_

## Deliberate omissions

_TBD — documented as phases are completed._
