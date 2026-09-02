using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockDesk.Services;
using StockDesk.ViewModels;
using Xunit;

namespace StockDesk.Tests;

public class MockDialogService : IDialogService
{
    public Task<bool> ShowCategoryDialogAsync(int? categoryId = null) => Task.FromResult(true);
    public Task<bool> ShowProductDialogAsync(int? productId = null) => Task.FromResult(true);
    public Task<bool> ShowWriteOffDialogAsync(IEnumerable<ProductItemViewModel> products) => Task.FromResult(true);
    public Task<bool> ShowReplenishDialogAsync(ProductItemViewModel product) => Task.FromResult(true);
    public void ShowHistoryWindow() { }
    public void ShowMessage(string title, string message, bool isError = false) { }
    public bool ShowConfirmation(string title, string message) => true;
    public string? OpenImageFileDialog() => null;
}

public class MainViewModelTests
{
    [Fact]
    public async Task FilteringAndSorting_WorksCorrectly()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);
            var dialogService = new MockDialogService();

            var cat1 = await inventoryService.AddCategoryAsync("Noutbuklar");
            var cat2 = await inventoryService.AddCategoryAsync("Aksessuarlar");

            await inventoryService.AddProductAsync("Dell Vostro", cat1.Id, 10, null);
            await inventoryService.AddProductAsync("HP Pavilion", cat1.Id, 5, null);
            await inventoryService.AddProductAsync("Logitech Mouse", cat2.Id, 20, null);

            var vm = new MainViewModel(inventoryService, imageStorage, dialogService);
            await vm.InitializeAsync();

            Assert.Equal(3, vm.FilteredProducts.Count);
            Assert.Equal(35, vm.TotalStockSum);

            // Filter by search text
            vm.SearchText = "Mouse";
            Assert.Single(vm.FilteredProducts);
            Assert.Equal("Logitech Mouse", vm.FilteredProducts[0].Name);

            // Reset search and filter by category "Noutbuklar"
            vm.SearchText = "";
            vm.SelectedCategoryFilter = vm.CategoryFilters.First(c => c.Name == "Noutbuklar");
            Assert.Equal(2, vm.FilteredProducts.Count);

            // Multi-selection
            vm.FilteredProducts[0].IsSelected = true;
            vm.FilteredProducts[1].IsSelected = true;
            Assert.Equal(2, vm.SelectedCount);
            Assert.True(vm.IsBulkActionBarVisible);
        }
    }
}
