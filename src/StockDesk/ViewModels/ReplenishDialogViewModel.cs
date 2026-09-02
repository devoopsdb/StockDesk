using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockDesk.Services;

namespace StockDesk.ViewModels;

public partial class ReplenishDialogViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;

    [ObservableProperty]
    private ProductItemViewModel? _product;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    public bool IsSaved { get; private set; }

    public ReplenishDialogViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public void Initialize(ProductItemViewModel product)
    {
        Product = product;
        Quantity = 1;
        Note = string.Empty;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task<bool> ConfirmReplenishAsync()
    {
        ErrorMessage = null;
        if (Product == null) return false;

        if (Quantity <= 0)
        {
            ErrorMessage = "Artırılacaq say 1 və ya daha çox olmalıdır.";
            return false;
        }

        try
        {
            await _inventoryService.ReplenishStockAsync(Product.Id, Quantity, Note);
            IsSaved = true;
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
    }
}
