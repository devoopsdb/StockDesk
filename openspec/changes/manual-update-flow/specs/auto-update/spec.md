## ADDED Requirements

### Requirement: User can manually trigger update check from top bar
The application SHALL provide an update button in the top bar allowing users to manually trigger an update check at any time.

#### Scenario: Manual update check initiated
- **WHEN** user clicks the update button in the top bar
- **THEN** the button is disabled to prevent duplicate requests and displays an active progress spinner while querying the update server

#### Scenario: Button state restored after check completes
- **WHEN** the manual update check finishes (either successfully or with an error)
- **THEN** the button is re-enabled and its icon returns to the default sync icon

### Requirement: Top bar button displays pending update badge
The application SHALL visually notify the user on the top bar button whenever a newer release is detected or ready.

#### Scenario: Update available badge shown
- **WHEN** an update check (either manual or automatic startup background check) detects a newer version
- **THEN** an indicator dot badge is displayed on the top bar update button

#### Scenario: Badge remains visible until updated
- **WHEN** an update is discovered and remains uninstalled
- **THEN** the indicator dot badge remains visible across view navigations

### Requirement: Application presents dedicated modal dialog for manual update outcomes
The application SHALL present a dedicated modal dialog in Azerbaijani communicating the exact outcome of a manual update check.

#### Scenario: New update available
- **WHEN** manual check finds a newer version
- **THEN** the application displays a modal dialog in Azerbaijani showing the new version number, release notes, digital authenticity confirmation, and an "İndi yenilə" button

#### Scenario: Application is up to date
- **WHEN** manual check finds no newer version
- **THEN** the application displays a modal dialog in Azerbaijani confirming that the user is running the latest version with an "Oldu" close button

#### Scenario: Network or connection failure
- **WHEN** manual check fails due to network outage or API error
- **THEN** the application displays a modal dialog in Azerbaijani explaining the network error with "Bağla" and "Yenidən cəhd et" options

#### Scenario: Running in unpackaged development environment
- **WHEN** manual check is invoked in an unpackaged/development environment
- **THEN** the application displays a modal dialog in Azerbaijani informing that update checking is disabled in development mode

### Requirement: User can download updates with visual progress and restart on completion
The application SHALL support in-dialog download progress tracking and automated restart.

#### Scenario: Interactive download progress
- **WHEN** the user clicks "İndi yenilə" in the update dialog
- **THEN** the dialog transitions to a downloading state displaying a progress bar from 0% to 100% and current percentage

#### Scenario: Automated restart on download completion
- **WHEN** the download reaches 100% completion
- **THEN** the application applies the update package and restarts seamlessly

### Requirement: Direct restart prompt when update is already downloaded
The application SHALL offer immediate restart if an update was already downloaded by the startup background worker.

#### Scenario: Clicking update button with downloaded update ready
- **WHEN** the background worker has already downloaded an update package and the user clicks the top bar update button
- **THEN** the application skips checking and re-downloading, directly opening the dialog with a "Quraşdır və yenidən başlat" action
