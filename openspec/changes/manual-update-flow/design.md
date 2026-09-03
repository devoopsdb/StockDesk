## Context

StockDesk uses Velopack 1.2.0 and WPF-UI 4.3.0 on .NET 10. `IUpdateService` is registered as a singleton in DI and currently runs an unobserved background check in `App.xaml.cs` on application startup. `MainWindow.xaml` does not expose any update controls, and `IDialogService` only provides basic `MessageBox` wrappers and product/category dialogs.

See `proposal.md` for motivation and `specs/auto-update/spec.md` for requirements.

## Goals / Non-Goals

**Goals:**
- Provide a responsive top bar button in `MainWindow.xaml` with busy spinner, idle icon, and badge indicator.
- Create a dedicated Fluent 2 modal dialog (`Views/Dialogs/UpdateDialog.xaml`) and corresponding `UpdateDialogViewModel` localized in Azerbaijani.
- Support real-time download progress reporting (`0%–100%`) through `Velopack.UpdateManager.DownloadUpdatesAsync`.
- Enable one-click immediate restart when an update is already downloaded.
- Provide clean separation between startup background checks and manual checks while sharing state via `IUpdateService`.

**Non-Goals:**
- In-app multi-language switcher (all update dialog strings are directly in Azerbaijani matching the existing application UI).
- Delta update package authoring (handled automatically by Velopack CLI in CI/CD).
- Rollback mechanisms for updates (Velopack handles downgrade safety).

## Decisions

### Decision 1: Dedicated Fluent 2 modal Window (`UpdateDialog.xaml`) instead of `MessageBox` or `ui:ContentDialog`
- **Choice:** Create `Views/Dialogs/UpdateDialog.xaml` inheriting from `Window` with `WindowStartupLocation="CenterOwner"`, `ShowInTaskbar="False"`, and WPF-UI styling, matching `CategoryDialog` and `ProductDialog`.
- **Rationale:** A dedicated window provides full layout flexibility for release notes scrolling, download progress bars, and custom action buttons, while maintaining theme consistency across Dark/Light modes.
- **Alternatives considered:**
  - *Standard MessageBox:* Cannot show progress bars, release notes formatting, or custom button layouts.
  - *`ui:ContentDialog`:* Requires embedding a `ContentPresenter` in `MainWindow` visual tree and coordinates poorly with multiple modal windows.

### Decision 2: State-driven dialog presentation via `UpdateDialogViewModel`
- **Choice:** Use an enum `UpdateDialogState` (`Checking`, `UpdateAvailable`, `Downloading`, `ReadyToRestart`, `UpToDate`, `NetworkError`, `DevMode`) in `UpdateDialogViewModel`. The view binds UI section visibility to this state.
- **Rationale:** Keeps view-model logic testable headlessly with xUnit and simplifies view switching within a single dialog instance without spawning multiple dialogs.
- **Alternatives considered:**
  - *Multiple separate dialog windows:* Fragmented code, jarring visual window transitions.

### Decision 3: Progress-reporting and stateful `IUpdateService`
- **Choice:** Extend `IUpdateService` to expose:
  - `Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)` with rich metadata (`UpdateStatus`, `TargetVersion`, `ReleaseNotes`, `IsDownloaded`, `ErrorMessage`).
  - `Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null, CancellationToken cancellationToken = default)`.
  - Observable / thread-safe state properties or event: `bool HasPendingUpdate`, `bool IsUpdateDownloaded`, `string? PendingVersion`, `string? ReleaseNotes`.
- **Rationale:** Velopack's `UpdateManager.DownloadUpdatesAsync` natively accepts `Action<int> progress`. Wrapping this with `IProgress<int>` marshals progress updates cleanly back to the WPF UI thread.
- **Alternatives considered:**
  - *Polled download status:* Inefficient and jerky progress animations.

### Decision 4: Handling unpackaged/development environment gracefully
- **Choice:** Check `_updateManager?.IsInstalled` in `UpdateService`. When `false`, return `UpdateStatus.DevMode` so the UI presents the Azerbaijani explanation rather than throwing an exception or falsely claiming the app is up to date.
- **Rationale:** Prevents developer confusion during local debugging.

## Risks / Trade-offs

- **[Risk]** User closes `UpdateDialog` while download is in progress.
  - **Mitigation:** Wire `CancellationTokenSource` to window closing or allow the download to complete in the background, keeping the top-bar badge dot lit.
- **[Risk]** Background startup check finishes after the user opens the dialog.
  - **Mitigation:** The singleton `IUpdateService` synchronizes download locks and pending states so concurrent calls don't trigger duplicate Velopack download processes.
- **[Risk]** GitHub API rate limits on unauthenticated release checks.
  - **Mitigation:** Velopack caches release metadata and queries releases using lightweight asset manifests. Network errors transition the dialog to the `NetworkError` state with a retry button.
