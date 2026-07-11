using TooltipAI.Core.Services;
using Xunit;

namespace TooltipAI.Tests.Core;

public class ErrorPropagationTests
{
    [Fact]
    public void SettingsUpdate_PropagatesPersistenceFailure()
    {
        var path = CreateDirectoryPath();

        try
        {
            using var service = new SettingsService(path);

            Assert.ThrowsAny<Exception>(() =>
                service.UpdateSettings(settings => settings.IsEnabled = false));
            Assert.True(service.GetSettings().IsEnabled);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public void BlacklistUpdate_PropagatesPersistenceFailure()
    {
        var path = CreateDirectoryPath();

        try
        {
            using var service = new AppBlacklistService(path);

            Assert.ThrowsAny<Exception>(() => service.Add("example"));
            Assert.Empty(service.Blacklist);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public async Task UpdateCheck_PropagatesCancellation()
    {
        using var httpClient = new HttpClient(new BlockingHandler());
        var service = new UpdateService(httpClient);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.CheckForUpdateAsync(cts.Token));
    }

    private static string CreateDirectoryPath()
    {
        var path = Path.Combine(Path.GetTempPath(), $"tooltip-ai-error-{Guid.NewGuid()}");
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class BlockingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Unreachable");
        }
    }
}
