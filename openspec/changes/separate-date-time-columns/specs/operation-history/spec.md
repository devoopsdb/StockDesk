## MODIFIED Requirements

### Requirement: History Journal View and Filtering
The system SHALL display operations in descending chronological order with color-coded operation badges, separate dedicated single-line columns for calendar date and time of day, and support multi-field filtering.

#### Scenario: Separate date and time columns
- **WHEN** the history journal table is displayed
- **THEN** operation records display the date in a dedicated "Tarix" column (`dd.MM.yyyy`) and time in a dedicated "Saat" column (`HH:mm`) on the same row

#### Scenario: Color-coded type display
- **WHEN** the history journal is opened
- **THEN** inflow ("Mədaxil") records are marked with green indicators and outflow ("Məxaric") records with red indicators

#### Scenario: Filtering history
- **WHEN** user filters by operation type (Mədaxil/Məxaric), date range, or recipient name
- **THEN** the journal table updates to show only matching historical records
