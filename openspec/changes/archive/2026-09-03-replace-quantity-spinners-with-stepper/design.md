## Context

See `proposal.md` for motivation. In the desktop application, `WriteOffDialog`, `ReplenishDialog`, and `ProductDialog` capture integer quantities using `ui:NumberBox`. Because `NumberBox` inherits from `Wpf.Ui.Controls.TextBox`, it shows an intrusive `X` clear button on focus that obscures typed digits in tight layouts. Furthermore, its vertical chevron spin buttons are small and unintuitive for quantity modification.

## Goals / Non-Goals

**Goals:**
- Provide a reusable `QuantityStepper` UserControl in `StockDesk.Controls` matching Windows 11 / Fluent 2 design principles.
- Deliver an intuitive `[ − ] [  Value  ] [ + ]` segmented interaction with `SymbolRegular.Subtract20` and `SymbolRegular.Add20` icons.
- Support direct keyboard numeric entry (center-aligned, digits-only, no clear button) with immediate two-way ViewModel synchronization.
- Automatically disable the decrement button when `Value <= Minimum` and the increment button when `Value >= Maximum`.
- Provide mouse wheel scrolling and keyboard up/down support for rapid adjustments.
- Expand the quantity column in `WriteOffDialog.xaml` from 130px to 160px to provide balanced visual hierarchy.

**Non-Goals:**
- Decimal or fractional quantities (all inventory operations in StockDesk are integer piece counts `int`).
- Altering existing ViewModel interfaces, database models, or business logic in `InventoryService`.

## Decisions

### Decision 1: Dedicated `QuantityStepper` UserControl vs WPF-UI `NumberBox` ControlTemplate Override
- **Rationale**: `NumberBox` in WPF-UI is tightly coupled to `TextBox` internals, including hardcoded template part handling for clear buttons and vertical chevron spin buttons. Overriding its complex control template would be fragile across library updates. A clean, purpose-built `QuantityStepper` UserControl gives total control over visual styling, corner radiuses, button states, and keyboard/mouse interaction.
- **Alternative considered**: Overriding `NumberBox` style with `ClearButtonEnabled="False"`. Rejected because inline spin buttons remain chevron-based and lack independent boundary disable states (`IsEnabled` per button).

### Decision 2: DependencyProperties with Value Coercion
- **Rationale**: Implement `Value`, `Minimum`, `Maximum`, and `Step` as standard WPF `DependencyProperty` with `FrameworkPropertyMetadataOptions.BindsTwoWayByDefault`. Use `CoerceValueCallback` to guarantee that `Value` remains strictly within `[Minimum, Maximum]` regardless of input mechanism (buttons, typing, mouse wheel, or programmatic updates).
- **Default values**: `Minimum = 1` (overridable, e.g. 0 in `ProductDialog`), `Maximum = int.MaxValue`, `Step = 1`.

### Decision 3: Text Input Validation and Instant Synchronization
- **Rationale**: Handle `PreviewTextInput` to discard non-digit characters. During editing, parse text and synchronize `Value` immediately or on lost focus/Enter, ensuring the ViewModel property always matches without requiring explicit focus shifts.

## Risks / Trade-offs

- **[Risk]** Two-way binding race conditions or loopback when typing numbers.
  - **Mitigation**: Standard WPF dependency property change handlers that guard against redundant value assignments if the parsed value equals the current property value.
- **[Risk]** Existing unit tests expecting `ui:NumberBox` in `DialogViewModelTests`.
  - **Mitigation**: Existing unit tests in `DialogViewModelTests.cs` test ViewModel synchronization and can be updated/supplemented with headless and UI tests for `QuantityStepper`.
