using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockDesk.Services;

namespace StockDesk.ViewModels;

public enum UpdateDialogMode
{
    Checking,
    UpdateAvailable,
    Downloading,
    ReadyToRestart,
    UpToDate,
    NetworkError,
    DevMode
}

public partial class UpdateDialogViewModel : ObservableObject
{
    private readonly IUpdateService _updateService;
    private CancellationTokenSource? _downloadCts;

    public event Action? RequestClose;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChecking))]
    [NotifyPropertyChangedFor(nameof(IsUpdateAvailable))]
    [NotifyPropertyChangedFor(nameof(IsDownloading))]
    [NotifyPropertyChangedFor(nameof(IsReadyToRestart))]
    [NotifyPropertyChangedFor(nameof(IsUpToDate))]
    [NotifyPropertyChangedFor(nameof(IsNetworkError))]
    [NotifyPropertyChangedFor(nameof(IsDevMode))]
    private UpdateDialogMode _currentMode = UpdateDialogMode.Checking;

    [ObservableProperty]
    private string _currentVersion = string.Empty;

    [ObservableProperty]
    private string _targetVersion = string.Empty;

    [ObservableProperty]
    private string _releaseNotes = string.Empty;

    [ObservableProperty]
    private int _downloadProgress;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool IsChecking => CurrentMode == UpdateDialogMode.Checking;
    public bool IsUpdateAvailable => CurrentMode == UpdateDialogMode.UpdateAvailable;
    public bool IsDownloading => CurrentMode == UpdateDialogMode.Downloading;
    public bool IsReadyToRestart => CurrentMode == UpdateDialogMode.ReadyToRestart;
    public bool IsUpToDate => CurrentMode == UpdateDialogMode.UpToDate;
    public bool IsNetworkError => CurrentMode == UpdateDialogMode.NetworkError;
    public bool IsDevMode => CurrentMode == UpdateDialogMode.DevMode;

    public UpdateDialogViewModel(IUpdateService updateService)
    {
        _updateService = updateService;
        CurrentVersion = _updateService.CurrentVersion;
    }

    public void Initialize(UpdateCheckResult result)
    {
        CurrentVersion = _updateService.CurrentVersion;
        ApplyCheckResult(result);
    }

    private void ApplyCheckResult(UpdateCheckResult result)
    {
        switch (result.Status)
        {
            case UpdateStatus.AlreadyDownloaded:
                CurrentMode = UpdateDialogMode.ReadyToRestart;
                TargetVersion = result.TargetVersion ?? _updateService.PendingVersion ?? CurrentVersion;
                ReleaseNotes = result.ReleaseNotes ?? _updateService.ReleaseNotes ?? "Yeniliklər haqqında məlumat qeyd edilməyib.";
                break;

            case UpdateStatus.UpdateAvailable:
                CurrentMode = UpdateDialogMode.UpdateAvailable;
                TargetVersion = result.TargetVersion ?? "";
                ReleaseNotes = !string.IsNullOrWhiteSpace(result.ReleaseNotes)
                    ? result.ReleaseNotes
                    : "Yeniliklər haqqında məlumat qeyd edilməyib.";
                break;

            case UpdateStatus.UpToDate:
                CurrentMode = UpdateDialogMode.UpToDate;
                break;

            case UpdateStatus.NetworkError:
                CurrentMode = UpdateDialogMode.NetworkError;
                ErrorMessage = result.ErrorMessage ?? "Yeniləmə serveri ilə əlaqə yaratmaq mümkün olmadı.";
                break;

            case UpdateStatus.DevMode:
                CurrentMode = UpdateDialogMode.DevMode;
                break;
        }
    }

    [RelayCommand]
    public async Task StartDownloadAsync()
    {
        CurrentMode = UpdateDialogMode.Downloading;
        DownloadProgress = 0;
        StatusMessage = "Yeniləmə paketi yüklənir...";

        _downloadCts = new CancellationTokenSource();
        var progress = new Progress<int>(percent =>
        {
            DownloadProgress = percent;
        });

        try
        {
            var success = await _updateService.DownloadUpdatesAsync(progress, _downloadCts.Token);
            if (success)
            {
                DownloadProgress = 100;
                StatusMessage = "Yükləmə tamamlandı! Tətbiq yenidən başladılır...";
                CurrentMode = UpdateDialogMode.ReadyToRestart;

                // Seamless automatic restart upon confirmed download completion
                await Task.Delay(500);
                _updateService.ApplyUpdatesAndRestart();
            }
            else
            {
                CurrentMode = UpdateDialogMode.NetworkError;
                ErrorMessage = "Yeniləmə faylı endirilərkən xəta baş verdi. İnternet bağlantısını yoxlayın.";
            }
        }
        catch (OperationCanceledException)
        {
            CurrentMode = UpdateDialogMode.UpdateAvailable;
        }
        catch (Exception ex)
        {
            CurrentMode = UpdateDialogMode.NetworkError;
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    public void ApplyAndRestart()
    {
        _updateService.ApplyUpdatesAndRestart();
    }

    [RelayCommand]
    public async Task RetryCheckAsync()
    {
        CurrentMode = UpdateDialogMode.Checking;
        var result = await _updateService.CheckForUpdatesAsync();
        ApplyCheckResult(result);
    }

    [RelayCommand]
    public void Close()
    {
        _downloadCts?.Cancel();
        RequestClose?.Invoke();
    }
}
