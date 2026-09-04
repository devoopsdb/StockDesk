## 1. Product Catalog Date Format

- [x] 1.1 Update `MainWindow.xaml` to format `CreatedAt` as `dd.MM.yyyy` with width 110 and verify the build succeeds

## 2. Operation History Date & Time Columns

- [x] 2.1 Update `HistoryWindow.xaml` to replace the combined `Tarix və Saat` column with separate `Tarix` (`dd.MM.yyyy`, width 105) and `Saat` (`HH:mm`, width 75) columns
- [x] 2.2 Verify both columns bind to `Timestamp` and preserve native chronological sorting on header click

## 3. Verification & Changelog

- [x] 3.1 Run `dotnet build` and `dotnet test` to confirm compilation and existing tests pass with zero regressions
- [x] 3.2 Document changes under `## [Unreleased]` in `CHANGELOG.md`
