namespace MinimalPomodoro.Core;

public sealed record ActivityLogEvent(
    ActivityLogAction Action,
    TimerPhase Phase,
    DateTimeOffset OccurredAt,
    TimeSpan PlannedDuration,
    TimeSpan ActualDuration)
{
    public static ActivityLogEvent Started(TimerPhase phase, DateTimeOffset occurredAt, TimeSpan plannedDuration)
    {
        return new ActivityLogEvent(ActivityLogAction.Started, phase, occurredAt, plannedDuration, TimeSpan.Zero);
    }

    public static ActivityLogEvent Paused(TimerPhase phase, DateTimeOffset occurredAt, TimeSpan plannedDuration, TimeSpan actualDuration)
    {
        return new ActivityLogEvent(ActivityLogAction.Paused, phase, occurredAt, plannedDuration, actualDuration);
    }

    public static ActivityLogEvent Resumed(TimerPhase phase, DateTimeOffset occurredAt, TimeSpan plannedDuration, TimeSpan actualDuration)
    {
        return new ActivityLogEvent(ActivityLogAction.Resumed, phase, occurredAt, plannedDuration, actualDuration);
    }

    public static ActivityLogEvent Completed(TimerPhase phase, DateTimeOffset occurredAt, TimeSpan plannedDuration, TimeSpan actualDuration)
    {
        return new ActivityLogEvent(ActivityLogAction.Completed, phase, occurredAt, plannedDuration, actualDuration);
    }

    public static ActivityLogEvent Cancelled(TimerPhase phase, DateTimeOffset occurredAt, TimeSpan plannedDuration, TimeSpan actualDuration)
    {
        return new ActivityLogEvent(ActivityLogAction.Cancelled, phase, occurredAt, plannedDuration, actualDuration);
    }
}
