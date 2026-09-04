# ci-cd-release Specification

## Purpose
Defines the continuous integration, continuous delivery, Authenticode code signing, installer packaging, and automated GitHub Releases publishing workflow.

## Requirements

### Requirement: Continuous Integration validation on pull requests and pushes
The CI pipeline SHALL trigger automatically on pull requests and pushes targeting the main branch to compile the solution and execute all automated unit tests.

#### Scenario: Pull request build and test success
- **WHEN** a pull request is submitted or updated with new commits
- **THEN** the workflow compiles the solution on a Windows runner and runs all unit tests, reporting status back to the pull request

#### Scenario: Unit test failure blocks CI
- **WHEN** a pull request introduces failing unit tests or compilation errors
- **THEN** the workflow fails and marks the check as failed

### Requirement: Authenticode Code Signing of binaries and installer
The CD pipeline SHALL sign the application executable and the installer binary with Authenticode SHA-256 signatures and RFC 3161 timestamps during release builds.

#### Scenario: Signing with repository secrets
- **WHEN** `CODE_SIGN_CERT_BASE64` and `CODE_SIGN_PASSWORD` are configured in repository secrets
- **THEN** the workflow decodes the PFX certificate, executes SignTool with timestamping on `StockDesk.exe` and `StockDesk-Setup.exe`, and cleans up the certificate file after signing

#### Scenario: Fallback self-signed certificate generation
- **WHEN** repository code signing secrets are not present
- **THEN** the workflow generates an automated fallback code signing certificate in the runner session to ensure successful artifact packaging without failing the pipeline

### Requirement: Automated installer creation and GitHub Release publishing
The CD pipeline SHALL package the application into a user-level installer (`StockDesk-Setup.exe`), generate update metadata assets, and publish a GitHub Release when a version tag is pushed.

#### Scenario: Release triggered by version tag
- **WHEN** a Git tag matching `v*.*.*` is pushed or a manual release workflow dispatch is executed
- **THEN** the workflow publishes a self-contained Windows x64 binary, packages the Velopack installer and release assets, creates a GitHub Release with auto-generated release notes, and attaches the release assets

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
