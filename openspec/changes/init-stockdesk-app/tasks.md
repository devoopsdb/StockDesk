## 1. Project Infrastructure & Core Setup

- [ ] 1.1 Create .NET 10 WPF project structure with `Wpf.Ui`, `CommunityToolkit.Mvvm`, `Microsoft.EntityFrameworkCore.Sqlite`, and `SkiaSharp` dependencies, verifying `dotnet build` succeeds.
- [ ] 1.2 Configure application lifecycle, DI container in `App.xaml.cs`, and local storage directories in `%LocalAppData%/StockDesk` (`stockdesk.db` and `Images/`).

## 2. Data Layer & Image Storage

- [ ] 2.1 Implement EF Core `StockDbContext` with entities (`Category`, `Product`, `Recipient`, `InventoryOperation`), unique constraints, non-negative balance checks, and WAL mode configuration.
- [ ] 2.2 Implement `IImageStorageService` supporting JPG, PNG, and WEBP decoding/conversion, unique GUID storage, and non-file-locking `BitmapSource` loading.

## 3. Business Logic & Repositories

- [ ] 3.1 Implement category management service with uniqueness validation and deletion protection when associated products exist.
- [ ] 3.2 Implement stock operations service supporting product creation inflow, restocking (Mədaxil), and transactional single/bulk write-off (Məxaric) with strict balance validation.
- [ ] 3.3 Implement recipient service with live autocomplete suggestions and automatic database persistence during write-off confirmation.

## 4. ViewModels & Application Logic

- [ ] 4.1 Implement `MainViewModel` with live search, category filtering, multi-criteria sorting, product observable collection, and multi-selection state tracking.
- [ ] 4.2 Implement `CategoryDialogViewModel` and `ProductDialogViewModel` with image selection, preview, and validation.
- [ ] 4.3 Implement `WriteOffDialogViewModel` supporting single and bulk mode, editable recipient combo, and dynamic balance limits.
- [ ] 4.4 Implement `HistoryViewModel` with chronological sorting, type filters, date filters, recipient filters, and color-coded status badges.

## 5. UI Views with WPF-UI & Fluent 2

- [ ] 5.1 Build `MainWindow.xaml` featuring top search/filter/action bar, product catalog list with photo thumbnails/placeholders, row actions, and slide-in bulk action bottom bar.
- [ ] 5.2 Build modal dialogs (`CategoryDialog.xaml`, `ProductDialog.xaml`, `WriteOffDialog.xaml`) with modern Fluent controls, image preview block, and responsive validation states.
- [ ] 5.3 Build `HistoryWindow.xaml` with styled data grid, green/red operation badges, and filter toolbar.

## 6. Localization & Verification

- [ ] 6.1 Review and ensure all UI labels, placeholders, dialog titles, validation errors, and notification messages are 100% in Azerbaijani (`az-Latn-AZ`).
- [ ] 6.2 Execute full application build, database creation, test product addition with WEBP/PNG images, and verify atomic single and bulk write-offs in the operation history.
