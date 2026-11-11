# CLAUDE.md — Backend

Backend-specific guidance. Repository-wide rules are in the root `CLAUDE.md`.

## Overview

ASP.NET Core Web API on .NET 10 (C# 14), PostgreSQL via EF Core (Npgsql, snake_case naming), Serilog logging to PostgreSQL. Multi-tenant: every business entity belongs to a tenant. The domain model is still evolving; there are no tests yet.

## Commands

Run from `backend/`:

```bash
dotnet build TuntiQ.slnx
dotnet run --project TuntiQ.Api

# Add a migration (Package Manager Console)
Add-Migration <Name> -OutputDir Persistence/Migrations -Project TuntiQ.Infrastructure -StartupProject TuntiQ.Api
```

TypeGen (generates TypeScript types for the frontend) will be added later: `TypeGen generate`.

`TuntiQ.Infrastructure/readme.txt` is the developer's personal command cheat sheet. Keep it, and keep it in sync with this section.

## Architecture

| Project | Responsibility | References |
|---|---|---|
| `TuntiQ.Domain` | Aggregates, domain events, repository interfaces, validation primitives | Shared |
| `TuntiQ.Application` | Use-case services, validators, request/DTO messages | Domain, Shared |
| `TuntiQ.Infrastructure` | Infrastructure code: EF Core, repositories, caching, event dispatching | Application, Domain, Shared |
| `TuntiQ.Api` | Controllers, middleware, composition root | all |
| `TuntiQ.Shared` | General-purpose helpers that belong to no layer, e.g. `DateUtils`. No business concepts. Currently empty. | — |

The dependency direction is a rule, not just the current state. Do not add project or package references that break it (e.g. EF Core or ASP.NET in Domain). If new code seems to need one, use an interface in the inner layer or ask.

Code is organized by feature: `Domain/Dimensions/`, `Application/Dimensions/Messages/`, `Application/Dimensions/Services/`, etc.

## Rules

### General

1. **Namespaces:** Every file has a file-scoped namespace that matches its folder path (e.g. `TuntiQ.Domain.Dimensions`). No types in the global namespace.
2. **Async suffix:** Every method returning `Task`/`ValueTask` ends with `Async`. Exception: controller actions (ASP.NET strips the suffix, which breaks `CreatedAtAction(nameof(...))`).
3. **Guids:** Use `Guid.CreateVersion7()`. `Guid.NewGuid()` is banned (`BannedSymbols.txt`, build error). Version 7 Guids are time-ordered, so new ids are increasing, which keeps database indexes efficient.
4. **User-facing text** is bilingual: every validation message has `MessageEn` and `MessageFi`. All other strings (exception messages, logs, comments) are in English.
5. **Query in the database:** Filtering, sorting and paging run in the database via `IQueryable`. Loading a full set into memory is allowed only as a documented exception (see below) and must have a comment pointing here.

### Domain

**Structure and naming**

- Aggregates are grouped into feature folders with a plural namespace: `Dimension` → `TuntiQ.Domain.Dimensions`. The folder contains the aggregates, their events (`XxxEvents.cs`), repository interfaces (`IXxxRepository`) and query classes.
- A small, closely related aggregate shares the folder of its main aggregate instead of getting its own (e.g. `DimensionValue` lives in `Dimensions`; `EmployeeType` would live in `Employees`). It still has its own repository and service. This is the developer's decision: when creating a new aggregate, ask which folder it belongs to.
- When a repository lookup needs several parameters, wrap them in a `XxxQuery` class in the Domain (e.g. `DimensionValuePagedListQuery : PagedListQuery`). Application-side counterparts are `XxxRequest`.
- The `Entity` suffix is reserved for EF entities in Infrastructure. Never use it for domain types.
- `TuntiQ.Domain.Common` holds building blocks used by almost every aggregate and is imported via `GlobalUsings.cs`. Do not add `using TuntiQ.Domain.Common;` inside the Domain project.

**Aggregates**

- Aggregates inherit `AggregateRoot` and implement `ITenantScoped` (nearly all data is tenant-specific).
- An aggregate with a name in several languages implements `ILocalizedName` (`NameEn`, `NameFi`). The names are plain properties in the domain model but stored in a separate `xxx_names` table.
- The logged-in user and tenant are read from `CurrentUser`.
- Properties have `private set`.
- New instances are created through a static `Create(...)` factory that validates, trims input and raises a `XxxCreated` event. The public full constructor is only for rehydration from persistence.
- State changes go through `SetXxx(...)` methods that: call `EnsureNotDeleted()`, validate, return without an event if nothing changed, otherwise update state and `Raise(...)` an event.
- Deletion is soft: a `Delete()` method sets `DeletedAt`, exposes `Deleted => DeletedAt is not null` and raises `XxxDeleted`. The change is saved with the repository's `UpdateAsync`; repositories of soft-deletable aggregates have no `DeleteAsync` method.
- Validation constants (e.g. `MaxNameLength`) are `const` on the aggregate. Infrastructure reads them from there; never duplicate the value.
- Enum type names are plural: `DimensionKinds`, `OrderDirections`.

**Validation and exceptions**

- Use `ErrorCollector` to report several errors at once (`ThrowIfAny()`). Throwing a `ValidationException` directly on the first error is also fine.
- Error keys are property names (`nameof(NameEn)`).
- Validate in stages: if the input already has errors, throw before any check that needs the database. Never call a repository while errors are known. Example: `Name1` and `Name2` are both required and unique; if `Name1` is missing, do not query whether `Name2` is unique.
- `ValidationException` is shown to the end user through the API and UI. Its messages must never contain sensitive information.
- Situations that should never happen with a correctly working client (e.g. creating data for another tenant) are not validation errors. Throw a dedicated exception such as `TenantMismatchException` (guard with `TenantMismatchException.ThrowIfMismatch(...)`), which is not exposed to the user.
- **Domain models must be serializable with `System.Text.Json`** (they are cached in serialized form):
  - The full rehydration constructor is marked `[JsonConstructor]` and takes every piece of persisted state as a parameter. Parameter names match property names (`nameEn` ↔ `NameEn`).
  - Computed properties (e.g. `Deleted`) are marked `[JsonIgnore]`.
  - No persisted state outside the constructor parameters. Do not add state that can only be set through methods.
  - Pending domain events are not serialized; only persisted state is.
  - A new aggregate or a new property must survive a serialize → deserialize round trip unchanged.

### Domain events

- One file per aggregate: `XxxEvents.cs`, containing `sealed record`s that inherit `DomainEvent`.
- Named in past tense (`DimensionRenamed`, `DimensionEnabled`) and always include the aggregate id and `TenantId`.
- Rename events carry both old and new values.
- Events are an audit log, not event sourcing. They are serialized to JSON when saved and never deserialized back into event types; when read, they are returned as raw JSON. Events may therefore be changed or renamed freely, but must remain serializable with `System.Text.Json`.
- Events are persisted to the aggregate's `*_events` table (jsonb) in the same `SaveChanges` as the state change, then dispatched to `IDomainEventHandler<T>` implementations after saving.

### Application

**Structure**

- Folders and namespaces mirror the Domain feature folders (`Dimensions/`), including shared folders for closely related aggregates.
- `Xxx/Messages/`: everything that crosses the API boundary — requests, DTOs and their enums. **All** public input and output types live under a `Messages` namespace, so that TypeGen can generate TypeScript types from them. Never put a request or DTO elsewhere.
- `Xxx/Services/`: logic — services (command handlers) and validators.
- `Common/Messages/`: DTOs and requests shared by all features (`PagedListDto`, `PagedListRequest`, `ErrorResponseDto`). `Common/` (outside `Messages`) is for shared services.

**Messages**

- The Application layer never returns domain objects. It always maps them to DTOs.
- Naming: returned objects are `XxxDto` (`Dimension` → `DimensionDto`). Requests (CQRS commands and queries) end with `Request`: `XxxCreateRequest`, `XxxUpdateRequest`, `XxxPagedListRequest`.
- Use `class` with `required` + `init` properties.
- Requests and DTOs never contain `TenantId`. The tenant always comes from the authenticated user (`CurrentUser.TenantId`).
- DTOs never expose domain enums. Define a `XxxDtoKinds`-style enum and map with `ToDomain()` / `ToDto()` extension members.
- Mapping domain → DTO is a static `XxxDto.ToDto(domain)` method.

**Services**

- Services (`XxxService`) orchestrate: load, validate, call domain methods, save. Business rules belong in the domain.
- Validation logic lives in validator classes (`XxxValidator`), not in services. Validators hold rules that need repository access (e.g. name uniqueness); rules that the aggregate can check on its own stay in the domain.
- The only check a service does itself is whether the loaded object exists: `if (x is null) throw ValidationException.NotFound();`.
- Register services and validators in `Application/DependencyInjection.cs`.

### Infrastructure

**Structure**

- `Persistence/`: repository implementations of the Domain interfaces (`XxxRepository : IXxxRepository`) and the `DbContext`.
- `Persistence/Ef/`: everything EF Core specific — entities, base classes, serializers.
- `Persistence/Migrations/`: generated EF migrations. Do not edit by hand. During early development the developer regenerates `InitialCreate` instead of adding new migrations, so do not create migrations; change the model and tell the developer that the migration needs to be regenerated.
- `Caching/`: cache helpers.

**EF entities**

- Named after the domain model with an `Entity` suffix: `Dimension` → `DimensionEntity`. Child tables follow the same pattern: `DimensionNameEntity`, `DimensionEventEntity`.
- Each aggregate entity has `FromDomain`, `UpdateFrom` and `ToDomain`.
- Localized names are stored in a separate `xxx_names` table with one row per language (`en`, `fi`).
- Tenant and soft-delete filtering are applied globally by query filters via `ITenantScopedEntity` and `ISoftDeletableEntity`. Do not add these conditions manually.

**Repositories**

- Repositories implement the domain interface and return domain objects, never EF entities.
- Read queries use `AsNoTracking()`. `UpdateAsync` returns early if the aggregate has no domain events.
- Register repositories in `Infrastructure/DependencyInjection.cs`.

**Database queries**

- Filter, join and sort only on indexed columns. When a new query needs a column that has no index, add the index (in `TuntiQDbContext.OnModelCreating`) in the same change.
- Tenant-scoped indexes start with `TenantId`, because every query is filtered by tenant.
- Indexes on soft-deletable tables are partial: `.HasFilter("deleted = false")`.
- Substring search (`ILIKE '%term%'`) needs a trigram GIN index: `.HasMethod("gin").HasOperators("gin_trgm_ops")`. The `pg_trgm` extension is enabled. Localized name tables have one on `name`.

### API

The API is at an early stage.

**Controller groups**

- `Controllers/App/`: controllers that serve the user interfaces (Svelte, Flutter). They may return Application-layer DTOs and requests directly.
- `Controllers/Integration/`: controllers for external integrations (none yet). They will differ from App controllers and use their own DTOs instead of Application DTOs, so the integration contract stays stable when the UI contract changes. Ask before creating the first one.

**Conventions**

- One controller per aggregate, even when aggregates share a feature folder: `DimensionController`, `DimensionValueController`.
- Controllers are named in singular form: `DimensionController`, `EmployeeController`. File name matches the class name.
- Routes are plural and kebab-case: `api/app/dimensions`.
- Standard endpoints: `GET` (paged list, `[FromQuery]`), `GET {id:guid}`, `POST by-ids` (body: list of ids), `POST` (create, returns `201` with id), `PUT` (update, id in body, returns `204`), `DELETE {id:guid}` (returns `204`; takes the id only, no request object).
- Controllers are thin: call one service method, no logic.
- Errors: `ValidationException` is converted by `ValidationExceptionHandler` into `ErrorResponseDto`, always with status `400`. Clients distinguish error types by `ErrorDto.Code` (`Required`, `Taken`, `NotFound`, ...), not by HTTP status.

## Documented exceptions

- **In-memory filtering of dimensions:** `DimensionService.GetPagedListAsync` and `DimensionValidator` work on the full list from `IDimensionRepository.GetAllAsync()`, because the number of dimensions per tenant is small. The list is cached (see Caching).

## Caching

- `IMemoryCache` (registered in `Infrastructure/DependencyInjection.cs`). Caching is an Infrastructure concern: repositories cache, services do not know about it.
- Cache keys are per tenant: `"<aggregate>:{tenantId}"`, e.g. `dimensions:{tenantId}`. Never cache results read as the system user (cross-tenant).
- Cache domain objects through `cache.GetOrCreateSerializedAsync(...)` (`Infrastructure/Caching/MemoryCacheExtensions.cs`). It stores the serialized form and returns a fresh copy on every read, because domain objects are mutable and must not be shared between requests. Do not cache domain objects with plain `GetOrCreate`/`Set`.
- Invalidate in the repository's `SaveAsync` right after `SaveChangesAsync()`, so every write path clears the cache.
- Absolute expiration (30 min) is a safety net only; correctness relies on invalidation.
- Single-instance only. When the API is scaled to multiple instances, switch to `HybridCache` with a distributed backend.

## Current state

- Authentication is not yet in place. `CurrentUser` is set from the `sub` and `tenant_id` claims in `Program.cs`; without them, operations that need a tenant throw.
- The `/serilog-ui` log viewer has no access control yet (development only).
