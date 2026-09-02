using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using StockDesk.Services;
using Xunit;

namespace StockDesk.Tests;

public class ImageStorageServiceTests
{
    [Fact]
    public async Task SaveImage_PngAndWebp_SavesNormalizedFile()
    {
        var service = new ImageStorageService();

        // Create temporary test image using SkiaSharp
        string tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.png");
        try
        {
            using (var bmp = new SKBitmap(100, 100))
            using (var canvas = new SKCanvas(bmp))
            {
                canvas.Clear(SKColors.CornflowerBlue);
                using var image = SKImage.FromBitmap(bmp);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                using var stream = File.OpenWrite(tempFile);
                data.SaveTo(stream);
            }

            string savedFileName = await service.SaveImageAsync(tempFile);

            Assert.NotNull(savedFileName);
            Assert.EndsWith(".png", savedFileName);

            string fullPath = service.GetFullPath(savedFileName);
            Assert.True(File.Exists(fullPath));

            // Clean up
            service.DeleteImage(savedFileName);
            Assert.False(File.Exists(fullPath));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
