## MODIFIED Requirements

### Requirement: Multi-Item Selection
The system SHALL allow users to select multiple products via checkboxes to activate bulk operations, ensuring checkboxes are clearly visible and unobstructed in all row states.

#### Scenario: Selecting two or more items
- **WHEN** user checks the checkbox for two or more product rows
- **THEN** system shows the bottom bulk action bar with the count of selected items and enables bulk write-off

#### Scenario: Unobstructed checkbox display
- **WHEN** product catalog rows are rendered in unselected or selected state
- **THEN** each selection checkbox SHALL be fully visible with complete borders and centered checkmark without clipping or overlap from adjacent elements
