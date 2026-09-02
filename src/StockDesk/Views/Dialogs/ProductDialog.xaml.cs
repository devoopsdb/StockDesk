using System.Windows;
using StockDesk.ViewModels;

namespace StockDesk.Views.Dialogs;

public partial class ProductDialog : Window
{
    public ProductDialogViewModel ViewModel => (ProductDialogViewModel)DataContext;

    public ProductDialog(ProductDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        bool success = await ViewModel.SaveAsync();
        if (success)
        {
            DialogResult = true;
            Close();
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
