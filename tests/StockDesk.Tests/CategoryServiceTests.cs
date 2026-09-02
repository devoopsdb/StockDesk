using System;
using System.Threading.Tasks;
using StockDesk.Services;
using Xunit;

namespace StockDesk.Tests;

public class CategoryServiceTests
{
    [Fact]
    public async Task AddCategory_ValidName_SavesSuccessfully()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);

            var category = await inventoryService.AddCategoryAsync("Noutbuklar");

            Assert.True(category.Id > 0);
            Assert.Equal("Noutbuklar", category.Name);

            var list = await inventoryService.GetCategoriesAsync();
            Assert.Single(list);
        }
    }

    [Fact]
    public async Task AddCategory_DuplicateName_ThrowsException()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);

            await inventoryService.AddCategoryAsync("Noutbuklar");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                inventoryService.AddCategoryAsync("noutbuklar") // case-insensitive duplicate
            );

            Assert.Contains("artıq mövcuddur", ex.Message);
        }
    }

    [Fact]
    public async Task DeleteCategory_WithAssociatedProducts_ThrowsException()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);

            var category = await inventoryService.AddCategoryAsync("Noutbuklar");
            await inventoryService.AddProductAsync("Dell Vostro", category.Id, 5, null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                inventoryService.DeleteCategoryAsync(category.Id)
            );

            Assert.Contains("məhsul var", ex.Message);
        }
    }
}
