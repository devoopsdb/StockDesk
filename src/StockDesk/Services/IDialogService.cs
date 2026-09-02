using System.Collections.Generic;
using System.Threading.Tasks;
using StockDesk.ViewModels;

namespace StockDesk.Services;

public interface IDialogService
{
    Task<bool> ShowCategoryDialogAsync(int? categoryId = null);
    Task<bool> ShowProductDialogAsync(int? productId = null);
    Task<bool> ShowWriteOffDialogAsync(IEnumerable<ProductItemViewModel> products);
    Task<bool> ShowReplenishDialogAsync(ProductItemViewModel product);
    void ShowHistoryWindow();
    void ShowMessage(string title, string message, bool isError = false);
    bool ShowConfirmation(string title, string message);
    string? OpenImageFileDialog();
}
