using MinimalPomodoro.Core;

namespace MinimalPomodoro.Tests;

public sealed class TrayMenuSignatureTests
{
    [Fact]
    public void RemainingTimeChangesDoNotChangeMenuSignature()
    {
        var before = new TimerSnapshot(TimerPhase.Focus, false, TimeSpan.FromMinutes(24), 0, TimeSpan.FromMinutes(25), null);
        var after = before with { Remaining = TimeSpan.FromMinutes(23) };

        Assert.Equal(TrayMenuSignature.FromSnapshot(before), TrayMenuSignature.FromSnapshot(after));
    }

    [Fact]
    public void PauseStateChangesMenuSignature()
    {
        var before = new TimerSnapshot(TimerPhase.Focus, false, TimeSpan.FromMinutes(24), 0, TimeSpan.FromMinutes(25), null);
        var after = before with { IsPaused = true };

        Assert.NotEqual(TrayMenuSignature.FromSnapshot(before), TrayMenuSignature.FromSnapshot(after));
    }

    [Fact]
    public void LastCompletedPhaseChangesMenuSignature()
    {
        var before = new TimerSnapshot(TimerPhase.Idle, false, TimeSpan.Zero, 1, TimeSpan.Zero, null);
        var after = before with { LastCompletedPhase = TimerPhase.Focus };

        Assert.NotEqual(TrayMenuSignature.FromSnapshot(before), TrayMenuSignature.FromSnapshot(after));
    }
}
