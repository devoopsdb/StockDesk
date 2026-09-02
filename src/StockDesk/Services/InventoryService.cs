using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StockDesk.Data;
using StockDesk.Data.Entities;

namespace StockDesk.Services;

public class InventoryService : IInventoryService
{
    private readonly StockDbContext _dbContext;
    private readonly IRecipientService _recipientService;
    private readonly IImageStorageService _imageStorageService;

    public InventoryService(
        StockDbContext dbContext,
        IRecipientService recipientService,
        IImageStorageService imageStorageService)
    {
        _dbContext = dbContext;
        _recipientService = recipientService;
        _imageStorageService = imageStorageService;
    }

    // --- Category Management ---

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category> AddCategoryAsync(string name)
    {
        string trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("Kateqoriya adı boş ola bilməz.", nameof(name));
        }

        var allCategories = await _dbContext.Categories.ToListAsync();
        bool exists = allCategories.Any(c => string.Equals(c.Name, trimmed, StringComparison.CurrentCultureIgnoreCase));

        if (exists)
        {
            throw new InvalidOperationException($"'{trimmed}' adlı kateqoriya artıq mövcuddur.");
        }

        var category = new Category
        {
            Name = trimmed,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(int id, string name)
    {
        string trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("Kateqoriya adı boş ola bilməz.", nameof(name));
        }

        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null)
        {
            throw new InvalidOperationException("Kateqoriya tapılmadı.");
        }

        var allCategories = await _dbContext.Categories.ToListAsync();
        bool exists = allCategories.Any(c => c.Id != id && string.Equals(c.Name, trimmed, StringComparison.CurrentCultureIgnoreCase));

        if (exists)
        {
            throw new InvalidOperationException($"'{trimmed}' adlı kateqoriya artıq mövcuddur.");
        }

