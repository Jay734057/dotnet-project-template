# Changelog

All notable changes to this project are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-08-14

Initial scaffold of the BackendBase .NET 10 backend base project.

### Added

- **Solution** `BackendBase` (.NET 10, `.slnx` format) with Clean Architecture
  layering: `Domain`, `Application`, `Infrastructure`, `Api`, and a `UnitTests`
  project.
- **Product resource** with full CRUD and name search:
  - `GET /api/products` — case-insensitive name search with paging and sorting
    (`name` / `price` / `createdAt`), returning a `PagedResult<T>`.
  - `GET /api/products/{id}`, `POST`, `PUT /{id}`, `DELETE /{id}`.
- **CQRS via MediatR** — Create/Update/Delete commands and GetById/Search queries,
  each with its own handler.
- **Validation** — FluentValidation validators run automatically through a
  `ValidationBehavior` MediatR pipeline behavior.
- **Persistence** — EF Core 10 with Repository + Unit of Work; `AppDbContext`
  stamps `IAuditable` timestamps in its `SaveChanges` override.
- **Config-driven database provider** — `Database:Provider` selects
  `InMemory` (default), `SqlServer`, or `PostgreSql` with no code change; relational
  providers require `ConnectionStrings:DefaultConnection` (fails fast if missing).
- **Authentication & authorization** — JWT bearer auth with policy/role-based
  authorization (`Products.Read`, `Products.Write`; roles `Reader`/`Writer`/`Admin`).
- **Development-only token helper** (`POST /api/dev/token`) to mint test JWTs,
  removed from routing and Swagger outside Development via a controller convention.
- **Swagger / OpenAPI** — Swashbuckle with XML doc comments from every layer,
  response-type annotations, and a Bearer **Authorize** button.
- **Global exception handling** — `ExceptionHandlingMiddleware` maps
  `NotFoundException` → 404 and `ValidationException` → 400 (with field errors),
  everything else → 500, all as RFC 7807 `ProblemDetails`.
- **Options pattern** — `ApiOptions`, `JwtOptions` (validated on startup),
  `DatabaseOptions`.
- **Tests** — xUnit + Moq + FluentAssertions + EF InMemory: validator, handler,
  and repository (search/sort/paging + audit timestamp) coverage. 25 tests.
- **Tooling** — `global.json` pinning SDK 10.0.103; `launchSettings.json` opens
  `/swagger`; `.gitignore`; `README.md`.

### Notes

- FluentAssertions pinned to 7.2.2 (last free Apache-2.0 release; 8.x is
  commercially licensed).
- MediatR 14 is commercially licensed (free under a revenue threshold); retained
  as the core CQRS mechanism.

[Unreleased]: https://example.com/compare/v0.1.0...HEAD
[0.1.0]: https://example.com/releases/tag/v0.1.0
