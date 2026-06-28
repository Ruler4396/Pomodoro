using MinimalPomodoro.Core;

namespace MinimalPomodoro.Tests;

public sealed class TimerStateMachineTests
{
    [Fact]
    public void StartFocusBeginsFocusPeriodWithConfiguredDuration()
    {
        var timer = new TimerStateMachine(PomodoroSettings.Default, new FakeClock(new DateTimeOffset(2026, 6, 28, 9, 0, 0, TimeSpan.Zero)));

        timer.StartFocus();

        Assert.Equal(TimerPhase.Focus, timer.Snapshot.Phase);
        Assert.False(timer.Snapshot.IsPaused);
        Assert.Equal(TimeSpan.FromMinutes(25), timer.Snapshot.Remaining);
    }

    [Fact]
    public void TickCompletesFocusAndWaitsForManualBreakStart()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 6, 28, 9, 0, 0, TimeSpan.Zero));
        var timer = new TimerStateMachine(PomodoroSettings.Default, clock);
        timer.StartFocus();

        TimerCompletedEventArgs? completed = null;
        timer.Completed += (_, args) => completed = args;

        clock.Advance(TimeSpan.FromMinutes(25));
        timer.Tick();

        Assert.NotNull(completed);
        Assert.Equal(TimerPhase.Focus, completed.CompletedPhase);
        Assert.Equal(TimerPhase.Idle, timer.Snapshot.Phase);
        Assert.Equal(1, timer.Snapshot.CompletedFocusCount);
        Assert.Equal(TimeSpan.Zero, timer.Snapshot.Remaining);
    }

    [Fact]
    public void ManualBreakStartUsesShortBreakUntilLongBreakInterval()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 6, 28, 9, 0, 0, TimeSpan.Zero));
        var settings = PomodoroSettings.Default with { LongBreakInterval = 4 };
        var timer = new TimerStateMachine(settings, clock);
        CompleteFocus(timer, clock);

        timer.StartBreakAfterCompletedFocus();

        Assert.Equal(TimerPhase.ShortBreak, timer.Snapshot.Phase);
        Assert.Equal(TimeSpan.FromMinutes(5), timer.Snapshot.Remaining);
    }

    [Fact]
    public void ManualBreakStartUsesLongBreakAtConfiguredInterval()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 6, 28, 9, 0, 0, TimeSpan.Zero));
        var settings = PomodoroSettings.Default with { LongBreakInterval = 2 };
        var timer = new TimerStateMachine(settings, clock);
        CompleteFocus(timer, clock);
        timer.StartBreakAfterCompletedFocus();
        timer.CancelCurrentPeriod();
        CompleteFocus(timer, clock);

        timer.StartBreakAfterCompletedFocus();

        Assert.Equal(TimerPhase.LongBreak, timer.Snapshot.Phase);
        Assert.Equal(TimeSpan.FromMinutes(15), timer.Snapshot.Remaining);
    }

    [Fact]
    public void PauseAndResumePreservesRemainingTime()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 6, 28, 9, 0, 0, TimeSpan.Zero));
        var timer = new TimerStateMachine(PomodoroSettings.Default, clock);
        timer.StartFocus();
        clock.Advance(TimeSpan.FromMinutes(7));
        timer.Pause();
        clock.Advance(TimeSpan.FromMinutes(5));

        Assert.True(timer.Snapshot.IsPaused);
        Assert.Equal(TimeSpan.FromMinutes(18), timer.Snapshot.Remaining);

        timer.Resume();
        clock.Advance(TimeSpan.FromMinutes(18));
        timer.Tick();

        Assert.Equal(TimerPhase.Idle, timer.Snapshot.Phase);
        Assert.Equal(1, timer.Snapshot.CompletedFocusCount);
    }

    [Fact]
    public void UpdatingSettingsAffectsOnlyFuturePeriods()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 6, 28, 9, 0, 0, TimeSpan.Zero));
        var timer = new TimerStateMachine(PomodoroSettings.Default, clock);
        timer.StartFocus();
        clock.Advance(TimeSpan.FromMinutes(5));

        timer.UpdateSettings(PomodoroSettings.Default with { FocusMinutes = 45 });

        Assert.Equal(TimeSpan.FromMinutes(20), timer.Snapshot.Remaining);

        clock.Advance(TimeSpan.FromMinutes(20));
        timer.Tick();
        timer.StartFocus();

        Assert.Equal(TimeSpan.FromMinutes(45), timer.Snapshot.Remaining);
    }

    private static void CompleteFocus(TimerStateMachine timer, FakeClock clock)
    {
        timer.StartFocus();
        clock.Advance(timer.Snapshot.Remaining);
        timer.Tick();
    }

    private sealed class FakeClock : IClock
    {
        public FakeClock(DateTimeOffset now)
        {
            Now = now;
        }

        public DateTimeOffset Now { get; private set; }

        public void Advance(TimeSpan duration)
        {
            Now += duration;
        }
    }
}
