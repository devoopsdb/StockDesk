using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using StockDesk.Controls;
using StockDesk.Data.Entities;
using StockDesk.Services;
using StockDesk.ViewModels;
using Wpf.Ui.Controls;
using Xunit;

namespace StockDesk.Tests;

public class DialogViewModelTests
{
    [Fact]
    public async Task ProductDialogViewModel_SaveAsync_WithCustomInitialQuantity_PersistsCorrectBalance()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);
            var dialogService = new MockDialogService();

            var cat = await inventoryService.AddCategoryAsync("Noutbuklar");

            var vm = new ProductDialogViewModel(inventoryService, imageStorage, dialogService);
            await vm.InitializeAsync();

            vm.Name = "MacBook Pro 16";
            vm.SelectedCategory = vm.Categories.First(c => c.Id == cat.Id);
            vm.InitialQuantity = 15; // Custom quantity > 1

            bool saved = await vm.SaveAsync();

            Assert.True(saved);
            Assert.True(vm.IsSaved);

            var products = await inventoryService.GetProductsAsync();
            var savedProduct = products.FirstOrDefault(p => p.Name == "MacBook Pro 16");
            Assert.NotNull(savedProduct);
            Assert.Equal(15, savedProduct.CurrentBalance);

            var history = await inventoryService.GetHistoryAsync();
            var op = history.FirstOrDefault(h => h.ProductId == savedProduct.Id);
            Assert.NotNull(op);
            Assert.Equal(15, op.QuantityDelta);
            Assert.Equal(OperationType.Inflow, op.OperationType);
        }
    }

    [Fact]
    public async Task ReplenishDialogViewModel_ConfirmReplenish_WithCustomQuantity_IncrementsBalanceCorrectly()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);

            var cat = await inventoryService.AddCategoryAsync("Printerlər");
            var product = await inventoryService.AddProductAsync("Canon MF3010", cat.Id, 5, null);
            var productItemVm = new ProductItemViewModel(product, imageStorage);

            var vm = new ReplenishDialogViewModel(inventoryService);
            vm.Initialize(productItemVm);

            Assert.Equal(1, vm.Quantity);

            // User enters 25 items to add
            vm.Quantity = 25;
            vm.Note = "Böyük partiya təchizatı";

            await vm.ConfirmReplenishCommand.ExecuteAsync(null);

            Assert.True(vm.IsSaved);

            var updatedProduct = await inventoryService.GetProductByIdAsync(product.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal(30, updatedProduct.CurrentBalance); // 5 + 25 = 30

            var history = await inventoryService.GetHistoryAsync();
            var latest = history.First();
            Assert.Equal(25, latest.QuantityDelta);
            Assert.Equal(OperationType.Inflow, latest.OperationType);
            Assert.Equal("Böyük partiya təchizatı", latest.Note);
        }
    }

    [Fact]
    public async Task WriteOffDialogViewModel_ConfirmWriteOff_WithCustomQuantity_DecrementsBalanceCorrectly()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var imageStorage = new ImageStorageService();
            var recipientService = new RecipientService(context);
            var inventoryService = new InventoryService(context, recipientService, imageStorage);

            var cat = await inventoryService.AddCategoryAsync("Monitorlar");
            var product = await inventoryService.AddProductAsync("Dell UltraSharp 27", cat.Id, 20, null);
            var productItemVm = new ProductItemViewModel(product, imageStorage);

            var vm = new WriteOffDialogViewModel(inventoryService, recipientService);
            await vm.InitializeAsync(new[] { productItemVm });

            Assert.Single(vm.Items);
            Assert.Equal(1, vm.Items[0].Quantity);

            // User writes off 7 items
            vm.Items[0].Quantity = 7;
            vm.RecipientName = "Rəqəmsal İnkişaf Şöbəsi";
            vm.Note = "Yeni layihə üçün";

            await vm.ConfirmWriteOffCommand.ExecuteAsync(null);

            Assert.True(vm.IsSaved);

            var updatedProduct = await inventoryService.GetProductByIdAsync(product.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal(13, updatedProduct.CurrentBalance); // 20 - 7 = 13

            var history = await inventoryService.GetHistoryAsync();
            var latest = history.First();
            Assert.Equal(-7, latest.QuantityDelta);
            Assert.Equal(OperationType.Outflow, latest.OperationType);
            Assert.Equal("Rəqəmsal İnkişaf Şöbəsi", latest.RecipientNameSnapshot);
        }
    }

    [Fact]
    public void QuantityStepper_ValueBinding_WithPropertyChanged_SynchronizesWithViewModel()
    {
        var thread = new Thread(() =>
        {
            var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
            using (connection)
            using (context)
            {
                var imageStorage = new ImageStorageService();
                var recipientService = new RecipientService(context);
                var inventoryService = new InventoryService(context, recipientService, imageStorage);
                var dialogService = new MockDialogService();

                var vm = new ProductDialogViewModel(inventoryService, imageStorage, dialogService);
                var stepper = new QuantityStepper { Minimum = 0 };

                var binding = new Binding(nameof(ProductDialogViewModel.InitialQuantity))
                {
                    Source = vm,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(stepper, QuantityStepper.ValueProperty, binding);

                // Initial synchronization
                Assert.Equal(1, stepper.Value);
                Assert.Equal(1, vm.InitialQuantity);

                // Simulate changing stepper.Value to 18
                stepper.Value = 18;
                Assert.Equal(18, vm.InitialQuantity);

                // Simulate changing VM property
                vm.InitialQuantity = 42;
                Assert.Equal(42, stepper.Value);

                // Increment and Decrement
                stepper.Increment();
                Assert.Equal(43, vm.InitialQuantity);

                stepper.Decrement();
                Assert.Equal(42, vm.InitialQuantity);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
