using System.Windows;
using System.Windows.Input;
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
        if (sender is UIElement element)
        {
            FocusManager.SetFocusedElement(this, element);
        }

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
