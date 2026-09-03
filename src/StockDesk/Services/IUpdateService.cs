using System;
using System.Threading;
using System.Threading.Tasks;

namespace StockDesk.Services;

public enum UpdateStatus
{
    UpdateAvailable,
    UpToDate,
    AlreadyDownloaded,
    NetworkError,
    DevMode
}

public record UpdateCheckResult(
    UpdateStatus Status,
    string? TargetVersion,
    string? ReleaseNotes = null,
    string? ErrorMessage = null)
{
    public bool IsUpdateAvailable => Status == UpdateStatus.UpdateAvailable || Status == UpdateStatus.AlreadyDownloaded;
    public bool IsDownloaded => Status == UpdateStatus.AlreadyDownloaded;

    // Helper factory constructor for compatibility
    public static UpdateCheckResult Available(string version, string? releaseNotes = null, bool isDownloaded = false) =>
        new(isDownloaded ? UpdateStatus.AlreadyDownloaded : UpdateStatus.UpdateAvailable, version, releaseNotes);

    public static UpdateCheckResult UpToDateResult(string currentVersion) =>
        new(UpdateStatus.UpToDate, currentVersion);

    public static UpdateCheckResult Error(string message) =>
        new(UpdateStatus.NetworkError, null, ErrorMessage: message);

    public static UpdateCheckResult DevModeResult() =>
        new(UpdateStatus.DevMode, null);
}

public interface IUpdateService
{
    string CurrentVersion { get; }
    bool IsInstalled { get; }
    bool HasPendingUpdate { get; }
    bool IsUpdateDownloaded { get; }
    string? PendingVersion { get; }
    string? ReleaseNotes { get; }

    event EventHandler? UpdateStateChanged;

    Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
    Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null, CancellationToken cancellationToken = default);
    void ApplyUpdatesAndRestart();
}