        category.Name = trimmed;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return;
        }

        if (category.Products.Any())
        {
            throw new InvalidOperationException(
                $"Bu kateqoriyada {category.Products.Count} məhsul var. Kateqoriyanı silmək üçün əvvəlcə ona aid məhsulları silməli və ya başqa kateqoriyaya keçirməlisiniz."
            );
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> HasProductsInCategoryAsync(int categoryId)
    {
        return await _dbContext.Products.AnyAsync(p => p.CategoryId == categoryId);
    }

    // --- Product Management ---

    public async Task<List<Product>> GetProductsAsync()
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> AddProductAsync(string name, int categoryId, int initialQuantity, string? imageFileName)
    {
        string trimmedName = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new ArgumentException("Məhsulun adı boş ola bilməz.", nameof(name));
        }

        if (initialQuantity < 0)
        {
            throw new ArgumentException("İlkin say mənfi ola bilməz.", nameof(initialQuantity));
        }

        var category = await _dbContext.Categories.FindAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException("Seçilmiş kateqoriya tapılmadı.");
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var product = new Product
        {
            Name = trimmedName,
            CategoryId = categoryId,
            CurrentBalance = initialQuantity,
            ImageFileName = imageFileName,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        if (initialQuantity > 0)
        {
            var initialOperation = new InventoryOperation
            {
                Timestamp = DateTime.UtcNow,
                OperationType = OperationType.Inflow,
                ProductId = product.Id,
                ProductNameSnapshot = product.Name,
                CategoryNameSnapshot = category.Name,
                QuantityDelta = initialQuantity,
                Note = "İlkin qalıq daxiletməsi"
            };

            _dbContext.InventoryOperations.Add(initialOperation);
            await _dbContext.SaveChangesAsync();
        }

        await transaction.CommitAsync();
        return product;
    }

    public async Task UpdateProductAsync(int id, string name, int categoryId, string? imageFileName)
    {
        string trimmedName = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new ArgumentException("Məhsulun adı boş ola bilməz.", nameof(name));
        }

        var product = await _dbContext.Products.FindAsync(id);
        if (product == null)
        {
            throw new InvalidOperationException("Məhsul tapılmadı.");
        }

        var category = await _dbContext.Categories.FindAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException("Seçilmiş kateqoriya tapılmadı.");
        }

        // Delete old image file if changed
        if (product.ImageFileName != null && product.ImageFileName != imageFileName)
        {
            _imageStorageService.DeleteImage(product.ImageFileName);
        }

        product.Name = trimmedName;
        product.CategoryId = categoryId;
        product.ImageFileName = imageFileName;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _dbContext.Products.FindAsync(id);
        if (product == null)
        {
            return;
        }

        if (product.ImageFileName != null)
        {
            _imageStorageService.DeleteImage(product.ImageFileName);
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
    }

    // --- Stock Operations ---

    public async Task ReplenishStockAsync(int productId, int quantity, string? note = null)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Artırılacaq miqdar 0-dan böyük olmalıdır.", nameof(quantity));
        }

        var product = await _dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            throw new InvalidOperationException("Məhsul tapılmadı.");
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        product.CurrentBalance += quantity;

        var operation = new InventoryOperation
        {
            Timestamp = DateTime.UtcNow,
            OperationType = OperationType.Inflow,
            ProductId = product.Id,
            ProductNameSnapshot = product.Name,
            CategoryNameSnapshot = product.Category.Name,
            QuantityDelta = quantity,
            Note = string.IsNullOrWhiteSpace(note) ? "Mədaxil (Qalıq artımı)" : note.Trim()
        };

        _dbContext.InventoryOperations.Add(operation);
        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task WriteOffStockAsync(int productId, int quantity, string recipientName, string? note = null)
    {
        await BulkWriteOffStockAsync(new[] { (productId, quantity) }, recipientName, note);
    }

    public async Task BulkWriteOffStockAsync(IEnumerable<(int ProductId, int Quantity)> items, string recipientName, string? note = null)
    {
        var itemList = items.ToList();
        if (!itemList.Any())
        {
            throw new ArgumentException("Heç bir məhsul seçilməyib.", nameof(items));
        }

        string trimmedRecipient = recipientName.Trim();
        if (string.IsNullOrWhiteSpace(trimmedRecipient))
        {
            throw new ArgumentException("Təhvil alan şəxs/şöbə mütləq göstərilməlidir.", nameof(recipientName));
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var recipient = await _recipientService.GetOrCreateRecipientAsync(trimmedRecipient);

        var productIds = itemList.Select(i => i.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Include(p => p.Category)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var (productId, quantity) in itemList)
        {
            if (quantity <= 0)
            {
                throw new InvalidOperationException("Silinəcək say 0-dan böyük olmalıdır.");
            }

            if (!products.TryGetValue(productId, out var product))
            {
                throw new InvalidOperationException($"Məhsul (ID: {productId}) tapılmadı.");
            }

            if (product.CurrentBalance < quantity)
            {
                throw new InvalidOperationException(
                    $"'{product.Name}' üçün kifayət qədər qalıq yoxdur. Mövcud qalıq: {product.CurrentBalance} ədəd, tələb olunan: {quantity} ədəd."
                );
            }

            product.CurrentBalance -= quantity;

            var operation = new InventoryOperation
            {
                Timestamp = DateTime.UtcNow,
                OperationType = OperationType.Outflow,
                ProductId = product.Id,
                ProductNameSnapshot = product.Name,
                CategoryNameSnapshot = product.Category.Name,
                QuantityDelta = -quantity,
                RecipientId = recipient.Id,
                RecipientNameSnapshot = recipient.Name,
                Note = string.IsNullOrWhiteSpace(note) ? "Məxaric (Təhvil)" : note.Trim()
            };

            _dbContext.InventoryOperations.Add(operation);
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    // --- History Operations ---

    public async Task<List<InventoryOperation>> GetHistoryAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        OperationType? type = null,
        string? recipientQuery = null)
    {
        var query = _dbContext.InventoryOperations
            .Include(o => o.Recipient)
            .AsNoTracking();

        if (fromDate.HasValue)
        {
            var startUtc = fromDate.Value.Date.ToUniversalTime();
            query = query.Where(o => o.Timestamp >= startUtc);
        }

        if (toDate.HasValue)
        {
            var endUtc = toDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(o => o.Timestamp <= endUtc);
        }

        if (type.HasValue)
        {
            query = query.Where(o => o.OperationType == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(recipientQuery))
        {
            string search = recipientQuery.Trim().ToLower();
            query = query.Where(o => o.RecipientNameSnapshot != null && o.RecipientNameSnapshot.ToLower().Contains(search));
        }

        return await query
            .OrderByDescending(o => o.Timestamp)
            .ToListAsync();
    }
}
