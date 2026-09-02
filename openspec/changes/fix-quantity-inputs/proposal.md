# Proposal: Fix Quantity Inputs in Dialogs

## Why

When users enter initial stock quantities during product creation (`ProductDialog`) or replenish quantities (`ReplenishDialog`) or write-off quantities (`WriteOffDialog`), typing a custom number and immediately clicking the primary confirmation button causes only `1` unit to be processed instead of the entered amount. This is caused by WPF focus retention on `ui:NumberBox`, which delays parsing text into the numeric `Value` property and fails to push changes to the ViewModel before the dialog closes.

## What Changes

- Update `ui:NumberBox` bindings across `ProductDialog.xaml`, `ReplenishDialog.xaml`, and `WriteOffDialog.xaml` to explicitly include `UpdateSourceTrigger=PropertyChanged`.
- Ensure dialog code-behind files (`ProductDialog.xaml.cs`, `ReplenishDialog.xaml.cs`, `WriteOffDialog.xaml.cs`) programmatically clear keyboard focus / commit active bindings before executing save and confirmation logic.
- Add comprehensive unit and ViewModel tests verifying that arbitrary initial quantities and replenish/write-off quantities are properly reflected in ViewModels and persisted via `InventoryService`.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `product-catalog`: Product creation reliably respects the user-entered initial stock quantity.
- `stock-operations`: Inflow (replenishment) and outflow (write-off) dialogs reliably capture and apply the exact user-specified quantities.

## Impact

- `src/StockDesk/Views/Dialogs/ProductDialog.xaml` & `ProductDialog.xaml.cs`
- `src/StockDesk/Views/Dialogs/ReplenishDialog.xaml` & `ReplenishDialog.xaml.cs`
- `src/StockDesk/Views/Dialogs/WriteOffDialog.xaml` & `WriteOffDialog.xaml.cs`
- `tests/StockDesk.Tests/` (new/updated ViewModel and dialog integration tests)
