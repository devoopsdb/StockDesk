using System.Threading.Tasks;
using StockDesk.Services;
using Xunit;

namespace StockDesk.Tests;

public class UpdateServiceTests
{
    [Fact]
    public async Task CheckForUpdatesAsync_WhenNotPackaged_ReturnsFalseGracefully()
    {
        var service = new UpdateService(logger: null, githubRepoUrl: "https://github.com/devoopsdb/StockDesk");

        Assert.False(service.IsInstalled);

        var result = await service.CheckForUpdatesAsync();

        Assert.NotNull(result);
        Assert.False(result.IsUpdateAvailable);
        Assert.Null(result.TargetVersion);
    }

    [Fact]
    public void CurrentVersion_ReturnsNonEmptyVersion()
    {
        var service = new UpdateService(logger: null, githubRepoUrl: "https://github.com/devoopsdb/StockDesk");

        var version = service.CurrentVersion;

        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    [Fact]
    public async Task DownloadUpdatesAsync_WhenNotInstalled_ReturnsFalse()
    {
        var service = new UpdateService(logger: null, githubRepoUrl: "https://github.com/devoopsdb/StockDesk");

        var success = await service.DownloadUpdatesAsync();

        Assert.False(success);
    }

    [Fact]
    public void ApplyUpdatesAndRestart_WhenNotInstalled_DoesNotThrow()
    {
        var service = new UpdateService(logger: null, githubRepoUrl: "https://github.com/devoopsdb/StockDesk");

        var exception = Record.Exception(() => service.ApplyUpdatesAndRestart());

        Assert.Null(exception);
    }
}
