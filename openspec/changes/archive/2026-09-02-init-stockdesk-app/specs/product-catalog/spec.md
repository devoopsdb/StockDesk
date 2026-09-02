## Purpose

Provides a visual, searchable, and filterable product catalog with image previews, stock balance tracking, and multi-item selection for batch operations.

## ADDED Requirements

### Requirement: Product Creation with Optional Photo
The system SHALL allow users to create products with a name, mandatory category selection, optional photo (JPG, PNG, WEBP), and initial stock quantity (defaulting to 1).

#### Scenario: Product creation with photo
- **WHEN** user provides a product name, selects a category, chooses an image file (JPG/PNG/WEBP), specifies an initial quantity, and confirms
- **THEN** system copies the image to `%LocalAppData%/StockDesk/Images/` with a unique GUID filename, saves the product record, and records an initial inflow history entry

#### Scenario: Product creation without photo
- **WHEN** user creates a product without choosing an image
- **THEN** system saves the product with a null image path and displays a neutral placeholder icon in the catalog

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
The system SHALL allow users to select multiple products via checkboxes to activate bulk operations.

#### Scenario: Selecting two or more items
- **WHEN** user checks the checkbox for two or more product rows
- **THEN** system shows the bottom bulk action bar with the count of selected items and enables bulk write-off
