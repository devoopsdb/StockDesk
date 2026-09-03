using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Sources;

namespace StockDesk.Services;

public class UpdateService : IUpdateService
{
    private readonly ILogger<UpdateService>? _logger;
    private readonly UpdateManager? _updateManager;
    private readonly object _lock = new();
    private UpdateInfo? _lastUpdateInfo;
    private bool _hasPendingUpdate;
    private bool _isUpdateDownloaded;
    private string? _pendingVersion;
    private string? _releaseNotes;

    public event EventHandler? UpdateStateChanged;

    public bool HasPendingUpdate
    {
        get { lock (_lock) return _hasPendingUpdate; }
        private set
        {
            lock (_lock) _hasPendingUpdate = value;
            UpdateStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsUpdateDownloaded
    {
        get { lock (_lock) return _isUpdateDownloaded; }
        private set
        {
            lock (_lock) _isUpdateDownloaded = value;
            UpdateStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string? PendingVersion
    {
        get { lock (_lock) return _pendingVersion; }
        private set { lock (_lock) _pendingVersion = value; }
    }

    public string? ReleaseNotes
    {
        get { lock (_lock) return _releaseNotes; }
        private set { lock (_lock) _releaseNotes = value; }
    }

    public UpdateService(ILogger<UpdateService>? logger = null, string githubRepoUrl = "https://github.com/devoopsdb/StockDesk")
    {
        _logger = logger;
        try
        {
            var source = new GithubSource(githubRepoUrl, accessToken: null, prerelease: false);
            _updateManager = new UpdateManager(source);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to initialize Velopack UpdateManager. Auto-updates may be disabled.");
        }
    }

    public UpdateService(UpdateManager updateManager, ILogger<UpdateService>? logger = null)
    {
        _updateManager = updateManager;
        _logger = logger;
    }

    public bool IsInstalled => _updateManager?.IsInstalled ?? false;

    public string CurrentVersion
    {
        get
        {
            if (_updateManager?.IsInstalled == true && _updateManager.CurrentVersion != null)
            {
                return _updateManager.CurrentVersion.ToFullString();
            }

            var asmVersion = Assembly.GetEntryAssembly()?.GetName().Version;
            return asmVersion != null ? $"{asmVersion.Major}.{asmVersion.Minor}.{asmVersion.Build}" : "1.0.0";
        }
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        if (_updateManager == null || !_updateManager.IsInstalled)
        {
            _logger?.LogInformation("Application is not running in installed/packaged mode. Skipping update check.");
            return UpdateCheckResult.DevModeResult();
        }

        try
        {
            _logger?.LogInformation("Checking for updates via GitHub Releases...");
            _lastUpdateInfo = await _updateManager.CheckForUpdatesAsync();

            if (_lastUpdateInfo != null)
            {
                var targetVersion = _lastUpdateInfo.TargetFullRelease.Version.ToFullString();
                _logger?.LogInformation("New update found: version {Version}", targetVersion);

                PendingVersion = targetVersion;
                ReleaseNotes = _lastUpdateInfo.TargetFullRelease.NotesMarkdown ?? _lastUpdateInfo.TargetFullRelease.NotesHTML;
                HasPendingUpdate = true;

                return new UpdateCheckResult(
                    IsUpdateDownloaded ? UpdateStatus.AlreadyDownloaded : UpdateStatus.UpdateAvailable,
                    targetVersion,
                    ReleaseNotes);
            }

            _logger?.LogInformation("Application is up to date (current version: {Version})", CurrentVersion);
            return UpdateCheckResult.UpToDateResult(CurrentVersion);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while checking for updates.");
            return UpdateCheckResult.Error(ex.Message);
        }
    }

    public async Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        if (_updateManager == null || !_updateManager.IsInstalled || _lastUpdateInfo == null)
        {
            return false;
        }

        try
        {
            _logger?.LogInformation("Downloading update {Version}...", _lastUpdateInfo.TargetFullRelease.Version.ToFullString());
            await _updateManager.DownloadUpdatesAsync(_lastUpdateInfo, p => progress?.Report(p), cancellationToken);
            IsUpdateDownloaded = true;
            _logger?.LogInformation("Update downloaded successfully and ready to apply.");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to download update packages.");
            return false;
        }
    }

    public void ApplyUpdatesAndRestart()
    {
        if (_updateManager == null || !_updateManager.IsInstalled || _lastUpdateInfo == null)
        {
            return;
        }

        try
        {
            _logger?.LogInformation("Applying update and restarting application...");
            _updateManager.ApplyUpdatesAndRestart(_lastUpdateInfo);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to restart and apply update.");
        }
    }
}
