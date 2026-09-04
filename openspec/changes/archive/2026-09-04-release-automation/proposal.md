# Proposal: Automated Release Script (update.bat)

## Why
Currently, preparing and publishing a new release requires several manual, error-prone steps: inspecting previous tags, bumping `<Version>` in `StockDesk.csproj`, moving unreleased notes in `CHANGELOG.md` to a versioned section, creating an annotated Git commit, and pushing a versioned tag `v*.*.*` to trigger GitHub Actions. Automating this through a single self-contained polyglot script (`update.bat`) eliminates manual errors, prevents duplicate tag collisions, guarantees UTF-8 integrity in documentation, and streamlines the Velopack release lifecycle.

## What Changes
- Introduce a self-contained, hybrid batch/PowerShell script `update.bat` in the repository root.
- Automatically detect the current version from `src/StockDesk/StockDesk.csproj` and suggest the next semantic version (both patch and minor increments, checking against local and remote Git tags).
- Allow either interactive selection or command-line arguments (e.g. `update.bat 1.1.0` or `update.bat patch`).
- Validate pre-conditions: working tree cleanliness, active branch verification, and running test suite (`dotnet test`) prior to release.
- Automatically update `<Version>` in `src/StockDesk/StockDesk.csproj`.
- Automatically update `CHANGELOG.md` in UTF-8 encoding (preserving non-ASCII localization characters), migrating `## [Unreleased]` items into a new versioned section `## [X.Y.Z] — YYYY-MM-DD`.
- Stage updated files, create release commit `chore(release): bump version to X.Y.Z`, create annotated tag `vX.Y.Z`, and prompt for confirmation before pushing to remote repository to trigger GitHub Actions CD.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `ci-cd-release`: Add requirements for automated developer release workflow, version bump validation, pre-flight checks, and automated commit/tag triggering.

## Impact
- Files: Adds `update.bat` in the repository root. Updates `openspec/specs/ci-cd-release/spec.md`.
- Workflow: Developers can trigger releases by running `update.bat` or double-clicking it on Windows.
- CI/CD: The script triggers the existing GitHub Actions pipeline (`.github/workflows/ci-cd.yml`) via pushed `v*.*.*` tags.
