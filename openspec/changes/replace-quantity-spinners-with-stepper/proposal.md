## Why

In the current UI, quantity inputs across the application (`WriteOffDialog`, `ReplenishDialog`, and `ProductDialog`) use WPF-UI's `ui:NumberBox`. This introduces two severe usability issues:
1. When focused, `ui:NumberBox` displays a clear button (`X`) that overlaps and obscures the entered digits—especially in constrained layouts such as `WriteOffDialog` where the column is only 130px wide. Clearing quantity to empty is also nonsensical for stock operations and triggers validation errors.
2. The vertical chevron spin arrows (`^` / `v`) are unintuitive for inventory management, small to target with a mouse, and frequently mistaken for dropdown menu selectors.

Replacing these spinners with a dedicated, Fluent 2-styled `QuantityStepper` control (`[ − ] [ value ] [ + ]`) eliminates UI element clipping and provides clear, accessible quantity adjustment.

## What Changes

- Create a reusable `QuantityStepper` UserControl with:
  - Decrement (`−`) and Increment (`+`) buttons styled with Fluent icons (`SymbolRegular.Subtract20` / `Add20`).
  - Centered numeric text input with no clear button (`ClearButtonEnabled="False"`), digit-only input restriction, and auto-clamping on edit.
  - Boundary enforcement: decrement button is automatically disabled when `Value <= Minimum`; increment button is automatically disabled when `Value >= Maximum`.
  - Mouse wheel and arrow key support for rapid increment/decrement.
- Replace `ui:NumberBox` with `QuantityStepper` across all quantity inputs:
  - `WriteOffDialog.xaml`: Set `Minimum="1"`, `Maximum="{Binding Product.CurrentBalance}"`, and widen input column from 130px to 160px for ample breathing room.
  - `ReplenishDialog.xaml`: Set `Minimum="1"`.
  - `ProductDialog.xaml`: Set `Minimum="0"`.
- Add comprehensive automated unit tests covering `QuantityStepper` behavior (decrement, increment, min/max clamping, and two-way binding synchronization).

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `stock-operations`: Specify stepper control interaction, clear button removal, and automatic boundary disabling for replenish and write-off quantity fields.
- `product-catalog`: Specify stepper control interaction and clear button removal for product creation initial quantity.

## Impact

- UI Controls: New `QuantityStepper` component in `StockDesk.Controls`.
- Dialog Views: `WriteOffDialog.xaml`, `ReplenishDialog.xaml`, and `ProductDialog.xaml`.
- ViewModels: Retain existing integer binding properties (`Quantity`, `InitialQuantity`) with seamless two-way binding.
- Tests: New unit tests in `StockDesk.Tests` for `QuantityStepper` and updated existing dialog binding tests if applicable.
