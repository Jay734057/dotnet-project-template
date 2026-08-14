# BackendBase

A production-shaped **.NET 10** base backend API to start every new backend
service from. It ships one fully implemented `Product` resource (CRUD + name
search) as the reference pattern to copy for real features, wired with Clean
Architecture, CQRS, validation, JWT/role authorization, a swappable database,
and complete Swagger docs.

> The business logic inside handlers is intentionally a thin placeholder — the
> **plumbing around it is the point**. Copy the `Product` slice to add real
> resources.

---

## Table of contents

- [Tech stack](#tech-stack)
- [Architecture](#architecture)
- [Getting started](#getting-started)
- [API surface](#api-surface)
- [Authentication & authorization](#authentication--authorization)
- [Switching the database](#switching-the-database)
- [Configuration reference](#configuration-reference)
- [Testing](#testing)
- [Adding a new resource](#adding-a-new-resource)
- [Dependency & licensing notes](#dependency--licensing-notes)
- [Project layout](#project-layout)

---

## Tech stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 (LTS) |
| API | ASP.NET Core Web API (controllers) |
| Architecture | Clean Architecture + CQRS |
| Mediator | MediatR |
| Validation | FluentValidation (via a MediatR pipeline behavior) |
| Data access | EF Core 10 (Repository + Unit of Work) |
| Database | InMemory by default; SqlServer / PostgreSQL by config |
| Auth | JWT bearer + policy/role-based authorization |
| API docs | Swashbuckle (Swagger UI) with XML comments from every layer |
| Tests | xUnit + Moq + FluentAssertions + EF InMemory |

---

## Architecture

Four projects with dependencies pointing **inward only**
(`Api`/`Infrastructure` → `Application` → `Domain`):

```
BackendBase.Api            HTTP surface: controllers, auth, Swagger, middleware, options
   │        │
   │        └── BackendBase.Infrastructure   EF Core: DbContext, repositories, provider switch
   │                     │
   └── BackendBase.Application               CQRS handlers, validators, DTOs, interfaces
                         │
              BackendBase.Domain             Entities, domain exceptions — zero dependencies
```

Key patterns:

- **CQRS** — every use case is a `Command` (write) or `Query` (read) with its own
  handler under `Application/Products/{Commands,Queries}`, dispatched via MediatR.
- **Validation as a pipeline behavior** — `ValidationBehavior` runs every
  `FluentValidation` validator before the handler; handlers never validate their
  own input.
- **Repository + Unit of Work** — handlers depend on `IProductRepository` /
  `IUnitOfWork`, never on `DbContext`. One atomic `SaveChangesAsync` per request.
- **Central exception mapping** — `ExceptionHandlingMiddleware` turns
  `NotFoundException` → 404 and `ValidationException` → 400, everything else → 500,
  all as RFC 7807 `ProblemDetails`.
- **Options pattern** — all config is bound to strongly-typed classes
  (`ApiOptions`, `JwtOptions`, `DatabaseOptions`); no ad-hoc string lookups.
- **Automatic audit timestamps** — entities implementing `IAuditable` get
  `CreatedAt`/`UpdatedAt` stamped in the `DbContext.SaveChanges` override.

---

## Getting started

Requires the **.NET 10 SDK** (`global.json` pins `10.0.103`).

```bash
dotnet restore
dotnet build
dotnet run --project src/BackendBase.Api
```

The app starts in **Development** and opens Swagger:

- Swagger UI: <http://localhost:5035/swagger>
- HTTPS profile: <https://localhost:7193/swagger> (`--launch-profile https`)

The default InMemory database resets on every restart — that is expected, not a bug.

### Try it in Swagger (30 seconds)

1. Expand **Dev (local only) → `POST /api/dev/token`**, execute with a body like
   `{ "roles": ["Admin"] }`, and copy the `accessToken`.
2. Click **Authorize** (top-right), paste the token, and confirm.
3. Call the `Products` endpoints — create, search, update, delete.

---

## API surface

Base route: `/api/products`

| Method | Route | Policy | Description |
|---|---|---|---|
| `GET` | `/api/products` | `Products.Read` | Search by name, paged & sorted |
| `GET` | `/api/products/{id}` | `Products.Read` | Get one product |
| `POST` | `/api/products` | `Products.Write` | Create |
| `PUT` | `/api/products/{id}` | `Products.Write` | Full update |
| `DELETE` | `/api/products/{id}` | `Products.Write` | Delete |
| `POST` | `/api/dev/token` | *(none, dev only)* | Mint a test JWT |

**Search query parameters:**

| Param | Type | Default | Notes |
|---|---|---|---|
| `name` | string? | — | Case-insensitive substring match; omit for all |
| `page` | int | `1` | 1-based |
| `pageSize` | int | `20` | 1–100 |
| `sortBy` | string | `name` | `name` \| `price` \| `createdAt` |
| `descending` | bool | `false` | |

Search responses are wrapped in a `PagedResult<T>` with `items`, `page`,
`pageSize`, `totalCount`, `totalPages`, `hasNextPage`, `hasPreviousPage`.

---

## Authentication & authorization

- **Authentication**: JWT bearer. Tokens are validated for issuer, audience,
  signing key, and lifetime (see `Program.cs`).
- **Authorization**: policy-based, defined once in `AuthorizationPolicies`:
  - `Products.Read` — satisfied by roles `Reader`, `Writer`, or `Admin`
  - `Products.Write` — satisfied by roles `Writer` or `Admin`
- Controllers reference policies via `[Authorize(Policy = ...)]`, so rules live in
  one place, not as inline role strings.

### The dev token helper

`DevTokenController` (`POST /api/dev/token`) mints signed JWTs so you can exercise
the secured endpoints without a real identity provider. It:

- is **Development-only** — removed from routing *and* Swagger in other
  environments by `DevOnlyControllerConvention`;
- performs **no authentication** — it hands out whatever roles you request.

In a real deployment, tokens come from your identity provider (Entra ID, Auth0,
IdentityServer, …). **Delete `DevTokenController` and `JwtTokenService` once that
is wired in.**

---

## Switching the database

The provider is selected by configuration — **no code change** required. Set
`Database:Provider` and supply a connection string:

```jsonc
// appsettings.json (or environment-specific override)
{
  "Database": { "Provider": "PostgreSql" },      // InMemory | SqlServer | PostgreSql
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=backendbase;Username=app;Password=..."
  }
}
```

Provider packages for SQL Server and PostgreSQL are already referenced. If you
select a relational provider without a connection string, the app fails fast at
startup with a clear message.

> **Migrations:** the InMemory provider does not use them. When you move to a
> relational provider, add EF Core migrations:
> ```bash
> dotnet tool install --global dotnet-ef
> dotnet ef migrations add InitialCreate --project src/BackendBase.Infrastructure --startup-project src/BackendBase.Api
> dotnet ef database update --project src/BackendBase.Infrastructure --startup-project src/BackendBase.Api
> ```

---

## Configuration reference

| Section | Key | Purpose |
|---|---|---|
| `Api` | `Title`, `Version` | Swagger document metadata |
| `Database` | `Provider` | `InMemory` \| `SqlServer` \| `PostgreSql` |
| `Database` | `InMemoryDatabaseName` | Name for the InMemory store |
| `ConnectionStrings` | `DefaultConnection` | Used by relational providers |
| `Jwt` | `Issuer`, `Audience` | Validated on incoming tokens |
| `Jwt` | `SigningKey` | Symmetric key, **≥ 32 chars**; validated on startup |
| `Jwt` | `ExpiryMinutes` | Lifetime of dev-minted tokens |

**Secrets never belong in committed JSON.** `appsettings.Development.json` contains
a throwaway dev signing key so the app runs out of the box locally. For any real
environment, supply `Jwt:SigningKey` and `ConnectionStrings:DefaultConnection`
via user-secrets or a secrets manager:

```bash
cd src/BackendBase.Api
dotnet user-secrets set "Jwt:SigningKey" "a-long-random-secret-at-least-32-characters"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."
```

---

## Testing

```bash
dotnet test
```

The `BackendBase.UnitTests` project covers:

- **Validators** — Create and Search rules (name required/length, price ≥ 0,
  paging bounds, sort-field allow-list).
- **Handlers** (Moq) — create persists + commits once; get/update/delete throw
  `NotFoundException` when missing; update/delete don't commit on failure.
- **Repository** (EF InMemory) — case-insensitive name filter, sorting, paging +
  total count, and automatic audit-timestamp stamping.

---

## Adding a new resource

Copy the `Product` slice end to end:

1. **Domain** — add the entity under `Domain/Entities` (implement `IAuditable` for
   timestamps).
2. **Application** — add `Commands/`, `Queries/`, a `Dtos/` response, validators,
   and an `I<Entity>Repository` interface.
3. **Infrastructure** — add a `DbSet<>`, an `IEntityTypeConfiguration<>`, the
   repository implementation, and register it in `DependencyInjection`.
4. **Api** — add a thin controller that only dispatches via MediatR; add
   authorization policies in `AuthorizationPolicies` if needed.
5. **Tests** — mirror the `Products` test folder.

MediatR and FluentValidation are registered by assembly scan, so new handlers and
validators are picked up automatically.

---

## Dependency & licensing notes

- **MediatR 14** is commercially licensed (free under a revenue threshold; a paid
  license is required above it). It is central to the CQRS pattern here. If your
  org needs to avoid it, the mediator can be replaced with hand-written dispatch
  interfaces without touching handler logic.
- **FluentAssertions** is pinned to **7.2.2**, the last version under the free
  Apache-2.0 license (8.x moved to a paid commercial license).
- All other dependencies (EF Core, FluentValidation, Swashbuckle, Npgsql, Moq,
  xUnit) are free/open-source.

---

## Project layout

```
BackendBase.slnx
global.json
src/
  BackendBase.Domain/
    Common/IAuditable.cs
    Entities/Product.cs
    Exceptions/NotFoundException.cs
  BackendBase.Application/
    Common/{Behaviors,Interfaces,Models}/
    Products/{Commands,Queries,Dtos}/
    DependencyInjection.cs
  BackendBase.Infrastructure/
    Persistence/{AppDbContext,UnitOfWork,DatabaseOptions}.cs
    Persistence/{Configurations,Repositories}/
    DependencyInjection.cs
  BackendBase.Api/
    Controllers/{ProductsController,DevTokenController}.cs
    Authorization/AuthorizationPolicies.cs
    Middleware/ExceptionHandlingMiddleware.cs
    Options/{ApiOptions,JwtOptions}.cs
    Security/JwtTokenService.cs
    Infrastructure/{DevOnlyAttribute,DevOnlyControllerConvention}.cs
    Program.cs, appsettings*.json
tests/
  BackendBase.UnitTests/
    Products/  Persistence/  TestHelpers/
```

See [CHANGELOG.md](CHANGELOG.md) for the version history.
