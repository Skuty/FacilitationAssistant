# Database Strategy Plan — Multi-Provider Support

## Executive Summary

Introduce a provider-agnostic data access layer that supports **Azure Cosmos DB**, **Microsoft SQL Server**, and is extensible for future providers (e.g., PostgreSQL). The current codebase is tightly coupled to the Cosmos DB EF Core provider through Cosmos-specific model configuration (`ToContainer`, `HasNoDiscriminator`, partition keys) and a dedicated `CosmosDbInitializer`. This plan describes how to decouple the infrastructure so the same Core domain works unchanged across all providers while each provider can optimise for its own strengths.

**Status:** Planning Phase  
**Priority:** Medium-High  
**Estimated Effort:** 5–8 days (iterative, phased)

---

## Current State Analysis

### What exists today

| Layer | Key Files | Cosmos-specific coupling |
|-------|-----------|--------------------------|
| **Core** | `Entities/*`, `Commands/*`, `Queries/*` | None — entities are clean POCOs ✅ |
| **Infrastructure** | `Data/FacilitationDbContext.cs` | `ToContainer()`, `HasNoDiscriminator()`, all navigation properties `Ignore()`d |
| | `Data/CosmosDbInitializer.cs` | Direct Cosmos SDK calls to create database & containers |
| | `Handlers/*` (31 handlers) | Use `IDbContextFactory<FacilitationDbContext>` + EF Core LINQ — mostly provider-agnostic ✅ |
| **Web** | `Program.cs` | Conditional `UseCosmos()` / `UseInMemoryDatabase()` registration, `CosmosDbInitializer` wiring |
| **Tests** | `MeetingWorkflowTests.cs` | `UseInMemoryDatabase()` with mocked `IDbContextFactory` |
| **Config** | `appsettings.json` | `Database:ConnectionStrings:CosmosDB:*` and `InMemory` keys |

### Key observations

1. **Entities (Core)** are already database-agnostic — no attributes, no Cosmos annotations. No changes needed.
2. **`FacilitationDbContext.OnModelCreating`** is the primary coupling point: every entity is mapped with `ToContainer(…)` and `HasNoDiscriminator()`, and all navigation properties are explicitly ignored (Cosmos DB separate-containers pattern).
3. **Handlers** interact only with `IDbContextFactory<FacilitationDbContext>` and EF Core LINQ. Two patterns exist:
   - Simple single-entity CRUD (most handlers) — fully provider-agnostic already.
   - Manual "load related entities" pattern in `GetMeetingByTokenHandler` (separate queries per child collection because Cosmos has no joins) — works on relational DBs too, but is suboptimal there.
4. **`CosmosDbInitializer`** uses the raw Cosmos SDK (`Microsoft.Azure.Cosmos`) to create containers. This class is entirely Cosmos-specific and must not run for SQL Server.
5. **`Program.cs`** already has rudimentary provider switching (`if cosmosEndpoint … else InMemory`) but is not extensible.
6. **NuGet packages**: `Microsoft.EntityFrameworkCore.Cosmos` and `Microsoft.Azure.Cosmos` are referenced unconditionally in Infrastructure.csproj.

---

## Goals & Non-Goals

### Goals

- **G1** — Run the identical handler code against Cosmos DB, SQL Server, or InMemory without source changes in handlers.
- **G2** — Select the database provider at startup via configuration (`appsettings.json` / environment variable).
- **G3** — Each provider supplies its own `OnModelCreating` configuration (containers vs. tables, relationships, indexes).
- **G4** — Each provider can supply its own database initializer/migrator (Cosmos container creation vs. EF migrations).
- **G5** — Keep the door open for PostgreSQL by keeping the pattern generic; do **not** implement a PostgreSQL provider yet.
- **G6** — Existing unit tests continue to pass with minimal change (they use InMemory today).
- **G7** — Minimise breaking changes to handler signatures and DI registrations.

### Non-Goals

