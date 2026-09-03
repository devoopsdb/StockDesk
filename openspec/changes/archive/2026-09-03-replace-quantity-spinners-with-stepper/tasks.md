## 1. Stepper Control Creation & Testing

- [x] 1.1 Add headless/STA unit tests for `QuantityStepper` dependency properties, bounds coercion (`Minimum`, `Maximum`), increment/decrement behavior, and two-way binding synchronization in `tests/StockDesk.Tests/QuantityStepperTests.cs`.
- [x] 1.2 Implement `QuantityStepper.xaml` and `QuantityStepper.xaml.cs` in `src/StockDesk/Controls` with minus/plus buttons, centered numeric text input, button boundary disabling, mouse wheel / arrow key support, and verify unit tests pass with `dotnet test`.

## 2. Dialog Integration

- [x] 2.1 Update `WriteOffDialog.xaml` to use `QuantityStepper`, set `Minimum="1"`, bind `Maximum="{Binding Product.CurrentBalance}"`, expand the quantity column from 130px to 160px, and verify compilation.
- [x] 2.2 Update `ReplenishDialog.xaml` to use `QuantityStepper` with `Minimum="1"`, and verify compilation.
- [x] 2.3 Update `ProductDialog.xaml` to use `QuantityStepper` with `Minimum="0"`, and verify compilation.
- [x] 2.4 Update dialog integration tests in `DialogViewModelTests.cs` to ensure two-way synchronization with `QuantityStepper` and verify with `dotnet test`.

## 3. Verification & Changelog

- [x] 3.1 Run full build and test suite (`dotnet test`) to verify zero compilation errors and all tests passing.
- [x] 3.2 Update `CHANGELOG.md` under `## [Unreleased]` documenting the new `QuantityStepper` control and replacement of `NumberBox` across all dialogs.
