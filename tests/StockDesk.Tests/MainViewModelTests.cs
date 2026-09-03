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
    public UpdateCheckResult? LastShownUpdateResult { get; private set; }
    public Task ShowUpdateDialogAsync(UpdateCheckResult result)
    {
        LastShownUpdateResult = result;
        return Task.CompletedTask;
    }
}

public class MockUpdateService : IUpdateService
{
    public string CurrentVersion { get; set; } = "1.0.3";
    public bool IsInstalled { get; set; } = false;
    public bool HasPendingUpdate { get; set; } = false;
    public bool IsUpdateDownloaded { get; set; } = false;
    public string? PendingVersion { get; set; }
    public string? ReleaseNotes { get; set; }
    public event System.EventHandler? UpdateStateChanged;

    public void TriggerStateChanged() => UpdateStateChanged?.Invoke(this, System.EventArgs.Empty);

    public Task<UpdateCheckResult> CheckForUpdatesAsync(System.Threading.CancellationToken cancellationToken = default)
        => Task.FromResult(UpdateCheckResult.DevModeResult());
    public Task<bool> DownloadUpdatesAsync(System.IProgress<int>? progress = null, System.Threading.CancellationToken cancellationToken = default)
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

    [Fact]
    public async Task CheckForUpdatesCommand_WhenAlreadyDownloaded_ShowsAlreadyDownloadedResult()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);
            var dialogService = new MockDialogService();
            var updateService = new MockUpdateService
            {
                IsUpdateDownloaded = true,
                PendingVersion = "1.2.0",
                ReleaseNotes = "Ready for restart"
            };

            var vm = new MainViewModel(inventoryService, imageStorage, dialogService, updateService);

            await vm.CheckForUpdatesCommand.ExecuteAsync(null);

            Assert.NotNull(dialogService.LastShownUpdateResult);
            Assert.Equal(UpdateStatus.AlreadyDownloaded, dialogService.LastShownUpdateResult.Status);
            Assert.Equal("1.2.0", dialogService.LastShownUpdateResult.TargetVersion);
        }
    }

    [Fact]
    public void UpdateStateChanged_UpdatesHasPendingUpdateProperty()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);
            var dialogService = new MockDialogService();
            var updateService = new MockUpdateService { HasPendingUpdate = false };

            var vm = new MainViewModel(inventoryService, imageStorage, dialogService, updateService);
            Assert.False(vm.HasPendingUpdate);

            updateService.HasPendingUpdate = true;
            updateService.TriggerStateChanged();

            Assert.True(vm.HasPendingUpdate);
        }
    }
}