- Implementing a full Repository/Unit-of-Work abstraction on top of EF Core (the `DbContext` + `IDbContextFactory` pattern is sufficient).
- Supporting multiple providers simultaneously in a single running instance.
- Implementing PostgreSQL provider (future phase).
- Changing entity models in Core.

---

## Architecture Design

### 1. Provider enumeration & configuration

```
appsettings.json
─────────────────
{
  "Database": {
    "Provider": "CosmosDb",     // "CosmosDb" | "SqlServer" | "InMemory"
    "ConnectionStrings": {
      "CosmosDb": {
        "AccountEndpoint": "…",
        "AccountKey": "…",
        "DatabaseName": "FacilitationAssistant"
      },
      "SqlServer": "Server=…;Database=FacilitationAssistant;…",
      "InMemory": "FacilitationDb"
    }
  }
}
```

A single `Database:Provider` key determines which provider is activated.

### 2. Project structure (target state)

```
src/
├── FacilitationAssistant.Core/              ← UNCHANGED
│   ├── Entities/
│   ├── Commands/
│   └── Queries/
│
├── FacilitationAssistant.Infrastructure/    ← MODIFIED
│   ├── Data/
│   │   ├── FacilitationDbContext.cs          ← Slim base: DbSets only, no OnModelCreating
│   │   ├── IDatabaseInitializer.cs           ← NEW — interface
│   │   ├── DatabaseProvider.cs               ← NEW — enum { CosmosDb, SqlServer, InMemory }
│   │   ├── Cosmos/
│   │   │   ├── CosmosModelConfiguration.cs   ← NEW — IEntityTypeConfiguration per entity (Cosmos-specific)
│   │   │   └── CosmosDbInitializer.cs        ← MOVED from Data/, implements IDatabaseInitializer
│   │   ├── SqlServer/
│   │   │   ├── SqlServerModelConfiguration.cs← NEW — IEntityTypeConfiguration per entity (relational)
│   │   │   ├── SqlServerDbInitializer.cs     ← NEW — runs EF migrations
│   │   │   └── Migrations/                   ← NEW — EF Core migration files
│   │   └── InMemory/
│   │       └── InMemoryDbInitializer.cs      ← NEW — EnsureCreated() wrapper
│   ├── Handlers/                             ← UNCHANGED (no handler changes needed)
│   ├── Hubs/                                 ← UNCHANGED
│   └── Extensions/
│       └── DatabaseServiceExtensions.cs      ← NEW — IServiceCollection.AddDatabase(config)
│
├── FacilitationAssistant.Web/               ← MODIFIED
│   ├── Program.cs                            ← Simplified: calls AddDatabase(config)
│   └── appsettings.json                      ← Add Provider key + SqlServer connection string
```

### 3. Core abstractions

#### 3.1 `DatabaseProvider` enum

```csharp
namespace FacilitationAssistant.Infrastructure.Data;

public enum DatabaseProvider
{
    InMemory,
    CosmosDb,
    SqlServer
    // PostgreSql — future
}
```

#### 3.2 `IDatabaseInitializer` interface

```csharp
namespace FacilitationAssistant.Infrastructure.Data;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
```

Each provider implements this:
- **Cosmos** → existing container-creation logic (wrapped from current `CosmosDbInitializer`).
- **SqlServer** → `context.Database.MigrateAsync()` (apply EF migrations).
- **InMemory** → `context.Database.EnsureCreatedAsync()`.

#### 3.3 `FacilitationDbContext` — provider-neutral base

Strip all Cosmos-specific configuration out of `OnModelCreating`. The `DbContext` becomes a thin shell:

```csharp
public class FacilitationDbContext : DbContext
{
    public FacilitationDbContext(DbContextOptions<FacilitationDbContext> options)
        : base(options) { }

    public DbSet<Meeting> Meetings { get; set; }
    public DbSet<AgendaStage> AgendaStages { get; set; }
    // … all other DbSets …

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FacilitationDbContext).Assembly);
    }
}
```

`ApplyConfigurationsFromAssembly` will pick up whichever `IEntityTypeConfiguration<T>` classes are registered (see per-provider sections below).

