## Context

StockDesk uses WPF-UI (Fluent 2) on .NET 10. The application currently defaults to generic Windows binary icons. See `proposal.md` for motivation and `specs/app-branding/spec.md` for functional requirements.

## Goals / Non-Goals

**Goals:**
- Provide official multi-resolution icon assets (`AppIcon.ico` with 16x16 to 256x256 mip levels and `AppIcon.png` master image) in `src/StockDesk/Assets/`.
- Configure `StockDesk.csproj` to compile `AppIcon.ico` as `<ApplicationIcon>` and embed both as WPF `<Resource>`.
- Set `Icon="pack://application:,,,/Assets/AppIcon.ico"` on `MainWindow.xaml` to brand the window title bar.
- Update CI/CD and build scripts so Velopack installer `--icon` points to the new asset.

**Non-Goals:**
- Modifying dynamic app themes (dark/light mode switching already handled by WPF-UI).
- In-app iconography alterations (product placeholders, navigation icons remain untouched).

## Decisions

### 1. Asset Storage & WPF Inclusion
- **Decision:** Place assets in `src/StockDesk/Assets/AppIcon.ico` and `src/StockDesk/Assets/AppIcon.png`.
- **Rationale:** Standard .NET/WPF convention. Setting `<ApplicationIcon>Assets\AppIcon.ico</ApplicationIcon>` embeds it into the PE header for File Explorer and Taskbar, while `<Resource Include="Assets\AppIcon.ico" />` makes it accessible via Pack URI in XAML.
- **Alternative considered:** Embedding as Base64 in code (rejected: cannot be read by Windows PE loader before process start).

### 2. Multi-Resolution Format
- **Decision:** Include 7 standard Windows icon mip levels: 16x16, 24x24, 32x32, 48x48, 64x64, 128x128, 256x256 in 32-bit ARGB.
- **Rationale:** Ensures razor-sharp rendering on standard DPI (100%), High-DPI (125%, 150%, 200%), and small title bar constraints without bilinear blur.

## Risks / Trade-offs

- [Risk] Title bar icon scaling blur on odd DPI scaling settings → Mitigation: Pre-sharpened 16x16 and 32x32 layers included directly in `.ico`.
- [Risk] Velopack setup packaging failure if icon path is incorrect → Mitigation: Point `--icon` argument to `src/StockDesk/Assets/AppIcon.ico` in release workflow.
