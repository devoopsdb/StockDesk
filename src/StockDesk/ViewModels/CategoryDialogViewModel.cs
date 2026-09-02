using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockDesk.Data.Entities;
using StockDesk.Services;

namespace StockDesk.ViewModels;

public partial class CategoryDialogViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<Category> Categories { get; } = new();

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private string _editingCategoryName = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    public bool IsSaved { get; private set; }

    public CategoryDialogViewModel(IInventoryService inventoryService, IDialogService dialogService)
    {
        _inventoryService = inventoryService;
        _dialogService = dialogService;
    }

    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        Categories.Clear();
        var list = await _inventoryService.GetCategoriesAsync();
        foreach (var cat in list)
        {
            Categories.Add(cat);
        }
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        ErrorMessage = null;
        EditingCategoryName = value?.Name ?? string.Empty;
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            ErrorMessage = "Kateqoriya adını daxil edin.";
            return;
        }

        try
        {
            await _inventoryService.AddCategoryAsync(NewCategoryName);
            NewCategoryName = string.Empty;
            await LoadCategoriesAsync();
            IsSaved = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task UpdateCategoryAsync()
    {
        ErrorMessage = null;
        if (SelectedCategory == null)
        {
            ErrorMessage = "Redaktə etmək üçün kateqoriya seçin.";
            return;
        }

        if (string.IsNullOrWhiteSpace(EditingCategoryName))
        {
            ErrorMessage = "Kateqoriya adı boş ola bilməz.";
            return;
        }

        try
        {
            await _inventoryService.UpdateCategoryAsync(SelectedCategory.Id, EditingCategoryName);
            await LoadCategoriesAsync();
            IsSaved = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(Category? category)
    {
        var target = category ?? SelectedCategory;
        if (target == null) return;

        ErrorMessage = null;
        if (!_dialogService.ShowConfirmation("Kateqoriyanı sil", $"'{target.Name}' kateqoriyasını silmək istədiyinizə əminsiniz?"))
        {
            return;
        }

        try
        {
            await _inventoryService.DeleteCategoryAsync(target.Id);
            await LoadCategoriesAsync();
            SelectedCategory = null;
            IsSaved = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