> **Alternative considered:** Use separate derived `DbContext` classes per provider (`CosmosDbContext : FacilitationDbContext`). Rejected because handlers would need to resolve different types, adding complexity. Instead, inject per-provider `IEntityTypeConfiguration` implementations at registration time.

**Refined approach (recommended):** Because `ApplyConfigurationsFromAssembly` would load *all* configurations regardless of provider, use an explicit approach where configurations are applied conditionally. Two clean options:

**Option A — Conditional in `OnModelCreating` with injected provider flag:**

```csharp
public class FacilitationDbContext : DbContext
{
    private readonly DatabaseProvider _provider;

    public FacilitationDbContext(
        DbContextOptions<FacilitationDbContext> options,
        DatabaseProvider provider = DatabaseProvider.InMemory)
        : base(options)
    {
        _provider = provider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        switch (_provider)
        {
            case DatabaseProvider.CosmosDb:
                CosmosModelConfiguration.Apply(modelBuilder);
                break;
            case DatabaseProvider.SqlServer:
                SqlServerModelConfiguration.Apply(modelBuilder);
                break;
            case DatabaseProvider.InMemory:
                // InMemory needs minimal config — or reuse SqlServer config
                SqlServerModelConfiguration.Apply(modelBuilder);
                break;
        }
    }
}
```

**Option B — Derived DbContext per provider (alternative if Option A becomes unwieldy):**

```csharp
// Shared base
public abstract class FacilitationDbContextBase : DbContext { /* DbSets */ }

// Per-provider
public class CosmosFacilitationDbContext : FacilitationDbContextBase { /* Cosmos OnModelCreating */ }
public class SqlFacilitationDbContext : FacilitationDbContextBase { /* SQL OnModelCreating */ }

// Register IDbContextFactory<FacilitationDbContextBase> → resolve concrete
```

**Recommendation:** Start with **Option A** — it is simpler, avoids changing handler DI signatures, and the provider flag naturally flows from configuration.

### 4. Per-provider model configuration

#### 4.1 Cosmos DB — `CosmosModelConfiguration.cs`

Moves the existing `OnModelCreating` logic (all `ToContainer`, `HasNoDiscriminator`, `Ignore` calls) into a static helper:

```csharp
public static class CosmosModelConfiguration
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToContainer("Meetings");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FacilitatorToken).IsRequired().HasMaxLength(50);
            // … (current Cosmos config, verbatim)
            entity.Ignore(m => m.Stages);
            entity.Ignore(m => m.Notes);
            // …
        });
        // … repeat for all 12 entity types …
    }
}
```

#### 4.2 SQL Server — `SqlServerModelConfiguration.cs`

Proper relational mapping with foreign keys, indexes, and navigation properties:

```csharp
public static class SqlServerModelConfiguration
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("Meetings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FacilitatorToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AttendeeToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasIndex(e => e.FacilitatorToken).IsUnique();
            entity.HasIndex(e => e.AttendeeToken).IsUnique();

            entity.HasMany(m => m.Stages)
                  .WithOne(s => s.Meeting)
                  .HasForeignKey(s => s.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Notes)
                  .WithOne(n => n.Meeting)
                  .HasForeignKey(n => n.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Concerns)
                  .WithOne(c => c.Meeting)
                  .HasForeignKey(c => c.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.AttendeeSessions)
                  .WithOne(a => a.Meeting)
                  .HasForeignKey(a => a.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Questions)
                  .WithOne(q => q.Meeting)
                  .HasForeignKey(q => q.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Messages)
                  .WithOne(msg => msg.Meeting)
                  .HasForeignKey(msg => msg.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AgendaStage>(entity =>
        {
            entity.ToTable("AgendaStages");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.MeetingId, e.OrderIndex });
            // … property constraints …
        });

        // … Concern, ConcernVote, Note, AttendeeSession, Question,
        //   QuestionOption, QuestionResponse, Message, MessageOption,
        //   MessageResponse — each with ToTable, FK, indexes …
    }
}
```

