using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SkiaSharp;
using StockDesk.Common;

namespace StockDesk.Services;

public class ImageStorageService : IImageStorageService
{
    private const int MaxDimension = 1024;

    public ImageStorageService()
    {
        AppPaths.EnsureDirectoriesCreated();
    }

    public async Task<string> SaveImageAsync(string sourceFilePath)
    {
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Seçilmiş şəkil faylı tapılmadı.", sourceFilePath);
        }

        AppPaths.EnsureDirectoriesCreated();

        return await Task.Run(() =>
        {
            using var inputStream = File.OpenRead(sourceFilePath);
            using var codec = SKCodec.Create(inputStream);
            if (codec == null)
            {
                throw new InvalidOperationException("Şəkil formatı oxuna bilmədi.");
            }

            using var originalBitmap = SKBitmap.Decode(codec);
            if (originalBitmap == null)
            {
                throw new InvalidOperationException("Şəkil emal edilə bilmədi.");
            }

            SKBitmap finalBitmap = originalBitmap;
            bool wasResized = false;

            if (originalBitmap.Width > MaxDimension || originalBitmap.Height > MaxDimension)
            {
                float ratio = Math.Min((float)MaxDimension / originalBitmap.Width, (float)MaxDimension / originalBitmap.Height);
                int targetWidth = Math.Max(1, (int)(originalBitmap.Width * ratio));
                int targetHeight = Math.Max(1, (int)(originalBitmap.Height * ratio));

                var imageInfo = new SKImageInfo(targetWidth, targetHeight, originalBitmap.ColorType, originalBitmap.AlphaType);
                var resized = originalBitmap.Resize(imageInfo, SKSamplingOptions.Default);
                if (resized != null)
                {
                    finalBitmap = resized;
                    wasResized = true;
                }
            }

            try
            {
                using var image = SKImage.FromBitmap(finalBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 90);

                string fileName = $"{Guid.NewGuid():N}.png";
                string targetPath = Path.Combine(AppPaths.ImagesDirectory, fileName);

                using (var outputStream = File.Create(targetPath))
                {
                    data.SaveTo(outputStream);
                }

                return fileName;
            }
            finally
            {
                if (wasResized)
                {
                    finalBitmap.Dispose();
                }
            }
        });
    }

    public BitmapSource? LoadBitmap(string? imageFileName)
    {
        if (string.IsNullOrWhiteSpace(imageFileName))
        {
            return null;
        }

        string fullPath = Path.Combine(AppPaths.ImagesDirectory, imageFileName);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        try
        {
            byte[] fileBytes = File.ReadAllBytes(fullPath);
            using var memoryStream = new MemoryStream(fileBytes);
            
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
        catch
        {
            return null;
        }
    }

    public void DeleteImage(string? imageFileName)
    {
        if (string.IsNullOrWhiteSpace(imageFileName))
        {
            return;
        }

        string fullPath = Path.Combine(AppPaths.ImagesDirectory, imageFileName);
        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // Ignore deletion error
        }
    }

    public string GetFullPath(string imageFileName)
    {
        return Path.Combine(AppPaths.ImagesDirectory, imageFileName);
    }
}
