using System;
using System.Linq;
using System.Threading.Tasks;
using StockDesk.Data.Entities;
using StockDesk.Services;
using Xunit;

namespace StockDesk.Tests;

public class InventoryServiceTests
{
    [Fact]
    public async Task AddProduct_WithInitialQuantity_CreatesProductAndInflowOperation()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);

            var category = await inventoryService.AddCategoryAsync("Noutbuklar");
            var product = await inventoryService.AddProductAsync("ThinkPad X1", category.Id, 10, null);

            Assert.True(product.Id > 0);
            Assert.Equal(10, product.CurrentBalance);

            var history = await inventoryService.GetHistoryAsync();
            Assert.Single(history);
            Assert.Equal(OperationType.Inflow, history[0].OperationType);
            Assert.Equal(10, history[0].QuantityDelta);
            Assert.Equal("ThinkPad X1", history[0].ProductNameSnapshot);
            Assert.Equal("Noutbuklar", history[0].CategoryNameSnapshot);
        }
    }

    [Fact]
    public async Task ReplenishStock_IncreasesBalanceAndLogsInflow()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);

            var category = await inventoryService.AddCategoryAsync("Aksessuarlar");
            var product = await inventoryService.AddProductAsync("Siçan MX Master", category.Id, 5, null);

            await inventoryService.ReplenishStockAsync(product.Id, 15, "Yeni partiya");

            var updatedProduct = await inventoryService.GetProductByIdAsync(product.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal(20, updatedProduct.CurrentBalance);

            var history = await inventoryService.GetHistoryAsync();
            Assert.Equal(2, history.Count);
            Assert.Equal(15, history[0].QuantityDelta);
            Assert.Equal("Yeni partiya", history[0].Note);
        }
    }

    [Fact]
    public async Task SingleWriteOff_ValidStock_DecrementsBalanceAndLogsOutflow()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);

            var category = await inventoryService.AddCategoryAsync("Kabellər");
            var product = await inventoryService.AddProductAsync("HDMI Kabel", category.Id, 25, null);

            await inventoryService.WriteOffStockAsync(product.Id, 5, "Maliyyə Şöbəsi", "Təqdimat üçün");

            var updatedProduct = await inventoryService.GetProductByIdAsync(product.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal(20, updatedProduct.CurrentBalance);

            var history = await inventoryService.GetHistoryAsync();
            Assert.Equal(2, history.Count);

            var latest = history[0];
            Assert.Equal(OperationType.Outflow, latest.OperationType);
            Assert.Equal(-5, latest.QuantityDelta);
            Assert.Equal("Maliyyə Şöbəsi", latest.RecipientNameSnapshot);
            Assert.Equal("Təqdimat üçün", latest.Note);
        }
    }

    [Fact]
    public async Task WriteOff_ExceedingAvailableStock_ThrowsExceptionAndRollsBack()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);

            var category = await inventoryService.AddCategoryAsync("Printerlər");
            var product = await inventoryService.AddProductAsync("HP LaserJet", category.Id, 2, null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                inventoryService.WriteOffStockAsync(product.Id, 5, "Anbar")
            );

            Assert.Contains("kifayət qədər qalıq yoxdur", ex.Message);

            // Verify balance wasn't changed
            var checkProduct = await inventoryService.GetProductByIdAsync(product.Id);
            Assert.NotNull(checkProduct);
            Assert.Equal(2, checkProduct.CurrentBalance);
        }
    }

    [Fact]
    public async Task BulkWriteOff_AtomicExecution_Succeeds()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);

            var category = await inventoryService.AddCategoryAsync("Avadanlıq");
            var p1 = await inventoryService.AddProductAsync("Monitor 27", category.Id, 10, null);
            var p2 = await inventoryService.AddProductAsync("Klaviatura", category.Id, 15, null);

            await inventoryService.BulkWriteOffStockAsync(
                new[] { (p1.Id, 3), (p2.Id, 7) },
                "Yeni Əməkdaş",
                "İş yeri qurulumu"
            );

            var checkP1 = await inventoryService.GetProductByIdAsync(p1.Id);
            var checkP2 = await inventoryService.GetProductByIdAsync(p2.Id);

            Assert.Equal(7, checkP1!.CurrentBalance);
            Assert.Equal(8, checkP2!.CurrentBalance);

            var history = await inventoryService.GetHistoryAsync();
            var outflows = history.Where(h => h.OperationType == OperationType.Outflow).ToList();
            Assert.Equal(2, outflows.Count);
            Assert.All(outflows, o => Assert.Equal("Yeni Əməkdaş", o.RecipientNameSnapshot));
        }
    }
}
