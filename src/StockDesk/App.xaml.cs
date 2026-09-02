using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockDesk.Common;
using StockDesk.Data;
using StockDesk.Services;
using StockDesk.ViewModels;
using StockDesk.Views;

namespace StockDesk;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Ensure storage directories exist
                AppPaths.EnsureDirectoriesCreated();

                // Database
                services.AddDbContext<StockDbContext>();

                // Core Services
                services.AddSingleton<IImageStorageService, ImageStorageService>();
                services.AddScoped<IRecipientService, RecipientService>();
                services.AddScoped<IInventoryService, InventoryService>();
                services.AddSingleton<IDialogService, DialogService>();

                // ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddTransient<CategoryDialogViewModel>();
                services.AddTransient<ProductDialogViewModel>();
                services.AddTransient<WriteOffDialogViewModel>();
                services.AddTransient<ReplenishDialogViewModel>();
                services.AddTransient<HistoryViewModel>();

                // Main Window
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await _host.StartAsync();

        // Initialize Database Schema and WAL mode
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
            db.InitializeDatabase();
        }

        // Show Main Window
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }

        base.OnExit(e);
    }
}
