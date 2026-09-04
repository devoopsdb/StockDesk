## Context

See `proposal.md` - Why. Currently, date and time are rendered together in single columns formatted as `dd.MM.yyyy HH:mm` in both `MainWindow.xaml` and `HistoryWindow.xaml`.

## Goals / Non-Goals

**Goals:**
- Present clean, readable calendar date (`dd.MM.yyyy`) without time in the catalog table (`MainWindow.xaml`).
- Separate calendar date and time of day into two dedicated columns (`Tarix` and `Saat`) in the operation history journal (`HistoryWindow.xaml`) on a single line.
- Preserve native sorting behavior: clicking either `Tarix` or `Saat` header sorts chronologically by `Timestamp`.

**Non-Goals:**
- Altering the database schema, EF Core entities, or SQLite storage (timestamps remain standard UTC `DateTime`).
- Changing date range filtering logic in `HistoryViewModel`.

## Decisions

### Decision 1: Dual column binding to `Timestamp` in `HistoryWindow.xaml`
- **Choice:** Create two `DataGridTextColumn`s, each bound to `{Binding Timestamp}` with specific `StringFormat`:
  - `Tarix`: `Binding="{Binding Timestamp, StringFormat='{}{0:dd.MM.yyyy}'}"`, width 105.
  - `Saat`: `Binding="{Binding Timestamp, StringFormat='{}{0:HH:mm}'}"`, width 75.
- **Rationale:** Because both columns bind directly to the `DateTime Timestamp` property, WPF's DataGrid natively performs DateTime comparison on sort clicks. No custom sorting comparers or ViewModel changes are required.
- **Alternatives considered:**
  - *Calculated string properties in ViewModel*: Breaks native DateTime sorting without custom comparers.
  - *Multi-line wrapping in a single cell*: Explicitly rejected during exploration; single-line scanning provides superior readability.

### Decision 2: Clean date-only format in `MainWindow.xaml`
- **Choice:** Format `CreatedAt` as `dd.MM.yyyy` and adjust column width from 140 to 110.
- **Rationale:** Removes visual clutter from the main catalog table and gives more horizontal space for product name and action buttons.

## Risks / Trade-offs

- **[Horizontal space in `HistoryWindow`]** Adding an extra column increases date/time footprint from 130px to 180px (+50px) → **Mitigation:** The history window is 1100px wide and the `Qeyd` (Note) column has star (`*`) width, easily absorbing the 50px adjustment without horizontal scrollbars.
