## 1. Service Layer & Update State

- [x] 1.1 Extend `IUpdateService` and `UpdateCheckResult` with rich update status (`UpdateStatus`, `ReleaseNotes`, `IProgress<int>` callback, and pending state properties) and verify unit tests compile.
- [x] 1.2 Update `UpdateService` implementation to support progress reporting, dev/unpackaged mode detection, and thread-safe state synchronization, and verify with updated unit tests in `UpdateServiceTests.cs`.
- [x] 1.3 Extend `IDialogService` and `DialogService` with `ShowUpdateDialogAsync` and verify DI registration in `App.xaml.cs`.

## 2. Dialog View & ViewModel

- [x] 2.1 Create `UpdateDialogViewModel` supporting update states (`Checking`, `UpdateAvailable`, `Downloading`, `ReadyToRestart`, `UpToDate`, `NetworkError`, `DevMode`), Azerbaijani text bindings, progress tracking, and actions (`StartDownloadCommand`, `ApplyAndRestartCommand`, `RetryCommand`), and verify with unit tests.
- [x] 2.2 Create `Views/Dialogs/UpdateDialog.xaml` and `UpdateDialog.xaml.cs` styled with WPF-UI Fluent 2 controls, scrollable release notes, and progress bar, and verify XAML compiles cleanly.

## 3. Main Window & Top Bar Integration

- [x] 3.1 Add `CheckForUpdatesCommand`, `IsCheckingForUpdates`, and `HasPendingUpdate` properties to `MainViewModel`, and verify state transitions via unit tests.
- [x] 3.2 Update `MainWindow.xaml` top bar action buttons to include the update button with sync icon, progress ring, and badge dot, and verify bindings and visual layout.
- [x] 3.3 Connect `App.xaml.cs` startup background update task to notify `IUpdateService` and update the badge dot on launch, and verify startup flow.

## 4. Verification & Testing

- [x] 4.1 Execute `dotnet test` across the solution to ensure all unit tests pass.
- [x] 4.2 Execute `dotnet build` to ensure zero compilation errors and warnings.
