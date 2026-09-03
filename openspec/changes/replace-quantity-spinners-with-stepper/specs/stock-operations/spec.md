## MODIFIED Requirements

### Requirement: Stock Inflow (Mədaxil)
The system SHALL support adding inventory to products and automatically creating an inflow operation record. The system SHALL provide a dedicated quantity stepper control (`[ − ] [ value ] [ + ]`) for replenish quantity input without any clear button (`X`). The system SHALL capture the exact quantity entered in the replenishment dialog, whether the user types the value directly or uses the stepper increment/decrement buttons. The decrement button SHALL be disabled when the quantity is at the minimum value of 1.

#### Scenario: Restocking existing product
- **WHEN** user opens the inflow modal for a product, enters a positive quantity (e.g. 5, 20), and confirms
- **THEN** system increases the product balance by the exact specified quantity and creates a green-flagged "Mədaxil" entry in the operations history

#### Scenario: Restocking without manual defocus
- **WHEN** user enters a custom quantity in the replenish input and clicks the confirm button directly
- **THEN** system commits the entered quantity, increments the stock balance by that quantity, and logs the operation

#### Scenario: Decrementing below minimum blocked
- **WHEN** replenish quantity is at 1
- **THEN** the decrement button (`−`) is disabled and cannot reduce the quantity below 1

### Requirement: Single Product Write-off (Məxaric)
The system SHALL allow writing off a quantity of a product to a designated recipient, ensuring the written-off quantity does not exceed the available balance. The system SHALL provide a dedicated quantity stepper control (`[ − ] [ value ] [ + ]`) for write-off quantity input without any clear button (`X`). The decrement button SHALL be disabled at the minimum quantity of 1, and the increment button SHALL be disabled when the entered quantity reaches the product's current balance.

#### Scenario: Valid single write-off
- **WHEN** user specifies a write-off quantity less than or equal to current balance, selects or enters a recipient, and confirms
- **THEN** system decrements the product balance by the exact specified quantity, records a red-flagged "Məxaric" entry in history, and closes the modal

#### Scenario: Write-off exceeding available stock
- **WHEN** user attempts to enter a write-off quantity greater than the product's current balance
- **THEN** system blocks confirmation, highlights the field, and displays a warning stating the maximum available quantity

#### Scenario: Stepper boundaries enforce available balance
- **WHEN** write-off quantity reaches the product's current balance
- **THEN** the increment button (`+`) is disabled and prevents incrementing beyond the available balance
