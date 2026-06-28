using System.Text.Json;

namespace MinimalPomodoro.Core;

public sealed class JsonSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _path;

    public JsonSettingsStore(string path)
    {
        _path = path;
    }

    public async Task<PomodoroSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_path))
        {
            return PomodoroSettings.Default;
        }

        await using var stream = File.OpenRead(_path);
        var settings = await JsonSerializer.DeserializeAsync<PomodoroSettings>(stream, SerializerOptions, cancellationToken);
        return Normalize(settings ?? PomodoroSettings.Default);
    }

    public async Task SaveAsync(PomodoroSettings settings, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_path);
        await JsonSerializer.SerializeAsync(stream, Normalize(settings), SerializerOptions, cancellationToken);
    }

    private static PomodoroSettings Normalize(PomodoroSettings settings)
    {
        return settings with
        {
            FocusMinutes = Math.Clamp(settings.FocusMinutes, 1, 180),
            ShortBreakMinutes = Math.Clamp(settings.ShortBreakMinutes, 1, 60),
            LongBreakMinutes = Math.Clamp(settings.LongBreakMinutes, 1, 120),
            LongBreakInterval = Math.Clamp(settings.LongBreakInterval, 1, 12)
        };
    }
}
