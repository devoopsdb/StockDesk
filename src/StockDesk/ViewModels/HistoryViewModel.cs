using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockDesk.Data.Entities;
using StockDesk.Services;

namespace StockDesk.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;

    public ObservableCollection<InventoryOperation> Operations { get; } = new();

    public ObservableCollection<string> OperationTypes { get; } = new()
    {
        "Hamısı",
        "Mədaxil (Artım)",
        "Məxaric (Təhvil)"
    };

    [ObservableProperty]
    private string _selectedOperationType = "Hamısı";

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [ObservableProperty]
    private string _recipientFilter = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalOperationsCount;

    public HistoryViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task InitializeAsync()
    {
        await LoadHistoryAsync();
    }

    [RelayCommand]
    public async Task LoadHistoryAsync()
    {
        IsLoading = true;
        try
        {
            OperationType? filterType = SelectedOperationType switch
            {
                "Mədaxil (Artım)" => OperationType.Inflow,
                "Məxaric (Təhvil)" => OperationType.Outflow,
                _ => null
            };

            var list = await _inventoryService.GetHistoryAsync(
                FromDate,
                ToDate,
                filterType,
                RecipientFilter
            );

            Operations.Clear();
            foreach (var op in list)
            {
                Operations.Add(op);
            }
            TotalOperationsCount = Operations.Count;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SelectedOperationType = "Hamısı";
        FromDate = null;
        ToDate = null;
        RecipientFilter = string.Empty;
        await LoadHistoryAsync();
    }
}
