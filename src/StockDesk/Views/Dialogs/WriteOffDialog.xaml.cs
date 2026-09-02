using System.Windows;
using System.Windows.Input;
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
        if (sender is UIElement element)
        {
            FocusManager.SetFocusedElement(this, element);
        }

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
