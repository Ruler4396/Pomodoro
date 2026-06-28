namespace MinimalPomodoro.Core;

public readonly record struct TrayMenuSignature(
    TimerPhase Phase,
    bool IsPaused,
    TimerPhase? LastCompletedPhase)
{
    public static TrayMenuSignature FromSnapshot(TimerSnapshot snapshot)
    {
        return new TrayMenuSignature(snapshot.Phase, snapshot.IsPaused, snapshot.LastCompletedPhase);
    }
}
