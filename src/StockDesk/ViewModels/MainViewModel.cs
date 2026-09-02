using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockDesk.Data.Entities;
using StockDesk.Services;

namespace StockDesk.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;
    private readonly IImageStorageService _imageStorageService;
    private readonly IDialogService _dialogService;

    private readonly List<ProductItemViewModel> _allProducts = new();

    public ObservableCollection<Category> CategoryFilters { get; } = new();
    public ObservableCollection<ProductItemViewModel> FilteredProducts { get; } = new();

    public ObservableCollection<string> SortOptions { get; } = new()
    {
        "Ad (A - Z)",
        "Ad (Z - A)",
        "Qalıq (Azalan)",
        "Qalıq (Artan)",
        "Tarix (Ən yeni)",
        "Tarix (Ən köhnə)"
    };

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategoryFilter;

    [ObservableProperty]
    private string _selectedSortOption = "Ad (A - Z)";

    [ObservableProperty]
    private int _selectedCount;

    [ObservableProperty]
    private bool _isBulkActionBarVisible;

    [ObservableProperty]
    private int _totalItemsCount;

    [ObservableProperty]
    private int _totalStockSum;

    [ObservableProperty]
    private bool _isLoading;

    private static readonly Category AllCategoriesFilter = new() { Id = 0, Name = "Bütün kateqoriyalar" };

    public MainViewModel(
        IInventoryService inventoryService,
        IImageStorageService imageStorageService,
        IDialogService dialogService)
    {
        _inventoryService = inventoryService;
        _imageStorageService = imageStorageService;
        _dialogService = dialogService;
    }

    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        CategoryFilters.Clear();
        CategoryFilters.Add(AllCategoriesFilter);

        var list = await _inventoryService.GetCategoriesAsync();
        foreach (var cat in list)
        {
            CategoryFilters.Add(cat);
        }

        SelectedCategoryFilter = AllCategoriesFilter;
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        IsLoading = true;
        try
        {
            foreach (var item in _allProducts)
            {
                item.PropertyChanged -= OnProductItemPropertyChanged;
            }
            _allProducts.Clear();

            var products = await _inventoryService.GetProductsAsync();
            foreach (var product in products)
            {
                var vm = new ProductItemViewModel(product, _imageStorageService);
                vm.PropertyChanged += OnProductItemPropertyChanged;
                _allProducts.Add(vm);
            }

            ApplyFiltersAndSort();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnProductItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductItemViewModel.IsSelected))
        {
            UpdateSelectionState();
        }
    }

    private void UpdateSelectionState()
    {
        SelectedCount = _allProducts.Count(p => p.IsSelected);
        IsBulkActionBarVisible = SelectedCount >= 2;
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFiltersAndSort();
    }

    partial void OnSelectedCategoryFilterChanged(Category? value)
    {
        ApplyFiltersAndSort();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        ApplyFiltersAndSort();
    }

    private void ApplyFiltersAndSort()
    {
        IEnumerable<ProductItemViewModel> query = _allProducts;

        // Category filter
        if (SelectedCategoryFilter != null && SelectedCategoryFilter.Id != 0)
        {
            query = query.Where(p => p.CategoryId == SelectedCategoryFilter.Id);
        }

        // Search text filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string search = SearchText.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || p.CategoryName.ToLower().Contains(search));
        }

        // Sorting
        query = SelectedSortOption switch
        {
            "Ad (A - Z)" => query.OrderBy(p => p.Name),
            "Ad (Z - A)" => query.OrderByDescending(p => p.Name),
            "Qalıq (Azalan)" => query.OrderByDescending(p => p.CurrentBalance),
            "Qalıq (Artan)" => query.OrderBy(p => p.CurrentBalance),
            "Tarix (Ən yeni)" => query.OrderByDescending(p => p.CreatedAt),
            "Tarix (Ən köhnə)" => query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        FilteredProducts.Clear();
        var list = query.ToList();
        foreach (var item in list)
        {
            FilteredProducts.Add(item);
        }

        TotalItemsCount = FilteredProducts.Count;
        TotalStockSum = FilteredProducts.Sum(p => p.CurrentBalance);
        UpdateSelectionState();
    }

    [RelayCommand]
    private async Task OpenAddCategoryAsync()
    {
        if (await _dialogService.ShowCategoryDialogAsync())
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private async Task OpenAddProductAsync()
    {
        if (CategoryFilters.Count <= 1)
        {
            _dialogService.ShowMessage("Məlumat", "Məhsul əlavə etmədən öncə ən azı bir kateqoriya yaratmalısınız.");
            await OpenAddCategoryAsync();
            if (CategoryFilters.Count <= 1) return;
        }

        if (await _dialogService.ShowProductDialogAsync())
        {
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private async Task EditProductAsync(ProductItemViewModel? product)
    {
        if (product == null) return;

        if (await _dialogService.ShowProductDialogAsync(product.Id))
        {
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteProductAsync(ProductItemViewModel? product)
    {
        if (product == null) return;

        if (_dialogService.ShowConfirmation("Məhsulu sil", $"'{product.Name}' məhsulunu bazadan silmək istədiyinizə əminsiniz?"))
        {
            await _inventoryService.DeleteProductAsync(product.Id);
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private async Task ReplenishStockAsync(ProductItemViewModel? product)
    {
        if (product == null) return;

        if (await _dialogService.ShowReplenishDialogAsync(product))
        {
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private async Task WriteOffSingleAsync(ProductItemViewModel? product)
    {
        if (product == null) return;

        if (product.CurrentBalance <= 0)
        {
            _dialogService.ShowMessage("Diqqət", $"'{product.Name}' məhsulunun qalığı 0-dır. Silinmə əməliyyatı mümkün deyil.", true);
            return;
        }

        if (await _dialogService.ShowWriteOffDialogAsync(new[] { product }))
        {
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private async Task WriteOffBulkAsync()
    {
        var selected = _allProducts.Where(p => p.IsSelected).ToList();
        if (!selected.Any()) return;

        var zeroStock = selected.Where(p => p.CurrentBalance <= 0).ToList();
        if (zeroStock.Any())
        {
            _dialogService.ShowMessage(
                "Diqqət",
                $"Seçilmiş bəzi məhsulların ({string.Join(", ", zeroStock.Select(p => p.Name))}) qalığı 0-dır. Onları seçimdən çıxarın.",
                true
            );
            return;
        }

        if (await _dialogService.ShowWriteOffDialogAsync(selected))
        {
            foreach (var p in selected) p.IsSelected = false;
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private void OpenHistory()
    {
        _dialogService.ShowHistoryWindow();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var p in FilteredProducts)
        {
            p.IsSelected = true;
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var p in _allProducts)
        {
            p.IsSelected = false;
        }
    }
}
