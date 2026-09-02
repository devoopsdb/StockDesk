using System.Windows;
using StockDesk.ViewModels;

namespace StockDesk.Views.Dialogs;

public partial class HistoryWindow : Window
{
    public HistoryViewModel ViewModel => (HistoryViewModel)DataContext;

    public HistoryWindow(HistoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
