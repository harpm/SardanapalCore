# AGENTS.md

Guidance for AI agents working in the **Sardanapal Core** repository.

## What this is

Sardanapal Core is a reusable **.NET 8.0 business-application framework**, shipped as a set of
independent NuGet packages. It is a library, not a runnable app — there is no startup project,
no `Program.cs` entry point, and no test projects. Every project under `Src/` builds into its own
`.nupkg` (see `Directory.Build.props` → `GeneratePackageOnBuild=True`).

## Repository layout

```
Sardanapal Core.sln          Solution file (at repo root) containing every project
SDBuild.go                   Go build/release orchestrator (reads Build.json)
Build.json                   Build config: version, nuget feed, ordered list of projects to publish
Directory.Build.props        Shared MSBuild props (net8.0, CPM on, Nullable, ImplicitUsings, ...)
Directory.Packages.props     Central Package Management — ALL package versions live here
Tests/
  Sardanapal.*.Tests         Test projects (xUnit), one per source project, with Unit/ & Integration/ subfolders
Src/
  .editorconfig              C# style/naming rules (enforced: _camelCase private fields)
  Sardanapal.Share           Shared utilities, extensions, types (no project refs)
  Sardanapal.Localization   ResX translation resources (Messages.resx); no project refs
  Sardanapal.ViewModel      -> Localization, Share            (IResponse, GridVM, VMs)
  Sardanapal.Contract       -> ViewModel                     (IModel, IRepository, IService)
  Sardanapal.Domain         -> Contract, Share               (entity models, attributes)
  Sardanapal.Service        -> Contract, Domain              (CrudServiceBase, PanelServiceBase)
  Sardanapal.Ef             -> Contract, Domain, ViewModel   (EF repos, UnitOfWork, EF services)
  Sardanapal.Http.Service   -> Contract                      (ASP.NET Core middlewares/services)
  Sardanapal.RabbitMQ       -> Contract, Share               (RabbitMQ sender services)
  Sardanapal.RedisCache     -> Contract                      (Redis cache service)
  Sardanapal.Validation     -> Contract                      (FluentValidation integration)
```

`->` denotes project references. `Sardanapal.Contract` is the dependency hub — almost everything
references it. Do not add a reference from `Contract`/`Share`/`Localization`/`ViewModel` to a
higher-level package; keep the dependency direction leafward.

## Commands

```bash
# Restore / build the whole solution
dotnet restore "Sardanapal Core.sln"
dotnet build "Sardanapal Core.sln" -c release

# Build a single project
dotnet build Src/Sardanapal.Service -c release

# Apply .editorconfig formatting (run before submitting changes)
dotnet format "Sardanapal Core.sln"
```

The release pipeline (`SDBuild.go`) iterates `Build.json`, running for each project:
`dotnet clean` -> `dotnet restore` -> `dotnet build -c release -p:Version=<version>` ->
`dotnet nuget push` to the GitHub Packages feed. It is driven by Go (`go run SDBuild.go`) and runs
only in CI (`.github/workflows/dotnet.yml`, on push to `master`). **Do not run `SDBuild.go` locally** —
the publish step hard-codes CI runner paths and will fail.

There is no test suite. Verify changes by building the solution and (where applicable) exercising
the public API in a downstream consumer.

## Workflow rules

These rules govern how an agent operates in this repo. Follow them strictly.

1. **Never commit without explicit permission.** Stage changes and stop; do not run `git commit`
   (or push, amend, open PRs, etc.) unless the user explicitly asks for it.
2. **Commit messages must match the existing pattern.** Look at `git log --oneline` first; this repo
   uses the format `<type>: <description>`, where `<type>` is one of the prefixes already in use
   (`resolve`, `refactor`, `release`, `implement`, `docs`, `rm`, …). Mimic that style exactly.
3. **Log any issue you notice while working.** If you spot a bug, inconsistency, or improvement
   opportunity during a task (even one unrelated to the current prompt), append a row to
   **`Issue.csv`** rather than silently fixing it. The file has a `State` column with one of these
   values: `pending`, `fixed`, `rejected`. Create the file with headers if it does not yet exist.

## Conventions you must follow

