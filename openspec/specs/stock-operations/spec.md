## Purpose

Handles inventory balance adjustments including restocking (Mədaxil) and atomic single/bulk write-offs (Məxaric) with strict non-negative balance guarantees.

## Requirements

### Requirement: Stock Inflow (Mədaxil)
The system SHALL support adding inventory to products and automatically creating an inflow operation record. The system SHALL capture the exact quantity entered in the replenishment dialog, regardless of whether the user types the value or uses spin buttons.

#### Scenario: Restocking existing product
- **WHEN** user opens the inflow modal for a product, enters a positive quantity (e.g. 5, 20), and confirms
- **THEN** system increases the product balance by the exact specified quantity and creates a green-flagged "Mədaxil" entry in the operations history

#### Scenario: Restocking without manual defocus
- **WHEN** user enters a custom quantity in the replenish input and clicks the confirm button directly
- **THEN** system commits the entered quantity, increments the stock balance by that quantity, and logs the operation

### Requirement: Single Product Write-off (Məxaric)
The system SHALL allow writing off a quantity of a product to a designated recipient, ensuring the written-off quantity does not exceed the available balance. The system SHALL capture the exact quantity entered in the write-off modal.

#### Scenario: Valid single write-off
- **WHEN** user specifies a write-off quantity less than or equal to current balance, selects or enters a recipient, and confirms
- **THEN** system decrements the product balance by the exact specified quantity, records a red-flagged "Məxaric" entry in history, and closes the modal

#### Scenario: Write-off exceeding available stock
- **WHEN** user attempts to enter a write-off quantity greater than the product's current balance
- **THEN** system blocks confirmation, highlights the field, and displays a warning stating the maximum available quantity

### Requirement: Bulk Product Write-off (Qrup Məxaric)
The system SHALL allow writing off multiple selected products to a single recipient within an atomic database transaction.

#### Scenario: Successful bulk write-off
- **WHEN** user initiates bulk write-off for multiple items, enters valid quantities for each item, provides a recipient, and confirms
- **THEN** system atomically decrements balances for all items, logs a "Məxaric" record for each item, and clears the selection

#### Scenario: Bulk write-off rollback on error
- **WHEN** any error or validation failure occurs during bulk processing
- **THEN** system rolls back all balance modifications and logs no partial history records
