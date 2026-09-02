using System.Windows;
using StockDesk.ViewModels;

namespace StockDesk.Views.Dialogs;

public partial class WriteOffDialog : Window
{
    public WriteOffDialogViewModel ViewModel => (WriteOffDialogViewModel)DataContext;

    public WriteOffDialog(WriteOffDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.ConfirmWriteOffCommand.ExecuteAsync(null);
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
