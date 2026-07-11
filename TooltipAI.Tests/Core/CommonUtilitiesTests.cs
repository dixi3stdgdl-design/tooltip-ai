using FluentAssertions;
using Xunit;
using TooltipAI.Core.Common;

namespace TooltipAI.Tests.Core;

public class AppDataPathsTests
{
    [Fact]
    public void RootShouldEndWithAppFolderName()
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppDataPaths.AppFolderName);

        AppDataPaths.Root.Should().Be(expected);
    }

    [Fact]
    public void CombineShouldAppendPartsUnderRoot()
    {
        var combined = AppDataPaths.Combine("models", "gemini-nano");

        combined.Should().Be(Path.Combine(AppDataPaths.Root, "models", "gemini-nano"));
    }

    [Fact]
    public void CombineWithNoPartsShouldReturnRoot()
    {
        AppDataPaths.Combine().Should().Be(AppDataPaths.Root);
    }

    [Fact]
    public void EnsureRootShouldCreateDirectoryAndReturnRoot()
    {
        var root = AppDataPaths.EnsureRoot();

        root.Should().Be(AppDataPaths.Root);
        Directory.Exists(root).Should().BeTrue();
    }
}

public class JsonFileTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"jsonfile_{Guid.NewGuid()}.json");

    public void Dispose()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }

    private sealed class Sample
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public void SaveThenLoadShouldRoundTrip()
    {
        var original = new Sample { Name = "tooltip", Value = 42 };

        JsonFile.Save(_path, original).Should().BeTrue();
        var loaded = JsonFile.Load(_path, () => new Sample());

        loaded.Name.Should().Be("tooltip");
        loaded.Value.Should().Be(42);
    }

    [Fact]
    public void SaveShouldWriteIndentedJsonByDefault()
    {
        JsonFile.Save(_path, new Sample { Name = "x", Value = 1 });

        File.ReadAllText(_path).Should().Contain(Environment.NewLine);
    }

    [Fact]
    public void LoadShouldReturnFallbackWhenFileMissing()
    {
        var result = JsonFile.Load(_path, () => new Sample { Name = "fallback" });

        result.Name.Should().Be("fallback");
    }

    [Fact]
    public void LoadShouldReturnFallbackWhenContentInvalid()
    {
        File.WriteAllText(_path, "{ not valid json");

        var result = JsonFile.Load(_path, () => new Sample { Name = "fallback" });

        result.Name.Should().Be("fallback");
    }
}

public class FileChangeWatcherTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"watch_{Guid.NewGuid()}.json");
    private FileSystemWatcher? _watcher;

    public void Dispose()
    {
        _watcher?.Dispose();
        if (File.Exists(_path))
            File.Delete(_path);
    }

    [Fact]
    public void TryWatchShouldReturnEnabledWatcherForValidPath()
    {
        File.WriteAllText(_path, "{}");

        _watcher = FileChangeWatcher.TryWatch(_path, (_, _) => { });

        _watcher.Should().NotBeNull();
        _watcher!.EnableRaisingEvents.Should().BeTrue();
        _watcher.Filter.Should().Be(Path.GetFileName(_path));
    }
}
