## Purpose

Maintains a dictionary of inventory recipients (individuals and departments), providing real-time autocomplete suggestions and automatic persistence.

## Requirements

### Requirement: Dynamic Recipient Autocomplete and Auto-save
The system SHALL provide an editable dropdown for selecting or typing recipient names during write-off, automatically adding new names to the dictionary upon operation confirmation.

#### Scenario: Selecting existing recipient
- **WHEN** user types or chooses a recipient from the dropdown suggestions
- **THEN** system associates the operation with the existing recipient entity

#### Scenario: Auto-saving new recipient
- **WHEN** user enters a previously unseen recipient name and successfully confirms a write-off
- **THEN** system saves the new recipient to the database so it appears in future suggestions
