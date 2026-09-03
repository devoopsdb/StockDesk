## MODIFIED Requirements

### Requirement: Product Creation with Optional Photo
The system SHALL allow users to create products with a name, mandatory category selection, optional photo (JPG, PNG, WEBP), and initial stock quantity (defaulting to 1). The initial quantity input SHALL use a dedicated stepper control (`[ − ] [ value ] [ + ]`) without any clear button (`X`). The decrement button SHALL be disabled when the quantity reaches 0. When the user enters any positive number or zero for initial quantity and confirms, the system SHALL record the exact entered quantity.

#### Scenario: Product creation with photo
- **WHEN** user provides a product name, selects a category, chooses an image file (JPG/PNG/WEBP), specifies an initial quantity, and confirms
- **THEN** system copies the image to `%LocalAppData%/StockDesk/Images/` with a unique GUID filename, saves the product record with the exact specified initial balance, and records an initial inflow history entry

#### Scenario: Product creation without photo
- **WHEN** user creates a product without choosing an image
- **THEN** system saves the product with a null image path, the exact specified initial balance, and displays a neutral placeholder icon in the catalog

#### Scenario: Product creation with custom initial quantity
- **WHEN** user enters a custom quantity (e.g., 10) in the initial quantity field and clicks the save button without manually defocusing or pressing enter
- **THEN** system saves the product with initial balance equal to 10 and creates an inflow operation with quantity 10

#### Scenario: Stepper prevents negative initial balance
- **WHEN** initial quantity is 0
- **THEN** the decrement button (`−`) is disabled and cannot reduce the quantity below 0
