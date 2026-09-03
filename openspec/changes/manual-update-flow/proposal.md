## Why

StockDesk currently performs silent background update checks on startup, but users have no visibility or control over this process. Users cannot manually check if a new version is available, view release notes, see download progress, or know whether an update is ready to install without restarting. Adding an interactive manual update check from the top bar with visual feedback, status indicators, and an Azerbaijani-localized modal dialog provides transparency, user trust, and complete control over updates.

## What Changes

- Add a manual update button with a badge dot and progress spinner in the top bar of `MainWindow`.
- Implement a busy/disabled state with a spinning indicator during update checks to prevent duplicate clicks.
- Display an indicator dot on the top-bar button when a newer version is available or prepared.
- Implement a dedicated modern modal dialog (`UpdateDialog`) localized in Azerbaijani supporting all outcomes:
  - **Update available**: Displays new version number, release notes, security authenticity badge, and an "İndi yenilə" action button.
  - **Downloading progress**: Displays a real-time progress bar (0%–100%) and status messages during download.
  - **Ready to restart**: When an update was already downloaded in the background, prompts the user to restart ("Quraşdır və yenidən başlat").
  - **Up to date**: Confirms that the current version is the latest ("Tətbiq aktualdır").
  - **Network error / connection failure**: Displays a descriptive error message with a "Yenidən cəhd et" option.
  - **Development environment**: Informs that updates are disabled in unpackaged/dev mode.
- Enhance `IUpdateService` to support progress reporting (`IProgress<int>`), expose download states, and coordinate startup background checks with the UI badge.

## Capabilities

### Modified Capabilities
- `auto-update`: Add manual update check initiation, UI button states (busy spinner, update badge), Azerbaijani modal dialog for update outcomes, real-time download progress tracking, and on-demand restart triggers.

## Impact

- **UI / Views**: `MainWindow.xaml` (top bar action buttons), new `Views/Dialogs/UpdateDialog.xaml` and `UpdateDialog.xaml.cs`.
- **ViewModels**: `MainViewModel.cs` (update check command, button states, badge indicator), new `ViewModels/UpdateDialogViewModel.cs`.
- **Services**: `IUpdateService` and `UpdateService.cs` (progress callback, pending update state, rich check result), `IDialogService` and `DialogService.cs` (methods to show update dialog).
- **Application Lifecycle**: `App.xaml.cs` (coordination between background startup check and `IUpdateService` state).
- **Dependencies**: Uses existing `Velopack` 1.2.0 and `WPF-UI` 4.3.0 controls.
