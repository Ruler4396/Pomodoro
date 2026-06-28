namespace MinimalPomodoro.Core;

public sealed record TimerSnapshot(
    TimerPhase Phase,
    bool IsPaused,
    TimeSpan Remaining,
    int CompletedFocusCount,
    TimeSpan PlannedDuration,
    TimerPhase? LastCompletedPhase);
