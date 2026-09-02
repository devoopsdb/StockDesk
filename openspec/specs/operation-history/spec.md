## Purpose

Provides an immutable audit log of all inventory movements (inflows and outflows) with visual status indicators, product/recipient snapshots, and multi-field filtering.

## Requirements

### Requirement: Immutable Audit Logging
The system SHALL create timestamped, non-editable audit records for every stock change including operation type, item name, category name snapshot, quantity delta, and recipient name snapshot.

#### Scenario: Operation logging on stock change
- **WHEN** any stock inflow or outflow completes
- **THEN** system immediately generates an immutable record with current system timestamp and captured snapshot values

### Requirement: History Journal View and Filtering
The system SHALL display operations in descending chronological order with color-coded operation badges and support multi-field filtering.

#### Scenario: Color-coded type display
- **WHEN** the history journal is opened
- **THEN** inflow ("Mədaxil") records are marked with green indicators and outflow ("Məxaric") records with red indicators

#### Scenario: Filtering history
- **WHEN** user filters by operation type (Mədaxil/Məxaric), date range, or recipient name
- **THEN** the journal table updates to show only matching historical records
