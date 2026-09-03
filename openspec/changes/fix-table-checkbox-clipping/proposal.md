## Why

In the main catalog table, the row selection checkboxes have their right borders and portions of the checkmark glyph visually clipped. This occurs because the WPF-UI default `CheckBox` style injects default padding and min-width constraints that exceed the available cell content width of the 48px column, making the checkboxes look truncated or obscured by the adjacent product column.

## What Changes

- Update the selection checkbox in `MainWindow.xaml` to reset `Padding="0"`, `MinWidth="0"`, and `MinHeight="0"`.
- Ensure the selection checkbox renders fully within the column bounds with complete borders and centered checkmark icon across both unselected and selected row states.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `product-catalog`: Clarify row selection checkbox rendering to require unobstructed, unclipped presentation in the table.

## Impact

- Modified file: `src/StockDesk/Views/MainWindow.xaml`.
- No breaking API or data schema changes.
