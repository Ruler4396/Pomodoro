namespace MinimalPomodoro.Core;

public sealed class TimerCompletedEventArgs : EventArgs
{
    public TimerCompletedEventArgs(TimerPhase completedPhase, TimeSpan plannedDuration, TimeSpan actualDuration)
    {
        CompletedPhase = completedPhase;
        PlannedDuration = plannedDuration;
        ActualDuration = actualDuration;
    }

    public TimerPhase CompletedPhase { get; }
    public TimeSpan PlannedDuration { get; }
    public TimeSpan ActualDuration { get; }
}
