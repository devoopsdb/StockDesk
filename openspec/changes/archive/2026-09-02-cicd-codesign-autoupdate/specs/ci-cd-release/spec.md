## Purpose

Defines the continuous integration, continuous delivery, Authenticode code signing, installer packaging, and automated GitHub Releases publishing workflow.

## ADDED Requirements

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