Key relational-specific concerns:
- **Unique indexes** on `FacilitatorToken` and `AttendeeToken`.
- **Composite indexes** where queries filter by `MeetingId` + another column.
- **Cascade deletes** on `MeetingId` foreign keys (meeting deletion removes all children).
- **FK constraints** for `ConcernVote → Concern`, `QuestionOption/Response → Question`, `MessageOption/Response → Message`.
- `QuestionResponse.AnswerChoiceIds` (currently `List<string>`) needs a value converter to JSON or a join table for relational DBs.

#### 4.3 InMemory

Reuse `SqlServerModelConfiguration.Apply()` — the InMemory provider understands relational-style config (tables, FKs are ignored but properties/keys work). Navigation properties and `Ignore()` are not needed since InMemory handles them. Any unsupported features (unique indexes, etc.) are silently ignored by InMemory provider.

### 5. DI registration — `DatabaseServiceExtensions.cs`

A single extension method replaces the current sprawling `Program.cs` logic:

```csharp
public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var providerName = configuration.GetValue<string>("Database:Provider") ?? "InMemory";
        var provider = Enum.Parse<DatabaseProvider>(providerName, ignoreCase: true);

        switch (provider)
        {
            case DatabaseProvider.CosmosDb:
                var endpoint = configuration.GetValue<string>("Database:ConnectionStrings:CosmosDb:AccountEndpoint")!;
                var key = configuration.GetValue<string>("Database:ConnectionStrings:CosmosDb:AccountKey")!;
                var dbName = configuration.GetValue<string>("Database:ConnectionStrings:CosmosDb:DatabaseName")
                    ?? "FacilitationAssistant";

                services.AddDbContextFactory<FacilitationDbContext>(options =>
                    options.UseCosmos(endpoint, key, dbName));
                services.AddSingleton<IDatabaseInitializer, CosmosDbInitializer>();
                break;

            case DatabaseProvider.SqlServer:
                var connectionString = configuration.GetValue<string>("Database:ConnectionStrings:SqlServer")!;

                services.AddDbContextFactory<FacilitationDbContext>(options =>
                    options.UseSqlServer(connectionString));
                services.AddSingleton<IDatabaseInitializer, SqlServerDbInitializer>();
                break;

            case DatabaseProvider.InMemory:
            default:
                var inMemoryName = configuration.GetValue<string>("Database:ConnectionStrings:InMemory")
                    ?? "FacilitationDb";

                services.AddDbContextFactory<FacilitationDbContext>(options =>
                    options.UseInMemoryDatabase(inMemoryName));
                services.AddSingleton<IDatabaseInitializer, InMemoryDbInitializer>();
                break;
        }

        // Register the provider enum as a singleton so DbContext can read it
        services.AddSingleton(provider);

        return services;
    }
}
```

### 6. Simplified `Program.cs`

```csharp
// Replace all current database configuration with:
builder.Services.AddDatabase(builder.Configuration);

// …

// Replace CosmosDbInitializer block with:
var initializer = app.Services.GetRequiredService<IDatabaseInitializer>();
await initializer.InitializeAsync();
```

### 7. Package references

#### Infrastructure.csproj — conditional references

