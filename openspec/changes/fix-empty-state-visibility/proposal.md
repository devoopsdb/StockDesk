# Proposal: Fix Empty State Visibility and Placeholder Rendering

## Why

When products exist in the catalog, the empty state placeholder ("Heç bir məhsul tapılmadı" and the "+ İlk məhsulu əlavə et" action button) remains persistently visible in the center of the window, overlapping the table rows. This happens because `FilteredProducts.Count` binds through a `NullToVisibilityConverter` that evaluates non-null for integer counts and ignores `ConverterParameter=invert`. Furthermore, products without photos fail to display the default placeholder icon due to the same ignored parameter. Fixing this ensures the empty state only displays when there are zero items, and image placeholders render correctly.

## What Changes

- Add an explicit, observable boolean property (`IsEmptyStateVisible` / `HasProducts`) to `MainViewModel` representing whether filtered items exist.
- Update `MainWindow.xaml` to bind the empty state overlay visibility to `IsEmptyStateVisible` using `BooleanToVisibilityConverter` (or `BoolToVis`).
- Enhance `NullToVisibilityConverter` in `ValueConverters.cs` to support `ConverterParameter="invert"` (case-insensitive string comparison) so null/whitespace checks can be cleanly inverted via binding parameters.
- Add unit tests for `NullToVisibilityConverter` parameter inversion and `MainViewModel` empty state property transitions.

## Capabilities

### Modified Capabilities
- `product-catalog`: Specifies that the catalog empty state overlay is visible only when no products are present, and placeholder icons display properly when a product has no image.

## Impact

- `StockDesk.Common.Converters.NullToVisibilityConverter`: Now honors `ConverterParameter="invert"` in addition to the existing CLR property `Invert`.
- `StockDesk.ViewModels.MainViewModel`: Exposes an observable property indicating whether products exist / whether the empty state should be shown.
- `StockDesk.Views.MainWindow`: Updates the visibility binding of the empty state `StackPanel` to reliably show only when zero products match.
- `StockDesk.Tests`: Tests for `NullToVisibilityConverter` and `MainViewModel` empty state behavior.
