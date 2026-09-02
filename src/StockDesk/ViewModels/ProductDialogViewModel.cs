using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockDesk.Data.Entities;
using StockDesk.Services;

namespace StockDesk.ViewModels;

public partial class ProductDialogViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;
    private readonly IImageStorageService _imageStorageService;
    private readonly IDialogService _dialogService;

    private int? _editingProductId;
    private string? _existingImageFileName;
    private string? _newSelectedImageFilePath;

    public ObservableCollection<Category> Categories { get; } = new();

    [ObservableProperty]
    private string _dialogTitle = "Yeni Məhsul";

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private int _initialQuantity = 1;

    [ObservableProperty]
    private bool _isInitialQuantityVisible = true;

    [ObservableProperty]
    private BitmapSource? _previewImageSource;

    [ObservableProperty]
    private bool _hasImage;

    [ObservableProperty]
    private string? _errorMessage;

    public bool IsSaved { get; private set; }

    public ProductDialogViewModel(
        IInventoryService inventoryService,
        IImageStorageService imageStorageService,
        IDialogService dialogService)
    {
        _inventoryService = inventoryService;
        _imageStorageService = imageStorageService;
        _dialogService = dialogService;
    }

    public async Task InitializeAsync(int? productId = null)
    {
        _editingProductId = productId;
        Categories.Clear();
        var catList = await _inventoryService.GetCategoriesAsync();
        foreach (var c in catList)
        {
            Categories.Add(c);
        }

        if (productId.HasValue)
        {
            DialogTitle = "Məhsula Düzəliş";
            IsInitialQuantityVisible = false;

            var product = await _inventoryService.GetProductByIdAsync(productId.Value);
            if (product != null)
            {
                Name = product.Name;
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
                _existingImageFileName = product.ImageFileName;
                
                if (!string.IsNullOrEmpty(_existingImageFileName))
                {
                    PreviewImageSource = _imageStorageService.LoadBitmap(_existingImageFileName);
                    HasImage = PreviewImageSource != null;
                }
            }
        }
        else
        {
            DialogTitle = "Yeni Məhsul";
            IsInitialQuantityVisible = true;
            SelectedCategory = Categories.FirstOrDefault();
        }
    }

    [RelayCommand]
    private void ChoosePhoto()
    {
        ErrorMessage = null;
        string? selectedFile = _dialogService.OpenImageFileDialog();
        if (string.IsNullOrEmpty(selectedFile)) return;

        try
        {
            // Load preview
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(selectedFile);
            bmp.EndInit();
            bmp.Freeze();

            PreviewImageSource = bmp;
            HasImage = true;
            _newSelectedImageFilePath = selectedFile;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Şəkli yükləmək mümkün olmadı: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RemovePhoto()
    {
        PreviewImageSource = null;
        HasImage = false;
        _newSelectedImageFilePath = null;
        _existingImageFileName = null;
    }

    [RelayCommand]
    public async Task<bool> SaveAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Məhsulun adını daxil edin.";
            return false;
        }

        if (SelectedCategory == null)
        {
            ErrorMessage = "Kateqoriya seçin.";
            return false;
        }

        if (!_editingProductId.HasValue && InitialQuantity < 0)
        {
            ErrorMessage = "İlkin say mənfi ola bilməz.";
            return false;
        }

        try
        {
            string? finalImageFileName = _existingImageFileName;

            if (!string.IsNullOrEmpty(_newSelectedImageFilePath))
            {
                finalImageFileName = await _imageStorageService.SaveImageAsync(_newSelectedImageFilePath);
            }
            else if (!HasImage)
            {
                finalImageFileName = null;
            }

            if (_editingProductId.HasValue)
            {
                await _inventoryService.UpdateProductAsync(
                    _editingProductId.Value,
                    Name,
                    SelectedCategory.Id,
                    finalImageFileName
                );
            }
            else
            {
                await _inventoryService.AddProductAsync(
                    Name,
                    SelectedCategory.Id,
                    InitialQuantity,
                    finalImageFileName
                );
            }

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
