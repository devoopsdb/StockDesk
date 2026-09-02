using System.Windows;
using StockDesk.ViewModels;

namespace StockDesk.Views.Dialogs;

public partial class ReplenishDialog : Window
{
    public ReplenishDialogViewModel ViewModel => (ReplenishDialogViewModel)DataContext;

    public ReplenishDialog(ReplenishDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.ConfirmReplenishCommand.ExecuteAsync(null);
        if (ViewModel.IsSaved)
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
