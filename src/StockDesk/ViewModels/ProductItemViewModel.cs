using System;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using StockDesk.Data.Entities;
using StockDesk.Services;

namespace StockDesk.ViewModels;

public partial class ProductItemViewModel : ObservableObject
{
    private readonly IImageStorageService _imageStorageService;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _categoryId;

    [ObservableProperty]
    private string _categoryName = string.Empty;

    [ObservableProperty]
    private int _currentBalance;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private string? _imageFileName;

    [ObservableProperty]
    private BitmapSource? _imageSource;

    public Product Product { get; private set; }

    public ProductItemViewModel(Product product, IImageStorageService imageStorageService)
    {
        Product = product;
        _imageStorageService = imageStorageService;
        UpdateFromEntity(product);
    }

    public void UpdateFromEntity(Product product)
    {
        Product = product;
        Id = product.Id;
        Name = product.Name;
        CategoryId = product.CategoryId;
        CategoryName = product.Category?.Name ?? "Təyinsiz";
        CurrentBalance = product.CurrentBalance;
        CreatedAt = product.CreatedAt.ToLocalTime();
        ImageFileName = product.ImageFileName;
        
        LoadImage();
    }

    public void LoadImage()
    {
        ImageSource = _imageStorageService.LoadBitmap(ImageFileName);
    }
}
