using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockDesk.Data.Entities;

namespace StockDesk.Services;

public interface IInventoryService
{
    // Category management
    Task<List<Category>> GetCategoriesAsync();
    Task<Category> AddCategoryAsync(string name);
    Task UpdateCategoryAsync(int id, string name);
    Task DeleteCategoryAsync(int id);
    Task<bool> HasProductsInCategoryAsync(int categoryId);

    // Product management
    Task<List<Product>> GetProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> AddProductAsync(string name, int categoryId, int initialQuantity, string? imageFileName);
    Task UpdateProductAsync(int id, string name, int categoryId, string? imageFileName);
    Task DeleteProductAsync(int id);

    // Stock operations
    Task ReplenishStockAsync(int productId, int quantity, string? note = null);
    Task WriteOffStockAsync(int productId, int quantity, string recipientName, string? note = null);
    Task BulkWriteOffStockAsync(IEnumerable<(int ProductId, int Quantity)> items, string recipientName, string? note = null);

    // History operations
    Task<List<InventoryOperation>> GetHistoryAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        OperationType? type = null,
        string? recipientQuery = null
    );
}
