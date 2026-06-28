using MinimalPomodoro.Core;

namespace MinimalPomodoro.Tests;

public sealed class ActivityLogStoreTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), "MinimalPomodoro.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task CountCompletedFocusForDateCountsOnlyCompletedFocusEventsOnThatDate()
    {
        var store = new JsonLinesActivityLogStore(Path.Combine(_tempDirectory, "events.jsonl"));
        var date = new DateOnly(2026, 6, 28);

        await store.AppendAsync(ActivityLogEvent.Completed(TimerPhase.Focus, new DateTimeOffset(2026, 6, 28, 9, 25, 0, TimeSpan.FromHours(8)), TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(25)));
        await store.AppendAsync(ActivityLogEvent.Completed(TimerPhase.ShortBreak, new DateTimeOffset(2026, 6, 28, 9, 30, 0, TimeSpan.FromHours(8)), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5)));
        await store.AppendAsync(ActivityLogEvent.Cancelled(TimerPhase.Focus, new DateTimeOffset(2026, 6, 28, 10, 0, 0, TimeSpan.FromHours(8)), TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(12)));
        await store.AppendAsync(ActivityLogEvent.Completed(TimerPhase.Focus, new DateTimeOffset(2026, 6, 27, 23, 50, 0, TimeSpan.FromHours(8)), TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(25)));

        var count = await store.CountCompletedFocusForDateAsync(date);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task AppendCreatesDirectoryAndKeepsJsonLinesReadable()
    {
        var path = Path.Combine(_tempDirectory, "nested", "events.jsonl");
        var store = new JsonLinesActivityLogStore(path);

        await store.AppendAsync(ActivityLogEvent.Started(TimerPhase.Focus, new DateTimeOffset(2026, 6, 28, 9, 0, 0, TimeSpan.FromHours(8)), TimeSpan.FromMinutes(25)));

        Assert.True(File.Exists(path));
        Assert.Single(await File.ReadAllLinesAsync(path));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
