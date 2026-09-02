## Context

StockDesk is a greenfield desktop inventory application for Windows built with .NET 10 and WPF. See `proposal.md` for overall motivation and capabilities. The app operates offline and stores all state locally in `%LocalAppData%/StockDesk`.

## Goals / Non-Goals

**Goals:**
- Provide a responsive, modern Windows 11 Fluent UI using `WPF-UI`.
- Maintain data integrity with SQLite transactions and immutable operation logs.
- Support common image formats including JPG, PNG, and WEBP without file locking issues.
- Deliver a native, seamless Azerbaijani-language user experience (`az-Latn-AZ`).

**Non-Goals:**
- Multi-user network synchronization or cloud backend (purely offline single-machine application).
- Barcode/QR scanner hardware SDK integrations (standard keyboard input only).
- Advanced accounting or fiscal tax compliance reporting.

## Decisions

### 1. UI Framework and MVVM Toolkit
- **Choice**: `Wpf.Ui` (Fluent 2) + `CommunityToolkit.Mvvm`.
- **Rationale**: `Wpf.Ui` provides polished Windows 11 aesthetics, native dark/light themes, and modern dialogs. `CommunityToolkit.Mvvm` leverages source generators (`[ObservableProperty]`, `[RelayCommand]`) for clean, high-performance MVVM without runtime reflection overhead.
- **Alternatives Considered**: Plain WPF with custom styles (too much maintenance) or `MaterialDesignInXAML` (user specifically requested `WPF-UI`).

### 2. Database and ORM Layer
- **Choice**: `Microsoft.EntityFrameworkCore.Sqlite` configured with Write-Ahead Logging (`PRAGMA journal_mode=WAL;`).
- **Rationale**: EF Core provides strongly-typed LINQ queries, migrations, relationships, and transaction management (`BeginTransactionAsync`). SQLite WAL mode ensures high read/write concurrency and resilience against crashes.
- **Alternatives Considered**: Dapper / raw ADO.NET (more manual SQL mapping, higher risk of subtle bugs in transactions).

### 3. File System & Image Storage Strategy
- **Choice**: Storage root at `Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "StockDesk")`.
  - Database: `.../StockDesk/stockdesk.db`
  - Images: `.../StockDesk/Images/<Guid>.<ext>`
- **Rationale**: Isolates application data cleanly in standard user space without requiring administrator privileges. Loading images into WPF will use `BitmapCacheOption.OnLoad` or `MemoryStream` to eliminate file locks.
- **Image Processing**: Use `SkiaSharp` to decode and normalize image inputs (including `.webp`) and generate standardized preview thumbnails (e.g., max 512x512).

### 4. Domain & Data Schema
```
Category
  - Id: int / Guid (PK)
  - Name: string (Unique index)
  - CreatedAt: DateTime

Product
  - Id: int / Guid (PK)
  - CategoryId: FK -> Category.Id (Restricted delete)
  - Name: string
  - ImageFileName: string (nullable)
  - CurrentBalance: int (CHECK CurrentBalance >= 0)
  - CreatedAt: DateTime

Recipient
  - Id: int / Guid (PK)
  - Name: string (Unique index, trimmed)
  - CreatedAt: DateTime

InventoryOperation
  - Id: int / Guid (PK)
  - Timestamp: DateTime
  - OperationType: enum (Inflow / Mədaxil = 1, Outflow / Məxaric = 2)
  - ProductId: FK -> Product.Id (nullable on delete, snapshot preserved)
  - ProductNameSnapshot: string
  - CategoryNameSnapshot: string
  - QuantityDelta: int
  - RecipientId: FK -> Recipient.Id (nullable)
  - RecipientNameSnapshot: string (nullable)
  - Note: string (nullable)
```

### 5. Atomic Stock Operations
- **Single & Bulk Write-off**: Wrapped in an `IDbContextTransaction`.
  1. Verify each product's current database balance >= requested write-off quantity.
  2. If any validation fails, throw and roll back transaction.
  3. Deduct `Product.CurrentBalance -= Quantity`.
  4. Ensure `Recipient` exists (insert if new).
  5. Insert `InventoryOperation` record(s) with snapshot values.
  6. Commit transaction.

## Risks / Trade-offs

- **[Risk: WPF BitmapImage File Lock]** → *Mitigation:* Load images via a custom `IImageStorageService` that reads byte arrays into `MemoryStream` with `BitmapCacheOption.OnLoad`, ensuring image files are never locked on disk.
- **[Risk: WebP Decoding in WPF]** → *Mitigation:* Convert or decode WebP files via `SkiaSharp` during the import stage, ensuring all rendered images are directly compatible with standard WPF `BitmapSource`.
- **[Risk: Race Conditions on Balance]** → *Mitigation:* Check constraints at the database level (`CHECK(CurrentBalance >= 0)`) and execute stock adjustments within atomic transactions.
