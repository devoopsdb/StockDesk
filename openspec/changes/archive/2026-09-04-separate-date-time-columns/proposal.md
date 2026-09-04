## Why

Currently, date and time values are rendered together in a single text column as `dd.MM.yyyy HH:mm` in both the product catalog and the operation history journal. This creates visual noise, prevents quick visual scanning of calendar dates, and impairs readability for warehouse operators. Displaying clean dates in the catalog and splitting date and time into separate dedicated columns in the history journal allows operators to scan records faster while preserving sorting and detail.

## What Changes

- In the product catalog (`MainWindow`), change the `Əlavə tarixi` column to display only the calendar date in `dd.MM.yyyy` format without time, reducing column width.
- In the operation history journal (`HistoryWindow`), replace the single `Tarix və Saat` column with two dedicated columns:
  - `Tarix` displaying `dd.MM.yyyy` (approx. 105px width).
  - `Saat` displaying `HH:mm` (approx. 75px width).
- Both columns bind to the same underlying `Timestamp` property, maintaining native chronological sorting when clicking either column header.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `product-catalog`: Product catalog table displays addition date as pure calendar date without time.
- `operation-history`: Operation history journal displays date and time in separate dedicated columns on the same row.

## Impact

- `src/StockDesk/Views/MainWindow.xaml`: Modified `DataGridTextColumn` for `CreatedAt`.
- `src/StockDesk/Views/Dialogs/HistoryWindow.xaml`: Replaced `DataGridTextColumn` for `Timestamp` with two separate columns for date and time.
- Zero changes to domain models, database schema, or SQLite persistence (timestamp remains `DateTime.UtcNow`).
