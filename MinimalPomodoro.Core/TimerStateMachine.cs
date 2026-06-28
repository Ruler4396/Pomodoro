namespace MinimalPomodoro.Core;

public sealed class TimerStateMachine
{
    private readonly IClock _clock;
    private PomodoroSettings _settings;
    private TimerPhase _phase = TimerPhase.Idle;
    private DateTimeOffset? _periodStartedAt;
    private DateTimeOffset? _periodEndsAt;
    private TimeSpan _plannedDuration = TimeSpan.Zero;
    private TimeSpan _pausedRemaining = TimeSpan.Zero;
    private bool _isPaused;
    private int _completedFocusCount;
    private TimerPhase? _lastCompletedPhase;

    public TimerStateMachine(PomodoroSettings settings, IClock clock)
    {
        _settings = settings;
        _clock = clock;
    }

    public event EventHandler<TimerCompletedEventArgs>? Completed;

    public TimerSnapshot Snapshot => new(_phase, _isPaused, GetRemaining(), _completedFocusCount, _plannedDuration, _lastCompletedPhase);

    public void UpdateSettings(PomodoroSettings settings)
    {
        _settings = settings;
    }

    public void StartFocus()
    {
        StartPeriod(TimerPhase.Focus, _settings.FocusDuration);
    }

    public void StartBreakAfterCompletedFocus()
    {
        if (_completedFocusCount > 0 && _completedFocusCount % Math.Max(1, _settings.LongBreakInterval) == 0)
        {
            StartPeriod(TimerPhase.LongBreak, _settings.LongBreakDuration);
            return;
        }

        StartPeriod(TimerPhase.ShortBreak, _settings.ShortBreakDuration);
    }

    public void Pause()
    {
        if (_phase == TimerPhase.Idle || _isPaused)
        {
            return;
        }

        _pausedRemaining = GetRemaining();
        _isPaused = true;
    }

    public void Resume()
    {
        if (!_isPaused)
        {
            return;
        }

        _periodEndsAt = _clock.Now + _pausedRemaining;
        _isPaused = false;
    }

    public void CancelCurrentPeriod()
    {
        ResetPeriod();
    }

    public void Tick()
    {
        if (_phase == TimerPhase.Idle || _isPaused || GetRemaining() > TimeSpan.Zero)
        {
            return;
        }

        CompleteCurrentPeriod();
    }

    private void StartPeriod(TimerPhase phase, TimeSpan duration)
    {
        _phase = phase;
        _lastCompletedPhase = null;
        _plannedDuration = duration;
        _periodStartedAt = _clock.Now;
        _periodEndsAt = _clock.Now + duration;
        _pausedRemaining = TimeSpan.Zero;
        _isPaused = false;
    }

    private TimeSpan GetRemaining()
    {
        if (_phase == TimerPhase.Idle)
        {
            return TimeSpan.Zero;
        }

        if (_isPaused)
        {
            return _pausedRemaining;
        }

        var remaining = (_periodEndsAt ?? _clock.Now) - _clock.Now;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    private void CompleteCurrentPeriod()
    {
        var completedPhase = _phase;
        var actualDuration = _periodStartedAt.HasValue ? _clock.Now - _periodStartedAt.Value : _plannedDuration;
        if (completedPhase == TimerPhase.Focus)
        {
            _completedFocusCount++;
        }

        _lastCompletedPhase = completedPhase;
        ResetPeriod();
        Completed?.Invoke(this, new TimerCompletedEventArgs(completedPhase, _plannedDuration, actualDuration));
    }

    private void ResetPeriod()
    {
        _phase = TimerPhase.Idle;
        _periodStartedAt = null;
        _periodEndsAt = null;
        _plannedDuration = TimeSpan.Zero;
        _pausedRemaining = TimeSpan.Zero;
        _isPaused = false;
    }
}
