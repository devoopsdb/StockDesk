using System;
using System.IO;

namespace StockDesk.Common;

public static class AppPaths
{
    public static string BaseDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "StockDesk"
    );

    public static string DatabasePath { get; } = Path.Combine(BaseDirectory, "stockdesk.db");

    public static string ImagesDirectory { get; } = Path.Combine(BaseDirectory, "Images");

    public static void EnsureDirectoriesCreated()
    {
        if (!Directory.Exists(BaseDirectory))
        {
            Directory.CreateDirectory(BaseDirectory);
        }

        if (!Directory.Exists(ImagesDirectory))
        {
            Directory.CreateDirectory(ImagesDirectory);
        }
    }
}