### C# style (enforced by `Src/.editorconfig`)
- **Private fields must be `_camelCase`** — this rule has severity `error` and will fail the build.
- Discourage `var`; use explicit types (rules are `false:suggestion`).
- Do not qualify with `this.`.
- **Allman braces** — opening brace on its own new line (`csharp_new_line_before_open_brace = all`).
- **Block-scoped namespaces** (not file-scoped).
- Modifier order: `public, private, protected, internal, static, extern, new, virtual, abstract,
  sealed, override, readonly, unsafe, volatile, async`.
- Accessibility modifiers are always required.
- Properties/indexers/accessors may be expression-bodied when single-line; **methods, constructors,
  and operators must NOT**.
- 4-space indent, CRLF line endings, UTF-8, trim trailing whitespace, final newline.
- Latest C# language version. Prefer pattern matching, throw expressions, simple `using`, braces.

### Project / package management
- **Central Package Management is ON.** Never put a `Version` on a `<PackageReference>`. Add new
  packages to `Directory.Packages.props` as `<PackageVersion Include="..." Version="..." />` and
  reference them versionless in the csproj.
- Target framework is `net8.0` for all projects (set centrally in `Directory.Build.props`).
- Nullable reference types and implicit usings are enabled globally.

## Adding a new package/project

1. Create the project folder under `Src/` (e.g. `Src/Sardanapal.Foo/`) with a `*.csproj` matching
   the `Microsoft.NET.Sdk` style of the others.
2. Add it to `Sardanapal Core.sln`.
3. Add the project path to `Build.json` → `projects_path` (order matters: dependencies should be
   listed before dependents, since publish iterates this list).
4. Declare any external dependencies as `<PackageVersion>` in `Directory.Packages.props`.
5. Reference only leafward projects (see layout above).

## Core architectural patterns

### Request-Response protocol (`Sardanapal.ViewModel/Response/`)
The framework's signature feature. Every service method returns `IResponse<T>` (or `IResponse<bool>`),
carrying `StatusCode`, `OperationType`, `DeveloperMessages[]`, `UserMessage`, and `Data`. Build results
through the `Response<T>` factory and wrap service bodies in `Fill`/`FillAsync`, which automatically
map `OperationCanceledException` -> `StatusCode.Canceled` and any other exception ->
`StatusCode.Exception` (logging via `ILogger`). See `CrudService.cs` for the canonical usage.

Status codes are a `byte` enum (`StatusCode`) and operation types a `byte` enum (`OperationType`) —
both in `ResponseTypes.cs`. Keep them stable; they are part of the wire protocol.

### CRUD service & repository generics
`CrudServiceBase<TRepository, TKey, TEntity, TSearchVM, TVM, TNewVM, TEditableVM>` and
`EFRepositoryBase<TContext, TKey, TModel>` are the central, heavily-generic base classes with
`where` constraints on `TKey` (`IComparable`, `IEquatable`) and on the entity/VM types. `TKey` is
parameterized throughout (do not hardcode `int`/`Guid`). Mapping between entities and VMs uses
AutoMapper (`_mapper.Map<...>`).

Repositories auto-detect soft-delete: entities implementing `ILogicalEntityModel` are flagged
`IsDeleted` on `DeleteRange*` instead of being removed. Preserve this branch when editing delete logic.

### Unit of Work
Database access flows through `ISdUnitOfWork` / `EFDatabaseManager` / `UnitOfWork` in
`Sardanapal.Ef/UnitOfWork/`. Repositories hold the `DbContext` as `_unitOfWork`.

### Exceptions
Use the guard helpers `EnsureNotNullReference` / `EnsureNotNullCollection` (see `EFRepository.cs`)
rather than reinventing null checks. Exceptions are captured by `FillAsync` and surfaced as
`DeveloperMessages` via `GetHierarchicalMessages()`.

## Notes & gotchas
- `Build.json` keys are lowercase (`version`, `projects_path`, `nuget_provider`) but the Go struct in
  `SDBuild.go` is PascalCase — Go's `encoding/json` matches case-insensitively, so this is intentional.
- NuGet packages are published to `https://nuget.pkg.github.com/harpm/index.json` using the
  `GITHUBTOKEN` secret; never commit tokens or local NuGet configs.
- The `bin/` and `obj/` folders are present and gitignored — do not edit anything in them.
- Branch that triggers CI is `master` (not `main`).
