using System;
using System.Threading;
using System.Threading.Tasks;
using StockDesk.Services;
using StockDesk.ViewModels;
using Xunit;

namespace StockDesk.Tests;

public class UpdateDialogViewModelTests
{
    private class FakeUpdateService : IUpdateService
    {
        public string CurrentVersion { get; set; } = "1.0.4";
        public bool IsInstalled { get; set; } = true;
        public bool HasPendingUpdate { get; set; } = false;
        public bool IsUpdateDownloaded { get; set; } = false;
        public string? PendingVersion { get; set; }
        public string? ReleaseNotes { get; set; }
        public bool DownloadResult { get; set; } = true;
        public bool AppliedAndRestarted { get; private set; }

        public event EventHandler? UpdateStateChanged { add { } remove { } }

        public Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            if (!IsInstalled)
                return Task.FromResult(UpdateCheckResult.DevModeResult());

            if (HasPendingUpdate)
                return Task.FromResult(new UpdateCheckResult(
                    IsUpdateDownloaded ? UpdateStatus.AlreadyDownloaded : UpdateStatus.UpdateAvailable,
                    PendingVersion ?? "1.1.0",
                    ReleaseNotes ?? "Notes"));

            return Task.FromResult(UpdateCheckResult.UpToDateResult(CurrentVersion));
        }

        public Task<bool> DownloadUpdatesAsync(IProgress<int>? progress = null, CancellationToken cancellationToken = default)
        {
            progress?.Report(50);
            progress?.Report(100);
            return Task.FromResult(DownloadResult);
        }

        public void ApplyUpdatesAndRestart()
        {
            AppliedAndRestarted = true;
        }
    }

    [Fact]
    public void Initialize_WithUpdateAvailable_SetsModeAndProperties()
    {
        var service = new FakeUpdateService { CurrentVersion = "1.0.4" };
        var vm = new UpdateDialogViewModel(service);

        vm.Initialize(UpdateCheckResult.Available("1.1.0", "Fixed table rendering"));

        Assert.Equal(UpdateDialogMode.UpdateAvailable, vm.CurrentMode);
        Assert.True(vm.IsUpdateAvailable);
        Assert.False(vm.IsChecking);
        Assert.Equal("1.1.0", vm.TargetVersion);
        Assert.Equal("Fixed table rendering", vm.ReleaseNotes);
        Assert.Equal("1.0.4", vm.CurrentVersion);
    }

    [Fact]
    public void Initialize_WithAlreadyDownloaded_SetsReadyToRestart()
    {
        var service = new FakeUpdateService { CurrentVersion = "1.0.4", PendingVersion = "1.1.0" };
        var vm = new UpdateDialogViewModel(service);

        vm.Initialize(new UpdateCheckResult(UpdateStatus.AlreadyDownloaded, "1.1.0", "Ready to install"));

        Assert.Equal(UpdateDialogMode.ReadyToRestart, vm.CurrentMode);
        Assert.True(vm.IsReadyToRestart);
        Assert.Equal("1.1.0", vm.TargetVersion);
    }

    [Fact]
    public void Initialize_WithUpToDate_SetsUpToDateMode()
    {
        var service = new FakeUpdateService { CurrentVersion = "1.0.4" };
        var vm = new UpdateDialogViewModel(service);

        vm.Initialize(UpdateCheckResult.UpToDateResult("1.0.4"));

        Assert.Equal(UpdateDialogMode.UpToDate, vm.CurrentMode);
        Assert.True(vm.IsUpToDate);
    }

    [Fact]
    public void Initialize_WithNetworkError_SetsNetworkErrorMode()
    {
        var service = new FakeUpdateService();
        var vm = new UpdateDialogViewModel(service);

        vm.Initialize(UpdateCheckResult.Error("No internet connection"));

        Assert.Equal(UpdateDialogMode.NetworkError, vm.CurrentMode);
        Assert.True(vm.IsNetworkError);
        Assert.Equal("No internet connection", vm.ErrorMessage);
    }

    [Fact]
    public void Initialize_WithDevMode_SetsDevMode()
    {
        var service = new FakeUpdateService { IsInstalled = false };
        var vm = new UpdateDialogViewModel(service);

        vm.Initialize(UpdateCheckResult.DevModeResult());

        Assert.Equal(UpdateDialogMode.DevMode, vm.CurrentMode);
        Assert.True(vm.IsDevMode);
    }

    [Fact]
    public async Task StartDownloadAsync_WhenSuccessful_ProgressesAndRestarts()
    {
        var service = new FakeUpdateService { DownloadResult = true };
        var vm = new UpdateDialogViewModel(service);
        vm.Initialize(UpdateCheckResult.Available("1.1.0", "New stuff"));

        await vm.StartDownloadAsync();

        Assert.Equal(100, vm.DownloadProgress);
        Assert.True(service.AppliedAndRestarted);
        Assert.Equal(UpdateDialogMode.ReadyToRestart, vm.CurrentMode);
    }

    [Fact]
    public async Task StartDownloadAsync_WhenFailed_TransitionsToNetworkError()
    {
        var service = new FakeUpdateService { DownloadResult = false };
        var vm = new UpdateDialogViewModel(service);
        vm.Initialize(UpdateCheckResult.Available("1.1.0", "New stuff"));

        await vm.StartDownloadAsync();

        Assert.Equal(UpdateDialogMode.NetworkError, vm.CurrentMode);
        Assert.True(vm.IsNetworkError);
    }

    [Fact]
    public void ApplyAndRestart_CallsUpdateService()
    {
        var service = new FakeUpdateService();
        var vm = new UpdateDialogViewModel(service);

        vm.ApplyAndRestart();

        Assert.True(service.AppliedAndRestarted);
    }

    [Fact]
    public void CloseCommand_TriggersRequestClose()
    {
        var service = new FakeUpdateService();
        var vm = new UpdateDialogViewModel(service);
        var closed = false;
        vm.RequestClose += () => closed = true;

        vm.Close();

        Assert.True(closed);
    }
}
