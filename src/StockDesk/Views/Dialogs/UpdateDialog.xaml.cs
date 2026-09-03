using System.Windows;
using StockDesk.ViewModels;

namespace StockDesk.Views.Dialogs;

public partial class UpdateDialog : Window
{
    public UpdateDialogViewModel ViewModel => (UpdateDialogViewModel)DataContext;

    public UpdateDialog(UpdateDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.RequestClose += () =>
        {
            DialogResult = true;
            Close();
        };
    }
}
