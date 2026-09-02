using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockDesk.Services;

namespace StockDesk.ViewModels;

public partial class WriteOffDialogViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;
    private readonly IRecipientService _recipientService;

    public ObservableCollection<WriteOffItemViewModel> Items { get; } = new();
    public ObservableCollection<string> RecipientSuggestions { get; } = new();

    [ObservableProperty]
    private string _recipientName = string.Empty;

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBulk;

    public bool IsSaved { get; private set; }

    public WriteOffDialogViewModel(
        IInventoryService inventoryService,
        IRecipientService recipientService)
    {
        _inventoryService = inventoryService;
        _recipientService = recipientService;
    }

    public async Task InitializeAsync(IEnumerable<ProductItemViewModel> products)
    {
        Items.Clear();
        foreach (var p in products)
        {
            Items.Add(new WriteOffItemViewModel(p));
        }

        IsBulk = Items.Count > 1;

        RecipientSuggestions.Clear();
        var names = await _recipientService.GetRecipientNamesAsync();
        foreach (var name in names)
        {
            RecipientSuggestions.Add(name);
        }
    }

    [RelayCommand]
    private async Task<bool> ConfirmWriteOffAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(RecipientName))
        {
            ErrorMessage = "Zəhmət olmasa təhvil alan şəxs və ya şöbə adını daxil edin.";
            return false;
        }

        if (!Items.Any())
        {
            ErrorMessage = "Heç bir məhsul seçilməyib.";
            return false;
        }

        foreach (var item in Items)
        {
            if (item.Quantity <= 0)
            {
                ErrorMessage = $"'{item.Product.Name}' üçün miqdar 0-dan böyük olmalıdır.";
                return false;
            }

            if (item.Quantity > item.Product.CurrentBalance)
            {
                ErrorMessage = $"'{item.Product.Name}' üçün daxil edilmiş say ({item.Quantity}) mövcud qalıqdan ({item.Product.CurrentBalance}) çoxdur.";
                return false;
            }
        }

        try
        {
            var tuples = Items.Select(i => (i.Product.Id, i.Quantity)).ToList();
            await _inventoryService.BulkWriteOffStockAsync(tuples, RecipientName.Trim(), Note);
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
