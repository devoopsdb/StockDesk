## 1. Converter & ViewModel Updates

- [ ] 1.1 Update `NullToVisibilityConverter` in `ValueConverters.cs` to support `ConverterParameter="invert"` and verify with unit tests
- [ ] 1.2 Add `IsEmptyStateVisible` property to `MainViewModel` and update its state in `ApplyFiltersAndSort()`
- [ ] 1.3 Add unit tests in `StockDesk.Tests` verifying `IsEmptyStateVisible` updates when products are loaded and filtered

## 2. UI Binding & Verification

- [ ] 2.1 Update empty state `StackPanel` in `MainWindow.xaml` to bind visibility to `IsEmptyStateVisible` using `BoolToVis`
- [ ] 2.2 Run all tests via `dotnet test` to confirm clean build and full test suite passing
