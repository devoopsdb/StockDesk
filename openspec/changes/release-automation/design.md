# Design: Self-Contained Hybrid Release Automation Script

## Context
See `proposal.md` for background motivation. StockDesk uses .NET 10 on Windows, GitHub Actions for CI/CD, Velopack for automatic packaging and updating, and Keep a Changelog format with UTF-8 localized text. The release pipeline is triggered by Git tags matching `v*.*.*`.

## Goals / Non-Goals

**Goals:**
- Provide a single, self-contained hybrid `update.bat` in the project root executable from CMD, PowerShell, or Windows Explorer double-click.
- Calculate and suggest the next semantic version (Patch, Minor, Major) based on `<Version>` in `src/StockDesk/StockDesk.csproj` and existing Git tags.
- Safely update `src/StockDesk/StockDesk.csproj` and `CHANGELOG.md` with zero character encoding loss.
- Run automated tests (`dotnet test`) and verify Git working tree cleanliness before touching files or tagging.
- Stage, commit, tag, and optionally push changes with explicit developer confirmation.

**Non-Goals:**
- Replace or duplicate GitHub Actions CD workflow (Velopack packaging and signing remain on GitHub Actions runner).
- Manage NuGet or external registry publishing.

## Decisions

### Decision 1: Polyglot Batch/PowerShell Architecture
- **Choice**: Implement `update.bat` using a clean hybrid polyglot pattern where cmd.exe executes PowerShell against the same file without temporary scripts.
  ```cmd
  <# : batch script section
  @echo off
  setlocal
  powershell -NoProfile -ExecutionPolicy Bypass -Command "$input | & { [ScriptBlock]::Create((Get-Content -Raw -LiteralPath '%~f0')) } %*"
  exit /b %ERRORLEVEL%
  : end batch section #>
  # PowerShell script section follows...
  ```
- **Rationale**: Windows cmd.exe cannot natively parse XML or multiline Markdown and mangles non-ASCII UTF-8 characters (e.g. Azerbaijani characters in `CHANGELOG.md`). A hybrid `.bat` allows zero-configuration execution from any Windows shell or double-click while leveraging full .NET/PowerShell capabilities.
- **Alternatives Considered**:
  - Pure Batch (`.bat`): Rejected due to severe limitations in UTF-8 text processing and complex markdown section parsing.
  - Separate `update.bat` + `scripts/release.ps1`: Rejected per user preference for a single self-contained file.

### Decision 2: SemVer Resolution and Tag Collision Detection
- **Choice**: Extract `<Version>(.*?)</Version>` from `src/StockDesk/StockDesk.csproj`. Calculate:
  - Patch: increment Z (`X.Y.(Z+1)`)
  - Minor: increment Y (`X.(Y+1).0`)
  - Major: increment X (`(X+1).0.0`)
- Check target tag `v$version` against:
  - Local tags: `git tag -l "v$version"`
  - Remote tags: `git ls-remote --tags origin "refs/tags/v$version"`
- Prompt user with choices `[1] Patch ($patch), [2] Minor ($minor), [3] Major ($major), [4] Custom input`. If a version is passed via CLI (`update.bat 1.1.0`), use it directly after validation.

### Decision 3: Safe CHANGELOG.md Section Migration
- **Choice**: Read `CHANGELOG.md` using `[System.IO.File]::ReadAllText` with UTF-8 encoding. Use regex to split around `## [Unreleased]`. If unreleased content exists, insert:
  ```markdown
  ## [Unreleased]

  ## [$version] — $today
  $unreleasedContent
  ```
  Write back using UTF-8 without BOM.
- **Rationale**: Keeps exact Keep a Changelog structure, preserves Unicode characters, and prevents corruption.

### Decision 4: Pre-flight Verification and Atomic Git Operations
- **Choice**:
  1. Check `git status --porcelain`. If modified/untracked files exist, require user confirmation or abort.
  2. Verify current branch is `main`.
  3. Run `dotnet test StockDesk.sln -c Release --no-restore` to guarantee tests pass before releasing.
  4. Perform atomic staging: `git add src/StockDesk/StockDesk.csproj CHANGELOG.md`.
  5. Commit: `git commit -m "chore(release): bump version to $version"`.
  6. Create annotated tag: `git tag -a "v$version" -m "Release v$version"`.
  7. Prompt developer: `Push commit and tag to origin to trigger CI/CD? [Y/n]`. Only push if confirmed.

## Risks / Trade-offs

- **[Risk] PowerShell execution policy restrictions**: On locked-down enterprise machines, `-ExecutionPolicy Bypass` might be blocked by group policy.
  - *Mitigation*: Use `-ExecutionPolicy Bypass` with `-Scope Process`, which succeeds on standard developer workstations.
- **[Risk] Unreleased section is empty in CHANGELOG.md**:
  - *Mitigation*: Detect if `## [Unreleased]` has no bullet points; issue a warning and prompt the developer if they still wish to proceed without changelog release notes.
