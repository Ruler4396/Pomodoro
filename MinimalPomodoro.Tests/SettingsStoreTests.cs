using MinimalPomodoro.Core;

namespace MinimalPomodoro.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), "MinimalPomodoro.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task LoadReturnsDefaultsWhenSettingsFileDoesNotExist()
    {
        var store = new JsonSettingsStore(Path.Combine(_tempDirectory, "settings.json"));

        var settings = await store.LoadAsync();

        Assert.Equal(PomodoroSettings.Default, settings);
    }

    [Fact]
    public async Task SaveCreatesDirectoryAndPersistsSettings()
    {
        var path = Path.Combine(_tempDirectory, "nested", "settings.json");
        var store = new JsonSettingsStore(path);
        var expected = PomodoroSettings.Default with
        {
            FocusMinutes = 45,
            ShortBreakMinutes = 10,
            LongBreakMinutes = 20,
            LongBreakInterval = 3,
            NotificationSoundEnabled = false,
            StartWithWindows = true
        };

        await store.SaveAsync(expected);
        var actual = await store.LoadAsync();

        Assert.Equal(expected, actual);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
