## ADDED Requirements

### Requirement: Self-contained developer release script
The repository SHALL provide a self-contained hybrid batch script (`update.bat`) that automates version resolution, project file updates, changelog tracking, and git release triggers in a single command.

#### Scenario: Running release script without arguments
- **WHEN** developer runs `update.bat` without command-line arguments
- **THEN** the script displays the current version extracted from `src/StockDesk/StockDesk.csproj`, presents calculated next SemVer options (patch, minor, major), prompts for user choice or custom version entry, and ensures the chosen version is not an existing Git tag

#### Scenario: Running release script with explicit version argument
- **WHEN** developer runs `update.bat <version>` (e.g. `update.bat 1.1.0`)
- **THEN** the script validates the SemVer format `X.Y.Z`, verifies that tag `v<version>` does not already exist locally or on remote, and proceeds with the release flow

#### Scenario: Pre-flight validation failure aborts release
- **WHEN** developer executes `update.bat` and the Git working tree has uncommitted modifications, the current branch is not `main`, or unit tests (`dotnet test`) fail
- **THEN** the script displays a descriptive error message and exits without modifying files, committing, or creating tags

#### Scenario: Automated synchronization of project version and changelog
- **WHEN** developer confirms the target release version
- **THEN** the script updates the `<Version>` element in `src/StockDesk/StockDesk.csproj` and moves all items under `## [Unreleased]` in `CHANGELOG.md` into a new `## [X.Y.Z] — YYYY-MM-DD` release section while preserving UTF-8 encoding and resetting `## [Unreleased]`

#### Scenario: Atomic commit and tag creation with push confirmation
- **WHEN** project files and changelog are updated
- **THEN** the script stages only the modified project file and changelog, commits with message `chore(release): bump version to X.Y.Z`, creates annotated Git tag `vX.Y.Z`, and prompts for developer confirmation before pushing the branch and tag to origin to trigger the CI/CD pipeline
