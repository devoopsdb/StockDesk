# app-branding Specification

## Purpose
Provides official visual branding and desktop application icon assets for StockDesk across Windows taskbar, window title bars, desktop shortcuts, and installer packages.

## Requirements

### Requirement: Application Icon Assets
The application bundle SHALL include multi-resolution icon assets (`AppIcon.ico` and `AppIcon.png`) representing the StockDesk Fluent Glass Parcel identity in resolutions from 16x16 up to 256x256 pixels with full alpha transparency.

#### Scenario: Multi-resolution icon availability
- **WHEN** the application is compiled and packaged
- **THEN** `AppIcon.ico` contains distinct layers for 16x16, 24x24, 32x32, 48x48, 64x64, 128x128, and 256x256 resolutions without background artifacts

### Requirement: Window Title Bar Branding
The main window of the application SHALL display the official application icon in the top-left corner of the title bar.

#### Scenario: Main window displays icon
- **WHEN** the user launches the StockDesk desktop application
- **THEN** the main window title bar displays the crisp 16x16 application icon adjacent to the window title

### Requirement: Binary and Taskbar Icon Integration
The compiled Windows executable (`StockDesk.exe`) SHALL embed `AppIcon.ico` as its primary `ApplicationIcon` metadata.

#### Scenario: Taskbar and Explorer display
- **WHEN** the application is running or viewed in Windows File Explorer
- **THEN** Windows Taskbar and File Explorer render the official StockDesk 3D isometric cube icon

### Requirement: Installer and Shortcut Branding
Velopack packaging and desktop shortcut generation SHALL use the official application icon.

#### Scenario: Desktop shortcut creation
- **WHEN** the application is installed or updated via `StockDesk-Setup.exe`
- **THEN** the created desktop shortcut and Start Menu entry use `AppIcon.ico`