```xml
<ItemGroup>
  <!-- Always needed -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />

  <!-- Cosmos DB -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.Cosmos" Version="9.0.0" />
  <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.44.1" />

  <!-- SQL Server -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />

  <!-- Design-time tools (migrations) -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

> **Note:** For a production deployment targeting only one provider, conditional `<PackageReference>` with build configurations could be used to trim unused provider packages. This is optional and can be deferred.

---

## Handler Impact Assessment

### Handlers requiring NO changes (25 of 31)

All handlers that perform simple single-entity operations against a single `DbSet` will work identically across providers. Pattern:

```csharp
await using var context = await _contextFactory.CreateDbContextAsync(ct);
context.SomeDbSet.Add(entity);
await context.SaveChangesAsync(ct);
```

These handlers include: `CreateMeetingHandler`, `AddAgendaStageHandler`, `DeleteAgendaStageHandler`, `ReorderAgendaStagesHandler`, `StartMeetingHandler`, `EndMeetingHandler`, `StartStageHandler`, `EndStageHandler`, `AddNoteHandler`, `UpdateNoteHandler`, `DeleteNoteHandler`, `RaiseConcernHandler`, `AcknowledgeConcernHandler`, `RespondToConcernHandler`, `WithdrawConcernHandler`, `VoteConcernHandler`, `CreateQuestionHandler`, `TriggerQuestionHandler`, `CloseQuestionHandler`, `SubmitQuestionResponseHandler`, `CreateMessageHandler`, `CloseMessageHandler`, `RespondToMessageHandler`, `RegenerateFacilitatorTokenHandler`, `GetMeetingByIdHandler`.

### Handlers requiring REVIEW (6 of 31)

These handlers manually load related data across separate queries (the Cosmos "no joins" pattern). On SQL Server, this still works but could optionally be optimised with `.Include()`:

| Handler | Current Pattern | SQL Server Opportunity |
|---------|----------------|----------------------|
| `GetMeetingByTokenHandler` | Loads Meeting, then separate queries for Stages, Notes | Could use `.Include(m => m.Stages).Include(m => m.Notes)` |
| `GetConcernsByMeetingQueryHandler` | Loads Concerns, then separate query for Votes | Could use `.Include(c => c.Votes)` |
| `GetQuestionsByMeetingHandler` | Loads Questions, then Options & Responses | Could use `.Include(q => q.Options).Include(q => q.Responses)` |
| `GetQuestionByIdHandler` | Loads Question + Options + Responses | Could use `.Include()` chain |
| `GetQuestionResultsHandler` | Loads Question + Options + Responses | Same as above |
| `GetMessageHandlers` | Loads Messages + Options + Responses | Could use `.Include()` chain |

**Decision — Two approaches:**

1. **Keep current pattern (recommended for Phase 1):** The manual-load pattern works on all providers. It's slightly less efficient on SQL Server (multiple round-trips) but correct. Optimising with `.Include()` is optional and can be done provider-conditionally later.

2. **Provider-conditional Include (Phase 2 optimisation):** Inject the `DatabaseProvider` enum into handlers that could benefit from `.Include()` and branch:

   ```csharp
   if (_provider == DatabaseProvider.SqlServer)
       query = query.Include(m => m.Stages);
   ```

   Or better yet — `.Include()` works on Cosmos for same-container entities but is a no-op for cross-container. Since Cosmos ignores navigations, calling `.Include()` on Cosmos simply does nothing (no error), so it may be safe to always use it. **This needs testing.**

**Recommendation:** Keep existing handler code unchanged in Phase 1. File a follow-up task to benchmark and optimise query patterns for SQL Server in Phase 2.

### Special consideration: `QuestionResponse.AnswerChoiceIds`

`QuestionResponse.AnswerChoiceIds` is `List<string>`. In Cosmos DB, this serialises naturally to a JSON array. In SQL Server, EF Core 9+ supports `List<string>` as a JSON column. Options:

1. **EF Core 9 JSON column** (recommended) — use `entity.Property(e => e.AnswerChoiceIds).HasConversion(…)` or rely on EF9's native primitive collection support.
2. **Value converter** — `List<string> ↔ string` (JSON serialization).
3. **Join table** — `QuestionResponseChoices` (most normalized, more work).

**Recommendation:** Use EF Core 9's built-in primitive collection support. For SQL Server, add in `SqlServerModelConfiguration`:
```csharp
entity.Property(e => e.AnswerChoiceIds)
      .HasColumnType("nvarchar(max)");
```

---

## Migration Strategy for SQL Server

### Initial migration

```bash
# From Infrastructure project directory, with SqlServer provider configured
dotnet ef migrations add InitialCreate \
  --startup-project ../FacilitationAssistant.Web \
  --context FacilitationDbContext \
  --output-dir Data/SqlServer/Migrations
