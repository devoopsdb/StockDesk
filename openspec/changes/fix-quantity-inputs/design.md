# Design: Fix Quantity Inputs in Dialogs

## Context

See `proposal.md` for motivation. In the desktop application, `ProductDialog`, `ReplenishDialog`, and `WriteOffDialog` use WPF-UI's `ui:NumberBox` control to capture integer quantities. In WPF-UI, `NumberBox` updates its `Value` dependency property upon `LostFocus`, Enter keypress, or spin-button clicks. However, clicking a WPF `ui:Button` does not naturally shift keyboard focus away from the active input before click events execute, leaving the ViewModel with the default property value of 1.

## Goals / Non-Goals

**Goals:**
- Ensure robust two-way synchronization between `ui:NumberBox` and ViewModel quantity properties across all dialogs.
- Clear/transfer focus or force binding updates whenever primary action buttons are clicked.
- Add comprehensive automated tests for ViewModels handling arbitrary quantity values.

**Non-Goals:**
- Replacing WPF-UI with another UI framework.
- Changing database schemas or entity models.

## Decisions

### Decision 1: Explicit `UpdateSourceTrigger=PropertyChanged` on `NumberBox.Value` bindings
- **Rationale**: Setting `UpdateSourceTrigger=PropertyChanged` ensures that as soon as `NumberBox.Value` changes (via typing, spin buttons, or programmatic change), the ViewModel property is updated immediately.
- **Alternative considered**: Relying solely on `LostFocus` — rejected because WPF button clicks do not reliably defocus the textbox.

### Decision 2: Defocusing active element in dialog click handlers
- **Rationale**: Adding a helper/call in dialog code-behind (e.g. `FocusManager.SetFocusedElement(this, (UIElement)sender)`) forces the active control to commit its text input to `Value` before the ViewModel command or method runs.
- **Alternative considered**: Overriding the `NumberBox` control template — rejected as unnecessary complexity when standard WPF focus management and binding triggers resolve the problem cleanly.

## Risks / Trade-offs

- **[Risk]** Type mismatch between `NumberBox.Value` (`double?` / `double`) and ViewModel `int` properties.
  - **Mitigation:** WPF handles double-to-int conversion automatically for integer ViewModel properties. Ensure ViewModels and tests validate integer ranges and non-negative constraints.
