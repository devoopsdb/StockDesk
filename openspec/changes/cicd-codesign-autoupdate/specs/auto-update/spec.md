## Purpose

Provides automatic update checking, background downloading, and seamless application restart for StockDesk desktop application via GitHub Releases.

## ADDED Requirements

### Requirement: Application automatically checks for updates on startup
The application SHALL query GitHub Releases upon startup in a non-blocking background task to check if a newer version of the application is available.

#### Scenario: Update available on GitHub Releases
- **WHEN** the application starts and a newer release version exists on the GitHub repository
- **THEN** the application detects the newer version, logs the update availability, and initiates a background download of the delta or full release assets

#### Scenario: Application is up to date
- **WHEN** the application starts and the current version matches or exceeds the latest release
- **THEN** the application continues normal startup without downloading updates

### Requirement: Application downloads updates in background
The application SHALL download update packages from GitHub Releases asynchronously without blocking the user interface or interrupting ongoing stock operations.

#### Scenario: Background download completed
- **WHEN** the update download finishes successfully
- **THEN** the update is prepared for execution on the next application restart

#### Scenario: Network failure during update download
- **WHEN** network connectivity is lost while downloading an update
- **THEN** the application gracefully handles the network error, does not crash, and preserves normal application functionality

### Requirement: Application applies updates seamlessly on restart
The application SHALL apply the prepared update when restarting or upon application closure without requiring administrative UAC prompts.

#### Scenario: Applying update on restart
- **WHEN** a new update has been downloaded and the application restarts
- **THEN** the application launches the updated version seamlessly preserving local SQLite data and user files
