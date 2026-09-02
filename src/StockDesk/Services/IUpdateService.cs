using System.Threading;
using System.Threading.Tasks;

namespace StockDesk.Services;

public record UpdateCheckResult(bool IsUpdateAvailable, string? TargetVersion, bool IsDownloaded);

public interface IUpdateService
{
    string CurrentVersion { get; }
    bool IsInstalled { get; }
    Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
    Task<bool> DownloadUpdatesAsync(CancellationToken cancellationToken = default);
    void ApplyUpdatesAndRestart();
}
