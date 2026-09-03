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

public class MockUpdateService : IUpdateService
{
    public string CurrentVersion { get; set; } = "1.0.3";
    public bool IsInstalled => false;
    public Task<UpdateCheckResult> CheckForUpdatesAsync(System.Threading.CancellationToken cancellationToken = default)
        => Task.FromResult(new UpdateCheckResult(false, null, false));
    public Task<bool> DownloadUpdatesAsync(System.Threading.CancellationToken cancellationToken = default)
        => Task.FromResult(false);
    public void ApplyUpdatesAndRestart() { }
}

public class MainViewModelTests
{
    [Fact]
    public void WindowTitle_IncludesCurrentAppVersion()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);
            var dialogService = new MockDialogService();
            var updateService = new MockUpdateService { CurrentVersion = "1.0.3" };

            var vm = new MainViewModel(inventoryService, imageStorage, dialogService, updateService);

            Assert.Equal("StockDesk v1.0.3 - Operativ Anbar Uçotu", vm.WindowTitle);
        }
    }

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
            var updateService = new MockUpdateService();

            var cat1 = await inventoryService.AddCategoryAsync("Noutbuklar");
            var cat2 = await inventoryService.AddCategoryAsync("Aksessuarlar");

            await inventoryService.AddProductAsync("Dell Vostro", cat1.Id, 10, null);
            await inventoryService.AddProductAsync("HP Pavilion", cat1.Id, 5, null);
            await inventoryService.AddProductAsync("Logitech Mouse", cat2.Id, 20, null);

            var vm = new MainViewModel(inventoryService, imageStorage, dialogService, updateService);
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

    [Fact]
    public async Task IsEmptyStateVisible_UpdatesCorrectlyBasedOnFilteredProducts()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);
            var dialogService = new MockDialogService();
            var updateService = new MockUpdateService();

            var vm = new MainViewModel(inventoryService, imageStorage, dialogService, updateService);

            // When catalog empty
            await vm.InitializeAsync();
            Assert.True(vm.IsEmptyStateVisible);

            // Add product
            var cat = await inventoryService.AddCategoryAsync("Noutbuklar");
            await inventoryService.AddProductAsync("Dell Vostro", cat.Id, 10, null);

            await vm.LoadProductsCommand.ExecuteAsync(null);
            Assert.False(vm.IsEmptyStateVisible);

            // Filter with search that has no matches
            vm.SearchText = "NonExistentProduct";
            Assert.True(vm.IsEmptyStateVisible);

            // Clear search
            vm.SearchText = string.Empty;
            Assert.False(vm.IsEmptyStateVisible);
        }
    }
}

