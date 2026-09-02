## Why

Currently, StockDesk lacks an automated build, test, signing, packaging, and delivery pipeline. Additionally, users have no seamless way to receive application updates automatically. Implementing a CI/CD process with code signing and background auto-updates via GitHub Releases ensures rapid, reliable delivery of signed desktop binaries and a frictionless update experience without requiring administrator privileges.

## What Changes

- **CI/CD Automation**: Add a single GitHub Actions workflow (`.github/workflows/ci-cd.yml`) that validates pull requests and pushes (`dotnet test`) and automatically builds, signs, packages, and creates GitHub Releases when version tags (`v*.*.*`) or manual release dispatches are triggered.
- **Code Signing & Key Management**: Implement Authenticode signing using SignTool with support for repository secret PFX keys (`CODE_SIGN_CERT_BASE64`, `CODE_SIGN_PASSWORD`) and automatic fallback to generate a self-signed code signing certificate during CI if secrets are not configured. Provide helper scripts for local certificate generation.
- **Installer & Packaging**: Package the application with Velopack (`StockDesk-Setup.exe` and portable release assets) allowing zero-UAC installation directly into `%LocalAppData%`.
- **Seamless Auto-Updates**: Integrate Velopack update manager into the WPF application to check for new GitHub Releases on startup or demand, download delta/full updates in the background, and seamlessly apply updates on restart.

## Capabilities

### New Capabilities
- `auto-update`: Background checking, downloading, and applying of application updates from GitHub Releases using Velopack without requiring administrative privileges.
- `ci-cd-release`: GitHub Actions workflow automating build verification, code signing, installer generation, and GitHub Releases publication.

### Modified Capabilities
<!-- None -->

## Impact

- **Build & Packaging**: Added `.github/workflows/ci-cd.yml` and `scripts/generate-cert.ps1`.
- **Dependencies**: Added `Velopack` NuGet package to `StockDesk.csproj`.
- **Application Startup**: Configured Velopack initialization in `App.xaml.cs` and an update check service in the background.
