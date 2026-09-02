## Why

StockDesk currently lacks a dedicated application icon (`AppIcon.ico`), resulting in default Windows executable placeholders in the taskbar, window title bar, and desktop shortcuts. Adding a modern Windows 11 Fluent 2 icon improves brand identity, visual polish, and professional user experience across all desktop interaction points.

## What Changes

- Add high-resolution multi-layer Windows icon asset (`src/StockDesk/Assets/AppIcon.ico`) and master PNG (`src/StockDesk/Assets/AppIcon.png`) featuring the Fluent Glass Parcel 3D design.
- Configure `StockDesk.csproj` to embed `ApplicationIcon` for the compiled binary.
- Configure `MainWindow.xaml` to display the official icon in the top-left title bar.
- Update packaging and release build scripts (`scripts/` and CI/CD) to ensure Velopack shortcuts use the official application icon.

## Capabilities

### New Capabilities
- `app-branding`: Application icon assets, window title bar branding, and desktop/taskbar visual identity.

### Modified Capabilities
<!-- None -->

## Impact

- **Affected Code:** `src/StockDesk/StockDesk.csproj`, `src/StockDesk/Views/MainWindow.xaml`, `src/StockDesk/Assets/`.
- **Dependencies & Tools:** Velopack packaging configuration, .NET 10 compilation assets.
- **Breaking Changes:** None.
