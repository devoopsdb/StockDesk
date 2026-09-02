## 1. Asset Deployment

- [x] 1.1 Deploy multi-resolution `AppIcon.ico` and master `AppIcon.png` into `src/StockDesk/Assets/` and verify files exist
- [x] 1.2 Validate that `AppIcon.ico` contains all standard mip resolutions (16x16 through 256x256) with valid headers

## 2. WPF Project Integration

- [x] 2.1 Update `src/StockDesk/StockDesk.csproj` with `<ApplicationIcon>` and `<Resource>` definitions
- [x] 2.2 Update `src/StockDesk/Views/MainWindow.xaml` to set `Icon="pack://application:,,,/Assets/AppIcon.ico"` on `<ui:FluentWindow>`
- [x] 2.3 Build the project via `dotnet build src/StockDesk/StockDesk.csproj` and verify zero compilation errors

## 3. Packaging & CI/CD Verification

- [x] 3.1 Check packaging scripts / CI workflow for Velopack `--icon` reference and update if necessary
- [x] 3.2 Run test suite via `dotnet test` to ensure all tests pass
