# Tasks: Self-Contained Hybrid Release Script (update.bat)

## 1. Script Scaffold & Version Engine

- [x] 1.1 Create `update.bat` polyglot batch/PowerShell scaffold in the repository root and verify it runs cleanly when executed from command prompt
- [x] 1.2 Implement current version reader from `src/StockDesk/StockDesk.csproj` and compute next SemVer options (patch, minor, major)
- [x] 1.3 Implement Git tag collision detection against local (`git tag -l`) and remote (`git ls-remote`) tags to prevent re-using existing tags like `v1.0.5`

## 2. Pre-flight Validation & Test Execution

- [x] 2.1 Implement Git working tree cleanliness check and `main` branch verification with informative prompts
- [x] 2.2 Integrate automated test execution (`dotnet test StockDesk.sln -c Release --no-restore`) and abort release if tests fail

## 3. Project & Changelog Synchronization

- [x] 3.1 Implement `<Version>` XML update in `src/StockDesk/StockDesk.csproj` while preserving indentation and formatting
- [x] 3.2 Implement UTF-8 safe `CHANGELOG.md` parser and updater to move `## [Unreleased]` items into `## [X.Y.Z] — YYYY-MM-DD`
- [x] 3.3 Implement atomic Git operations (`git add` of modified files, `git commit`, annotated `git tag`, and interactive confirmation before `git push`)

## 4. End-to-End Verification

- [x] 4.1 Test `update.bat` interactive menu and version suggestion display
- [x] 4.2 Verify UTF-8 characters in `CHANGELOG.md` remain uncorrupted after script execution
