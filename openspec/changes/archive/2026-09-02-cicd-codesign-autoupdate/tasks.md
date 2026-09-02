## 1. Certificate and Signing Utilities

- [x] 1.1 Create `scripts/generate-cert.ps1` to generate self-signed code signing certificate and export PFX/CER files; verify running the script produces valid `.pfx` and `.cer` files.
- [x] 1.2 Create `scripts/sign-binary.ps1` to handle Authenticode signing with SignTool and fallback certificate generation; verify with `Get-AuthenticodeSignature`.

## 2. Velopack and Auto-Update Service

- [x] 2.1 Add `Velopack` NuGet dependency to `src/StockDesk/StockDesk.csproj` and verify `dotnet build` succeeds.
- [x] 2.2 Configure `VelopackApp.Build().Run()` in application entry point to handle Velopack hooks (install, update, uninstall).
- [x] 2.3 Implement `IUpdateService` and `UpdateService` for background GitHub Releases update checking and download; register in DI container.
- [x] 2.4 Add unit tests for `UpdateService` in `tests/StockDesk.Tests` and verify all tests pass with `dotnet test`.

## 3. GitHub Actions CI/CD Pipeline

- [x] 3.1 Create `.github/workflows/ci-cd.yml` with `build-and-test` job (PR and push to main) and `release` job (`v*.*.*` tag and manual dispatch).
- [x] 3.2 Configure release publishing steps (self-contained win-x64, binary signing, `vpk pack` packaging, installer signing, and GitHub Release upload).
- [x] 3.3 Add documentation in `README.md` explaining GitHub Secrets setup, release tagging workflow, and importing public certificate.
