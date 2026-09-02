using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StockDesk.Services;

public interface IImageStorageService
{
    Task<string> SaveImageAsync(string sourceFilePath);
    BitmapSource? LoadBitmap(string? imageFileName);
    void DeleteImage(string? imageFileName);
    string GetFullPath(string imageFileName);
}
