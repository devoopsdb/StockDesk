using System.Windows;
using StockDesk.ViewModels;

namespace StockDesk.Views;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (s, e) =>
        {
            await ViewModel.InitializeAsync();
        };
    }

    private void OnSelectAllClick(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectAllCommand.Execute(null);
    }

    private void OnClearSelectionClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearSelectionCommand.Execute(null);
    }
}
