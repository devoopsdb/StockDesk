# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
