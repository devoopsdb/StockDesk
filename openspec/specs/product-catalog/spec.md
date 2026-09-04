## Purpose

Provides a visual, searchable, and filterable product catalog with image previews, stock balance tracking, and multi-item selection for batch operations.

## Requirements

### Requirement: Product Creation with Optional Photo
The system SHALL allow users to create products with a name, mandatory category selection, optional photo (JPG, PNG, WEBP), and initial stock quantity (defaulting to 1). The initial quantity input SHALL use a dedicated stepper control (`[ − ] [ value ] [ + ]`) without any clear button (`X`). The decrement button SHALL be disabled when the quantity reaches 0. When the user enters any positive number or zero for initial quantity and confirms, the system SHALL record the exact entered quantity. In the catalog table, products without an image SHALL visibly display a neutral placeholder icon.

#### Scenario: Product creation with photo
- **WHEN** user provides a product name, selects a category, chooses an image file (JPG/PNG/WEBP), specifies an initial quantity, and confirms
- **THEN** system copies the image to `%LocalAppData%/StockDesk/Images/` with a unique GUID filename, saves the product record with the exact specified initial balance, and records an initial inflow history entry

#### Scenario: Product creation without photo
- **WHEN** user creates a product without choosing an image
- **THEN** system saves the product with a null image path, the exact specified initial balance, and displays a neutral placeholder icon visibly in the catalog table

#### Scenario: Product creation with custom initial quantity
- **WHEN** user enters a custom quantity (e.g., 10) in the initial quantity field and clicks the save button without manually defocusing or pressing enter
- **THEN** system saves the product with initial balance equal to 10 and creates an inflow operation with quantity 10

#### Scenario: Stepper prevents negative initial balance
- **WHEN** initial quantity is 0
- **THEN** the decrement button (`−`) is disabled and cannot reduce the quantity below 0

### Requirement: Catalog Empty State Display
The system SHALL display an empty state placeholder ("Heç bir məhsul tapılmadı" and "+ İlk məhsulu əlavə et" action button) only when the catalog has zero products matching the active filter criteria. When one or more products are present, the empty state placeholder SHALL be collapsed and the product table SHALL be displayed without obstruction.

#### Scenario: Empty catalog display
- **WHEN** the catalog contains zero products or active search/category filters yield no results
- **THEN** the empty state placeholder with box icon, message, and add button is visible

#### Scenario: Non-empty catalog display
- **WHEN** one or more products match the active filters
- **THEN** the empty state placeholder is collapsed and hidden from the user interface

### Requirement: Live Search and Filtering
The system SHALL provide instant debounced filtering of products by name and category without blocking the user interface.

#### Scenario: Filtering by text
- **WHEN** user types text into the search input box
- **THEN** the product list instantly updates to show only items whose names contain the search substring (case-insensitive)

#### Scenario: Filtering by category
- **WHEN** user selects a specific category from the category dropdown
- **THEN** the product list displays only products belonging to that category

#### Scenario: All categories selected
- **WHEN** user selects "Bütün kateqoriyalar"
- **THEN** the product list displays products across all categories

### Requirement: Product Sorting
The system SHALL allow sorting the product list by name (A-Z, Z-A), current balance (ascending, descending), and creation date.

#### Scenario: Sorting by balance
- **WHEN** user selects sorting by balance (ascending)
- **THEN** the product list orders items with the smallest stock quantity first

### Requirement: Multi-Item Selection
The system SHALL allow users to select multiple products via checkboxes to activate bulk operations, ensuring checkboxes are clearly visible and unobstructed in all row states.

#### Scenario: Selecting two or more items
- **WHEN** user checks the checkbox for two or more product rows
- **THEN** system shows the bottom bulk action bar with the count of selected items and enables bulk write-off

#### Scenario: Unobstructed checkbox display
- **WHEN** product catalog rows are rendered in unselected or selected state
- **THEN** each selection checkbox SHALL be fully visible with complete borders and centered checkmark without clipping or overlap from adjacent elements

### Requirement: Product Catalog Date Display
The product catalog table SHALL display the product creation date as a calendar date in `dd.MM.yyyy` format without any time component.

#### Scenario: Displaying product creation date
- **WHEN** product items are displayed in the catalog table
- **THEN** the addition date column ("Əlavə tarixi") renders only the calendar date formatted as `dd.MM.yyyy`
