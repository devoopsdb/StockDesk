## Purpose

Manages the catalog of product categories, ensuring category names are unique and maintaining referential integrity across the inventory system.

## ADDED Requirements

### Requirement: Category Creation
The system SHALL allow users to create new categories with a non-empty, unique name in the Azerbaijani user interface.

#### Scenario: Successful category creation
- **WHEN** user enters a new unique category name and clicks "Yadda saxla"
- **THEN** system saves the category to the database and refreshes the category lists

#### Scenario: Duplicate category name
- **WHEN** user attempts to create a category with a name that already exists (case-insensitive)
- **THEN** system rejects the creation and displays a validation error message in Azerbaijani

#### Scenario: Empty category name
- **WHEN** user submits an empty or whitespace-only category name
- **THEN** system disables saving or displays a validation error requiring a name

### Requirement: Category Deletion Protection
The system SHALL prevent deletion of categories that currently contain associated products.

#### Scenario: Deleting category without products
- **WHEN** user deletes a category that has 0 associated products
- **THEN** system removes the category from the database

#### Scenario: Deleting category with active products
- **WHEN** user attempts to delete a category that has 1 or more associated products
- **THEN** system blocks deletion and displays an explanatory warning message in Azerbaijani
