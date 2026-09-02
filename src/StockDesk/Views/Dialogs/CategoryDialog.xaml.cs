using System.Windows;
using StockDesk.ViewModels;

namespace StockDesk.Views.Dialogs;

public partial class CategoryDialog : Window
{
    public CategoryDialogViewModel ViewModel => (CategoryDialogViewModel)DataContext;

    public CategoryDialog(CategoryDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        DialogResult = ViewModel.IsSaved;
        Close();
    }
}
