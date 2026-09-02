using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using StockDesk.ViewModels;
using StockDesk.Views.Dialogs;

namespace StockDesk.Services;

public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private Window? GetActiveWindow()
    {
        return Application.Current.MainWindow?.IsLoaded == true ? Application.Current.MainWindow : null;
    }

    public async Task<bool> ShowCategoryDialogAsync(int? categoryId = null)
    {
        var vm = _serviceProvider.GetRequiredService<CategoryDialogViewModel>();
        await vm.InitializeAsync();

        var dialog = new CategoryDialog(vm)
        {
            Owner = GetActiveWindow()
        };

        return dialog.ShowDialog() == true;
    }

    public async Task<bool> ShowProductDialogAsync(int? productId = null)
    {
        var vm = _serviceProvider.GetRequiredService<ProductDialogViewModel>();
        await vm.InitializeAsync(productId);

        var dialog = new ProductDialog(vm)
        {
            Owner = GetActiveWindow()
        };

        return dialog.ShowDialog() == true;
    }

    public async Task<bool> ShowWriteOffDialogAsync(IEnumerable<ProductItemViewModel> products)
    {
        var vm = _serviceProvider.GetRequiredService<WriteOffDialogViewModel>();
        await vm.InitializeAsync(products);

        var dialog = new WriteOffDialog(vm)
        {
            Owner = GetActiveWindow()
        };

        return dialog.ShowDialog() == true;
    }

    public async Task<bool> ShowReplenishDialogAsync(ProductItemViewModel product)
    {
        var vm = _serviceProvider.GetRequiredService<ReplenishDialogViewModel>();
        vm.Initialize(product);

        var dialog = new ReplenishDialog(vm)
        {
            Owner = GetActiveWindow()
        };

        return dialog.ShowDialog() == true;
    }

    public async void ShowHistoryWindow()
    {
        var vm = _serviceProvider.GetRequiredService<HistoryViewModel>();
        await vm.InitializeAsync();

        var window = new HistoryWindow(vm)
        {
            Owner = GetActiveWindow()
        };

        window.ShowDialog();
    }

    public void ShowMessage(string title, string message, bool isError = false)
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            isError ? MessageBoxImage.Warning : MessageBoxImage.Information
        );
    }

    public bool ShowConfirmation(string title, string message)
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        return result == MessageBoxResult.Yes;
    }

    public string? OpenImageFileDialog()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Məhsulun şəklini seçin",
            Filter = "Şəkil faylları (*.jpg;*.jpeg;*.png;*.webp;*.bmp)|*.jpg;*.jpeg;*.png;*.webp;*.bmp|Bütün fayllar (*.*)|*.*",
            Multiselect = false
        };

        return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
    }
}
