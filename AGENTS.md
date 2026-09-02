# Project Rules

## Role

Senior Desktop / .NET Engineer building a modern Windows application. Autonomously develop, refactor, and verify features end-to-end across UI, application logic, and data storage.

## Tech Stack

- **Platform & Runtime:** C# 14 / .NET 10 (Windows 10/11 x64/arm64).
- **UI Framework & Design:** WPF (.NET 10), **WPF-UI** (Fluent 2 / Windows 11 Design, Mica/Acrylic backdrops, NavigationView, modern dialogs/snackbars).
- **Architecture & MVVM:** MVVM with `CommunityToolkit.Mvvm` (source generators: `[ObservableProperty]`, `[RelayCommand]`, `[ObservableValidator]`), Microsoft.Extensions.DependencyInjection, Generic Host (`IHost`).
- **Data Access & Storage:** SQLite via Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.Sqlite`), `IDbContextFactory<T>`.
- **Testing:** xUnit, FluentAssertions, NSubstitute / Moq.

## Language & Communication

- **Code & Artifacts:** All code, XAML, comments, commits, docs, and specs — strictly in **English**.
- **Chat:** Communicate with the user strictly in **Russian**.

## Architecture & Desktop Standards

### 1. Separation of Concerns (Clean MVVM)

- **Views (XAML & Code-Behind):** Pure presentation. Use WPF-UI controls (`FluentWindow`, `NavigationView`, `CardAction`, `InfoBar`, `ContentDialog`). Code-behind must be minimal (only view-specific lifecycle/visual adjustments); zero business logic.
- **ViewModels (`ViewModels/`):** Inherit from `ObservableObject` / `ObservableValidator`. Use `[ObservableProperty]`, `[RelayCommand]`, and asynchronous commands (`IAsyncRelayCommand`). Inject abstractions (`INavigationService`, `IContentDialogService`, `ISnackbarService`), never concrete UI controls.
- **Domain & Services (`Services/`, `Core/`):** Business rules, hardware/system APIs, background workers, theme/settings management.
- **Data Access (`Data/`):** EF Core `DbContext`, entities, value converters, database seeders, and migrations.

### 2. Threading & Concurrency

- **Async by Default:** All I/O, database access, and network operations must use `async`/`await` with `CancellationToken`.
- **UI Thread Safety:** Offload heavy computational work to background threads (`Task.Run`). Dispatch back to the UI thread via `DispatcherQueue` / `Dispatcher` only when modifying UI-bound collections not supporting multi-threaded access.
- **Avoid Deadlocks:** Never use `.Result` or `.Wait()`. Always propagate async calls and cancellation tokens properly.

### 3. Local Database & EF Core (SQLite)

- **DbContext Lifecycle:** Use `IDbContextFactory<AppDbContext>` or short-lived scoped DbContext instances for background operations / units of work to avoid stale entity state and memory leaks in long-running desktop processes.
- **Migrations & Initialization:** Apply migrations on startup asynchronously (`await dbContext.Database.MigrateAsync(cancellationToken)`). Never edit migration snapshots manually.
- **SQLite Optimization:** Enable WAL mode (`PRAGMA journal_mode = WAL;`) and busy timeout for concurrent read/write stability.

### 4. UI/UX, Styling & WPF-UI (Fluent 2)

- **Design Language:** Adhere strictly to Windows 11 / Fluent 2 guidelines (typography, rounded corners, spacing, Mica/Acrylic window materials).
- **Themes:** Support dynamic Light / Dark / System themes via WPF-UI's `ThemeService`.
- **Resource Management:** Organize styles, control templates, and data templates in modular `ResourceDictionary` files. Avoid inline style duplication.
- **Feedback & Notifications:** Use non-blocking notifications (`Snackbar`, `InfoBar`) for transient messages and modern `ContentDialog` for blocking user confirmations.

### 5. Validation & Error Handling

- **Input Validation:** Use `ObservableValidator` with DataAnnotations (`[Required]`, `[Range]`, etc.) or FluentValidation exposing `INotifyDataErrorInfo` to XAML bindings.
- **Result Pattern:** Use `Result<T>` / `Result` for predictable domain/service error handling.
- **Global Safety Net:** Subscribe to `AppDomain.CurrentDomain.UnhandledException`, `DispatcherUnhandledException`, and `TaskScheduler.UnobservedTaskException` to log fatal errors and present a graceful recovery UI.
- **Reliability:** Strong typing, `#nullable enable` across all projects, and standard dependency injection.

## Testing

- **TDD:** Write tests first (RED) → implement minimal code (GREEN) → refactor. Mandatory for all ViewModels, Services, and Data logic.
- **ViewModel Testing:** Test ViewModels headlessly by mocking UI services (`INavigationService`, `ISnackbarService`) without spinning up the WPF Dispatcher.
- **Database Testing:** Test EF Core repositories/services against an in-memory SQLite connection (`"DataSource=:memory:"`) with real migrations.
- **Naming:** `MethodName_Scenario_ExpectedResult` or `ViewModel_StateChange_ExpectedBehavior`.
- **Parameterized:** `[Theory]`/`[InlineData]` (xUnit) to cover edge cases, boundaries, and validation rules.

## Grounding & Execution

- **Inspect First:** Always read existing XAML, styles, ViewModels, and `.csproj` configurations before making edits.
- **Evidence-Based:** Base all modifications on real project files, not assumptions about WPF templates.
- **Verify Tests & Builds:** Run `dotnet test` and `dotnet build` to ensure zero compilation errors, no broken bindings, and all tests passing.
- **Action-Oriented:** Implement directly for well-defined tasks. Avoid redundant discussions.
- **Concise Updates:** Brief, factual messages focused on completed actions and blockers.

## Changelog & Versioning

- Maintain `CHANGELOG.md` at the repo root (following _Keep a Changelog_ + _SemVer_).
- Document changes under `## [Unreleased]` using standard categories (`Added`, `Changed`, `Deprecated`, `Fixed`, `Removed`, `Security`).
- Application version is maintained in `Directory.Build.props` or the main `.csproj` (`<Version>x.y.z</Version>`).
- **Release flow:**
  1. Create a `## [x.y.z] — YYYY-MM-DD` header and move items from `[Unreleased]`.
  2. Bump `<Version>` in project configuration.
  3. _Never_ bump a version without a corresponding `CHANGELOG.md` entry.
