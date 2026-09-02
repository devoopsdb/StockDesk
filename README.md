# StockDesk 📦

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF%20%2B%20WPF--UI-0078D4?logo=windows&logoColor=white)](https://github.com/lepoco/wpfui)
[![SQLite](https://img.shields.io/badge/Database-SQLite%20(WAL)-003B57?logo=sqlite&logoColor=white)](https://sqlite.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**StockDesk** is a modern, offline Windows desktop application designed for fast, visual, and reliable warehouse and stock inventory management. Built with **C# .NET 10**, **WPF**, **WPF-UI (Fluent 2 / Windows 11 Design)**, and **SQLite (EF Core)**, with full localization in Azerbaijani (`az-Latn-AZ`).

---

## ✨ Features

- **🖼️ Visual Product Catalog:**
  - High-performance product catalog with thumbnail image previews (supporting **JPG, PNG, and WEBP** via SkiaSharp).
  - Non-file-locking image memory caching (safe file handling without Windows file locks).
  - Fallback placeholder icons for products without images.
  - Live, debounced search by product name or category.
  - Category dropdown filtering and multi-criteria sorting (A-Z, Z-A, stock level, creation date).

- **📥 Inflow & Restock Operations (Mədaxil):**
  - Automatic initial stock registration during product creation.
  - Fast restock/replenishment dialog with optional notes.

- **📤 Single & Bulk Write-offs / Issuance (Məxaric):**
  - Instant single-item write-off and multi-selection checkbox bar for batch write-offs.
  - **Atomic transactions**: Prevents partial failures and guarantees database consistency.
  - **Zero-negative balance protection**: Strict checks prevent stock from ever falling below zero.

- **👤 Smart Recipient Directory:**
  - Dynamic recipient (person or department) suggestions with inline autocomplete.
  - Seamless auto-persistence: New names typed into the write-off combobox are automatically saved to the database.

- **📜 Immutable Audit Log & History (Əməliyyat Jurnalı):**
  - Append-only, tamper-proof history tracking every inflow and outflow.
  - Historical snapshots of product names, categories, and recipient names preserved even if items are modified.
  - Color-coded operation badges (🟢 Inflow / 🔴 Outflow).
  - Multi-parameter filtering by operation type, date ranges, and recipient queries.

- **🗂️ Safe Category Management:**
  - Unique category name validation (culture-aware for Azerbaijani Latin characters).
  - Referential integrity protection: Blocks deletion of categories that contain active products.

- **🌐 100% Azerbaijani Localization:**
  - Native UI strings, messages, labels, and validation in Azerbaijani (`az-Latn-AZ`).

---

## 🏗️ Architecture & Tech Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Platform** | .NET 10 (`net10.0-windows`) | Core framework and runtime |
| **UI Framework** | WPF + [WPF-UI 4.x](https://github.com/lepoco/wpfui) | Fluent 2 design language, Windows 11 controls and styles |
| **MVVM Toolkit** | [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) | High-performance source generators (`[ObservableProperty]`, `[RelayCommand]`) |
| **Database** | SQLite + EF Core 10 | Local relational database with Write-Ahead Logging (`WAL`) mode |
| **Image Processing** | [SkiaSharp](https://github.com/mono/SkiaSharp) | Decoding, resizing, and normalization for JPG, PNG, and WEBP |
| **Dependency Injection** | `Microsoft.Extensions.Hosting` | Structured application lifecycle and DI container |

---

## 📁 Storage Structure

All application data is isolated within the user's local application data folder:

```
%LocalAppData%/StockDesk/
├── stockdesk.db           # SQLite database file (WAL mode enabled)
└── Images/                # Standardized image thumbnails
    ├── 4a8f9c12-...png    # Uniquely identified GUID image files
    └── e7b21a09-...png
```

---

## 🚀 Getting Started

### Prerequisites
- Windows 10 / 11 (64-bit)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or runtime

### Building from Source

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-username/StockDesk.git
   cd StockDesk
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build -c Release
   ```

4. **Run tests:**
   ```bash
   dotnet test
   ```

5. **Run the application:**
   ```bash
   dotnet run --project src/StockDesk/StockDesk.csproj
   ```

---

## 🧪 Testing

The solution includes comprehensive unit and integration tests covering business rules, atomic stock operations, negative balance constraints, recipient persistence, and image storage using an in-memory SQLite database:

```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
