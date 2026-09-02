using CommunityToolkit.Mvvm.ComponentModel;

namespace StockDesk.ViewModels;

public partial class WriteOffItemViewModel : ObservableObject
{
    public ProductItemViewModel Product { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyPropertyChangedFor(nameof(ValidationMessage))]
    private int _quantity = 1;

    public bool IsValid => Quantity > 0 && Quantity <= Product.CurrentBalance;

    public string? ValidationMessage
    {
        get
        {
            if (Quantity <= 0) return "Say 1 və ya daha çox olmalıdır.";
            if (Quantity > Product.CurrentBalance) return $"Maksimum qalıq: {Product.CurrentBalance} ədəd.";
            return null;
        }
    }

    public WriteOffItemViewModel(ProductItemViewModel product)
    {
        Product = product;
        // Default to 1, or maximum available if 0
        Quantity = product.CurrentBalance > 0 ? 1 : 0;
    }
}