```

### Design-time factory

A `IDesignTimeDbContextFactory<FacilitationDbContext>` may be needed to generate migrations from the CLI without running the app:

```csharp
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FacilitationDbContext>
{
    public FacilitationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FacilitationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=FacilitationAssistant_Design;Trusted_Connection=True;")
            .Options;

        return new FacilitationDbContext(options, DatabaseProvider.SqlServer);
    }
}
```

---

## Configuration Examples

### Local development (InMemory — default)
```json
{
  "Database": {
    "Provider": "InMemory"
  }
}
```

### Azure production (Cosmos DB)
```json
{
  "Database": {
    "Provider": "CosmosDb",
    "ConnectionStrings": {
      "CosmosDb": {
        "AccountEndpoint": "https://xxx.documents.azure.com:443/",
        "AccountKey": "xxx==",
        "DatabaseName": "FacilitationAssistant"
      }
    }
  }
}
```

### SQL Server (on-premises or Azure SQL)
```json
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionStrings": {
      "SqlServer": "Server=localhost;Database=FacilitationAssistant;Trusted_Connection=True;TrustServerCertificate=True;"
    }
  }
}
```

---

## Testing Strategy

### Unit tests (existing)
- Continue using InMemory provider — no changes to `MeetingWorkflowTests.cs`.
- The `CreateInMemoryOptions()` helper works as-is.
- Add a small test verifying `DatabaseServiceExtensions.AddDatabase(…)` resolves each provider correctly.

### Integration tests (new — optional)
- Add a `docker-compose.yml` with SQL Server container for local integration testing.
- Add a test fixture that runs against real SQL Server to catch relational-specific issues (FK violations, unique constraints).
- Cosmos DB integration tests (existing E2E tests) remain as-is.

### Configuration tests (new)
- Verify that parsing `Database:Provider` correctly selects the right model configuration.
- Verify that an unknown provider throws a clear error at startup.

---

## Implementation Phases & Task Breakdown

### Phase 1 — Core abstractions & Cosmos extraction (2–3 days)

| # | Task | Files affected |
|---|------|---------------|
| 1.1 | Create `DatabaseProvider` enum | `Data/DatabaseProvider.cs` (new) |
| 1.2 | Create `IDatabaseInitializer` interface | `Data/IDatabaseInitializer.cs` (new) |
| 1.3 | Extract Cosmos config into `CosmosModelConfiguration.cs` | `Data/Cosmos/CosmosModelConfiguration.cs` (new) |
| 1.4 | Refactor `CosmosDbInitializer` to implement `IDatabaseInitializer` | `Data/Cosmos/CosmosDbInitializer.cs` (modify + move) |
| 1.5 | Create `InMemoryDbInitializer` | `Data/InMemory/InMemoryDbInitializer.cs` (new) |
| 1.6 | Refactor `FacilitationDbContext` — remove Cosmos-specific `OnModelCreating`, add provider-conditional dispatch | `Data/FacilitationDbContext.cs` (modify) |
| 1.7 | Create `DatabaseServiceExtensions.AddDatabase(…)` | `Extensions/DatabaseServiceExtensions.cs` (new) |
| 1.8 | Simplify `Program.cs` to use `AddDatabase(…)` | `Program.cs` (modify) |
| 1.9 | Update `appsettings.json` — add `Database:Provider` key | `appsettings.json` (modify) |
| 1.10 | Verify all existing unit tests pass | `MeetingWorkflowTests.cs` (run) |

**Exit criteria:** Application runs identically with Cosmos DB and InMemory as it does today. No handler changes.

### Phase 2 — SQL Server provider (2–3 days)

| # | Task | Files affected |
|---|------|---------------|
| 2.1 | Add `Microsoft.EntityFrameworkCore.SqlServer` package | `Infrastructure.csproj` (modify) |
| 2.2 | Create `SqlServerModelConfiguration.cs` with relational mappings | `Data/SqlServer/SqlServerModelConfiguration.cs` (new) |
| 2.3 | Handle `QuestionResponse.AnswerChoiceIds` for SQL Server | Inside `SqlServerModelConfiguration.cs` |
| 2.4 | Create `SqlServerDbInitializer` (runs EF migrations) | `Data/SqlServer/SqlServerDbInitializer.cs` (new) |
| 2.5 | Create `DesignTimeDbContextFactory` for migration CLI | `Data/SqlServer/DesignTimeDbContextFactory.cs` (new) |
| 2.6 | Generate initial EF migration | `Data/SqlServer/Migrations/*` (new, generated) |
| 2.7 | Wire SQL Server case in `DatabaseServiceExtensions` | `Extensions/DatabaseServiceExtensions.cs` (modify) |
| 2.8 | Add SQL Server connection string to `appsettings.json` | `appsettings.json` (modify) |
| 2.9 | Test full workflow against LocalDB or Docker SQL Server | Manual / integration test |
| 2.10 | Update `appsettings.Development.json` with local SQL Server config | `appsettings.Development.json` (modify) |

**Exit criteria:** Application starts and full meeting workflow passes against SQL Server (LocalDB or Docker).

### Phase 3 — Optimisation & hardening (1–2 days)

| # | Task | Files affected |
|---|------|---------------|
| 3.1 | Add startup validation — fail fast on bad `Database:Provider` or missing connection string | `DatabaseServiceExtensions.cs` |
| 3.2 | Add logging for selected provider at startup | `DatabaseServiceExtensions.cs` / `Program.cs` |
| 3.3 | Add integration test with Docker SQL Server | `tests/` (new test project or fixture) |
| 3.4 | Evaluate `.Include()` optimisation for SQL Server handlers (benchmark) | Handlers (optional modify) |
| 3.5 | Update documentation (`README.md`, `IMPLEMENTATION.md`) | Docs |
| 3.6 | Consider conditional NuGet references per build config (optional) | `Infrastructure.csproj` |

### Phase 4 — Future: PostgreSQL (not yet)

When needed, the work is:
1. Add `Npgsql.EntityFrameworkCore.PostgreSQL` package.
2. Create `PostgreSqlModelConfiguration.cs` (very similar to SQL Server).
3. Create `PostgreSqlDbInitializer.cs`.
4. Add `PostgreSql` case in `DatabaseServiceExtensions`.
5. Generate migration (or share SQL Server migration if schemas match).

Estimated: 1 day, given the framework from Phases 1–2.

---

## Risk Register

| Risk | Impact | Mitigation |
|------|--------|------------|
| LINQ queries that work on Cosmos/InMemory fail on SQL Server (e.g., client-eval, unsupported operators) | Medium | Phase 2 integration tests; EF Core logging with `EnableSensitiveDataLogging` to catch warnings |
| `List<string>` property (`AnswerChoiceIds`) breaks on SQL Server | Medium | Explicit value converter or EF9 primitive collection support (tested in Phase 2) |
| EF Core model cache conflict if provider detected incorrectly | Low | Provider is set once at startup and never changes; singleton registration |
| Cosmos `HasNoDiscriminator()` + `Ignore()` navigations pattern not applied → runtime Cosmos errors | High | Extract verbatim from current working code; test against Cosmos emulator |
| Design-time migration generation fails (no startup project context) | Low | `IDesignTimeDbContextFactory` implementation |
| Performance regression on SQL Server due to separate queries (no `.Include()`) | Low | Acceptable for Phase 1; optimise in Phase 3; the query patterns are simple |

---

## Decision Log

| Decision | Rationale |
|----------|-----------|
| Use `DatabaseProvider` enum injected into `DbContext` (Option A) over derived DbContexts (Option B) | Avoids changing all handler DI signatures; simpler; one `DbContextFactory` type |
| Keep handlers unchanged in Phase 1 | The manual-load pattern works across all providers; optimisation is Phase 3 |
| InMemory reuses SqlServer model config | InMemory understands relational config; avoids maintaining a third config |
| Do NOT implement Repository pattern | `IDbContextFactory<FacilitationDbContext>` is the existing abstraction and is sufficient |
| Do NOT implement PostgreSQL yet | Per requirements — keep the door open via the enum + extension method pattern |
| All provider packages referenced unconditionally | Simplicity; conditional references via build config is optional future optimisation |
