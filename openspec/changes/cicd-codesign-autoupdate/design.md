## Context

StockDesk is a standalone .NET 10 desktop application using WPF and SQLite. It currently lacks automated build validation, automated release artifact generation, code signing, and in-app update mechanisms. See `proposal.md` for motivation.

## Goals / Non-Goals

**Goals:**
- Implement a unified GitHub Actions pipeline (`.github/workflows/ci-cd.yml`) running on `windows-latest` for PR validation and tag-based release publishing.
- Support Authenticode code signing using SignTool with Base64 PFX secrets and an automated self-signed fallback if secrets are not configured.
- Provide a PowerShell utility (`scripts/generate-cert.ps1`) to create self-signed code signing certificates and export `.pfx` and `.cer` files.
- Package the application with Velopack (`StockDesk-Setup.exe`), enabling single-click installation without administrator privileges.
- Add an `IUpdateService` to StockDesk using Velopack to check for updates from GitHub Releases and apply them on restart.

**Non-Goals:**
- Commercial EV HSM hardware token integration (only software PFX / SignTool / self-signed certificates).
- Non-Windows packaging (Linux/macOS), as StockDesk is WPF-based (`net10.0-windows`).
- Hosting a custom update server (GitHub Releases will serve as the sole update source).

## Decisions

### Decision 1: Velopack for packaging and auto-updates
- **Choice:** Use `Velopack` NuGet package and `vpk` CLI tool.
- **Rationale:** Velopack is modern, actively maintained, purpose-built for .NET Windows apps, creates zero-UAC installers in `%LocalAppData%`, supports delta updates, integrates natively with GitHub Releases (`GithubSource`), and handles binary/installer code signing.
- **Alternatives considered:**
  - *Inno Setup + custom GitHub API checker:* Requires custom download and process management logic, plus UAC prompts for installation.
  - *AutoUpdater.NET:* Requires hosting an external XML/JSON file or configuring custom update dialogs.

### Decision 2: Unified single-workflow CI/CD (`.github/workflows/ci-cd.yml`)
- **Choice:** Single YAML workflow with two sequential jobs: `validate` (runs on push/PR) and `release` (runs on `v*.*.*` tags or `workflow_dispatch`).
- **Rationale:** Keeps workflow maintenance simple in a single file while maintaining fast feedback for PRs and full packaging/signing only for releases.

### Decision 3: Code Signing with SignTool + GitHub Secrets + Automated Fallback
- **Choice:** Read `CODE_SIGN_CERT_BASE64` and `CODE_SIGN_PASSWORD` from GitHub Secrets. If secrets are present, decode to a temporary PFX file and sign with RFC 3161 timestamp (`http://timestamp.digicert.com`). If secrets are omitted, generate a temporary self-signed certificate on the runner so that builds never fail. Always delete temporary `.pfx` files in a post-execution step.
- **Rationale:** Guarantees that the pipeline works out-of-the-box on new forks or initial repository setup, while allowing plug-and-play addition of production certificates.

### Decision 4: Non-blocking update service in WPF
- **Choice:** Register `IUpdateService` as a singleton in `App.xaml.cs`. On `OnStartup`, trigger update check in a background task without blocking UI initialization.
- **Rationale:** Ensures immediate application launch and zero UI freeze while querying GitHub Releases.

## Risks / Trade-offs

- **[Risk]** Windows SmartScreen may flag binaries signed with self-signed certificates.
  - **Mitigation:** Attach the public `.cer` certificate to each GitHub Release and provide a PowerShell command in the README / release notes for users to trust the certificate if desired.
- **[Risk]** GitHub API rate limits on unauthenticated release checks.
  - **Mitigation:** Query GitHub Releases once on application startup or manual request, catching and suppressing any network or rate limit exceptions gracefully.
