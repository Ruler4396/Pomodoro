namespace MinimalPomodoro.Core;

public sealed record PomodoroSettings
{
    public static PomodoroSettings Default { get; } = new();

    public int FocusMinutes { get; init; } = 25;
    public int ShortBreakMinutes { get; init; } = 5;
    public int LongBreakMinutes { get; init; } = 15;
    public int LongBreakInterval { get; init; } = 4;
    public bool NotificationSoundEnabled { get; init; } = true;
    public bool StartWithWindows { get; init; }

    public TimeSpan FocusDuration => TimeSpan.FromMinutes(FocusMinutes);
    public TimeSpan ShortBreakDuration => TimeSpan.FromMinutes(ShortBreakMinutes);
    public TimeSpan LongBreakDuration => TimeSpan.FromMinutes(LongBreakMinutes);
}
