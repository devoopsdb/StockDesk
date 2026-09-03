## Context

In WPF-UI 4.3.0, `DefaultCheckBoxStyle` specifies `Padding="11,5,11,6"` and a default `MinWidth`. In `MainWindow.xaml`, the selection checkbox is hosted inside a `DataGridTemplateColumn` with `Width="48"`. Given `DataGridCell` padding (`6,0,6,0`), the inner cell width is 36px. The CheckBox template's `BulletDecorator` uses `Margin="{TemplateBinding Control.Padding}"`, taking 22px of horizontal space and leaving only 14px for the 20px checkbox bullet, resulting in 6px clipping on the right side.

## Goals / Non-Goals

**Goals:**
- Eliminate checkbox border and checkmark glyph clipping in `MainWindow.xaml`.
- Preserve the WPF-UI Fluent 2 visual style and animation.
- Keep the change localized to avoid unwanted side effects on other dialogs or controls.

**Non-Goals:**
- Do not replace `DataGridTemplateColumn` with `DataGridCheckBoxColumn` (which renders Win32 classic checkboxes not adhering to Fluent 2 styling).
- Do not globally modify `DefaultCheckBoxStyle` in `App.xaml`, as standard form checkboxes rely on their default margins/padding for content spacing.

## Decisions

### Decision: Reset CheckBox layout properties locally in DataGridTemplateColumn

- **Choice**: Add `Padding="0"`, `MinWidth="0"`, and `MinHeight="0"` to the `<CheckBox>` element inside the `DataGridTemplateColumn.CellTemplate`.
- **Rationale**:
  - Setting `Padding="0"` removes the 11px left/right margins from `BulletDecorator`.
  - The 20x20 bullet fits comfortably inside the 36px cell interior with 8px margins on each side when centered.
  - Resetting `MinWidth="0"` and `MinHeight="0"` prevents the default 120px/30px minimum dimensions from causing container clipping.
- **Alternatives considered**:
  - *DataGridCheckBoxColumn*: Uses standard WPF CheckBox without WPF-UI accent styling, breaking UI consistency.
  - *Widening the column to 70px*: Wastes horizontal space and leaves excessive whitespace around the checkbox.
  - *Global Style override*: Risks regressing CheckBox layout across all other views/dialogs.

## Risks / Trade-offs

- **[Risk]** Clickable hit-test area might be smaller if only the 20x20 bullet is clickable.
  - **Mitigation**: The entire 48px cell column is dedicated to selection; the 20x20 box is centered and easily clickable on desktop/touch.
