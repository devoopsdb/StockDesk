## Context

See proposal.md for background. The empty state `StackPanel` in `MainWindow.xaml` was bound to `FilteredProducts.Count` using `NullToVisibilityConverter` with `ConverterParameter=invert`. Because integer counts are never null and the converter ignored parameter values, the empty state remained visible at all times.

## Goals / Non-Goals

**Goals:**
- Guarantee that the empty state overlay is only visible when `FilteredProducts.Count == 0`.
- Restore placeholder image icons for products without custom images by making `NullToVisibilityConverter` honor `ConverterParameter="invert"`.
- Keep XAML clean, idiomatic, and MVVM-compliant without convoluted count-to-visibility converters in XAML.
- Add unit tests verifying converter behavior and view model empty state flag logic.

**Non-Goals:**
- Altering the visual design or wording of the empty state screen.
- Changing search or sorting algorithms.

## Decisions

### Decision 1: Explicit `IsEmptyStateVisible` observable property on `MainViewModel`
- **Approach:** Introduce `[ObservableProperty] private bool _isEmptyStateVisible;` in `MainViewModel`. Update it inside `ApplyFiltersAndSort()` as `IsEmptyStateVisible = FilteredProducts.Count == 0;`.
- **Binding in XAML:** `<StackPanel Visibility="{Binding IsEmptyStateVisible, Converter={StaticResource BoolToVis}}" ...>` using the existing `BoolToVis` converter resource.
- **Alternatives Considered:**
  - *Collection count converter in XAML*: Writing a custom `CountToVisibilityConverter`. Rejected because having an explicit boolean property on the ViewModel is more transparent, testable headless, and aligns with standard MVVM principles.

### Decision 2: Enhance `NullToVisibilityConverter` to support `ConverterParameter="invert"`
- **Approach:** Inspect `parameter is string param && param.Equals("invert", StringComparison.OrdinalIgnoreCase)` in `NullToVisibilityConverter.Convert`.
- **Rationale:** Existing bindings in `MainWindow.xaml` and `WriteOffDialog.xaml` already declare `ConverterParameter=invert` intending to invert the null check for the placeholder icon `Image24`. Supporting this parameter fixes the missing thumbnail icons across dialogs and the main window without touching every individual XAML binding.

## Risks / Trade-offs

- [Risk] Timing of `IsEmptyStateVisible` updates during initialization → [Mitigation] Initialize `IsEmptyStateVisible = true;` by default so no table flash occurs before the initial data load completes; `ApplyFiltersAndSort()` updates it immediately after `FilteredProducts` is populated.
