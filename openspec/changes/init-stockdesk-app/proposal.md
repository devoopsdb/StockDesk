## Why

StockDesk is needed as an offline Windows desktop application for operational warehouse and inventory tracking. It provides simple, reliable category and product catalog management with photo previews, accurate stock level monitoring, atomic single and bulk stock write-offs (issuance) to specific recipients, and an immutable operation audit log entirely localized in Azerbaijani.

## What Changes

Initialize the core StockDesk application with:
- **C# .NET 10 + WPF** desktop application with modern Windows 11 Fluent UI (`WPF-UI`).
- **Local SQLite Database** with Write-Ahead Logging (WAL) stored in `%LocalAppData%/StockDesk/stockdesk.db`.
- **Local Image Storage** in `%LocalAppData%/StockDesk/Images/` supporting JPG, PNG, and WEBP formats with auto-generated unique identifiers.
- **Category Management**: Create, edit, and delete product categories with uniqueness validation and dependency protection.
- **Product Catalog**: Live search, category filtering, multi-criteria sorting, thumbnail photo support with fallback placeholders, and multi-selection checkboxes.
- **Stock Inflow & Restock (Mədaxil)**: Record initial stock upon creation and replenish balances for existing items.
- **Stock Write-off / Issuance (Məxaric)**: Single and batch/bulk write-off of products to recipients with zero-negative balance validation.
- **Recipient Directory**: Dynamic recipient suggestions with automatic saving on write-off operations.
- **Immutable Operations History (Tarixçə)**: Append-only audit log capturing all inflows and outflows with color-coded badges, product snapshots, and multi-parameter filtering.
- **Azerbaijani Localization**: Complete user interface and error messaging in Azerbaijani (`az-Latn-AZ`).

## Capabilities

### New Capabilities
- `category-management`: Managing product categories (creation, editing, deletion with integrity checks).
- `product-catalog`: Visual product inventory listing, instant search, category filtering, sorting, photo previews, and multi-item selection.
- `stock-operations`: Stock adjustments including initial inflow, restocking (Mədaxil), and single/bulk write-offs (Məxaric) with strict non-negative balance checks.
- `recipient-management`: Tracking issuance recipients with inline autocomplete and persistent dictionary storage.
- `operation-history`: Immutable audit journal for inventory movements with filtering and historical snapshots.

### Modified Capabilities
<!-- None: new project initialization -->

## Impact

- **New Projects & Components**: `StockDesk` WPF Application targeting `net10.0-windows`, MVVM architecture with `CommunityToolkit.Mvvm`, EF Core with SQLite, image processing with `SkiaSharp` / `ImageSharp`, and `WPF-UI`.
- **File System / Storage**: Creates `%LocalAppData%/StockDesk` directory containing `stockdesk.db` and `Images/` folder.
- **Dependencies**: `Wpf.Ui`, `CommunityToolkit.Mvvm`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.DependencyInjection`, `SkiaSharp` (or `SixLabors.ImageSharp`).
