using System.Text.Json;

namespace MinimalPomodoro.Core;

public sealed class JsonLinesActivityLogStore
{
    private readonly string _path;

    public JsonLinesActivityLogStore(string path)
    {
        _path = path;
    }

    public async Task AppendAsync(ActivityLogEvent activityLogEvent, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var line = JsonSerializer.Serialize(activityLogEvent);
        await File.AppendAllTextAsync(_path, line + Environment.NewLine, cancellationToken);
    }

    public async Task<int> CountCompletedFocusForDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_path))
        {
            return 0;
        }

        var count = 0;
        foreach (var line in await File.ReadAllLinesAsync(_path, cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var item = JsonSerializer.Deserialize<ActivityLogEvent>(line);
            if (item is { Action: ActivityLogAction.Completed, Phase: TimerPhase.Focus }
                && DateOnly.FromDateTime(item.OccurredAt.LocalDateTime) == date)
            {
                count++;
            }
        }

        return count;
    }
}
