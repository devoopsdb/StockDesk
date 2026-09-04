# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.6] — 2026-09-04

## [1.0.5] — 2026-09-04

### Added

- Interactive manual update check button in the top action bar of `MainWindow` with loading spinner, pending update dot badge, and automated disabled state to prevent duplicate clicks.
- Modern Fluent 2 modal dialog (`UpdateDialog.xaml`) and `UpdateDialogViewModel` fully localized in Azerbaijani, presenting release notes, digital security authenticity confirmation, real-time download progress percentage, seamless restart triggers, up-to-date notifications, and unpackaged dev mode feedback.
- Progress reporting (`IProgress<int>`) and update state management in `IUpdateService` and `UpdateService`.

### Fixed

- Fixed catalog empty state overlay ("Heç bir məhsul tapılmadı") persistently showing over populated product rows by introducing an observable `IsEmptyStateVisible` property on `MainViewModel` and updating `MainWindow.xaml` binding.
- Fixed `NullToVisibilityConverter` to honor `ConverterParameter="invert"`, restoring neutral placeholder icon display for products without photos across catalog and dialog views.

## [1.0.4] — 2026-09-03

### Added

- Added current application version to the main window title bar (`StockDesk v{version} - Operativ Anbar Uçotu`).
- Reusable `QuantityStepper` UserControl with segmented `[ − ] [ value ] [ + ]` layout, automatic button boundary disabling, direct numeric typing without clear buttons, and mouse wheel / keyboard arrow support.

### Changed

- Replaced `ui:NumberBox` with `QuantityStepper` across all quantity dialogs (`WriteOffDialog`, `ReplenishDialog`, `ProductDialog`), eliminating clear button (`X`) text clipping and replacing vertical chevron spin buttons with intuitive plus/minus controls.
- Expanded quantity column width in `WriteOffDialog` from 130px to 160px for enhanced visual hierarchy.

### Fixed

- Fixed bug where entered initial product quantity in `ProductDialog` reverted to default (1) upon clicking save.
- Fixed bug where entered replenishment quantity in `ReplenishDialog` reverted to default (1) upon confirming inflow.
- Fixed quantity binding and focus handling in `WriteOffDialog` to prevent entered write-off quantities from falling back to default values.
- Fixed visual clipping of table row selection checkboxes in `MainWindow` by resetting CheckBox padding and minimum dimensions.
