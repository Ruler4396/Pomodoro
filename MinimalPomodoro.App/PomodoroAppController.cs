using System.Windows.Threading;
using MinimalPomodoro.Core;

namespace MinimalPomodoro.App;

public sealed class PomodoroAppController : IDisposable
{
    private readonly JsonSettingsStore _settingsStore;
    private readonly JsonLinesActivityLogStore _logStore;
    private readonly IClock _clock;
    private readonly NotificationService _notificationService;
    private readonly StartupService _startupService;
    private readonly DispatcherTimer _tickTimer;
    private readonly TimerStateMachine _timer;
    private PomodoroSettings _settings;
    private TrayController? _tray;
    private SettingsWindow? _settingsWindow;
    private StatusWindow? _statusWindow;

    public PomodoroAppController(
        PomodoroSettings settings,
        JsonSettingsStore settingsStore,
        JsonLinesActivityLogStore logStore,
        IClock clock,
        NotificationService notificationService,
        StartupService startupService)
    {
        _settings = settings;
        _settingsStore = settingsStore;
        _logStore = logStore;
        _clock = clock;
        _notificationService = notificationService;
        _startupService = startupService;
        _timer = new TimerStateMachine(settings, clock);
        _timer.Completed += OnTimerCompleted;
        _tickTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _tickTimer.Tick += (_, _) => Tick();
    }

    public event EventHandler? ExitRequested;

    public TimerSnapshot Snapshot => _timer.Snapshot;

    public PomodoroSettings Settings => _settings;

    public async void Start()
    {
        _tray = new TrayController(this);
        _tray.Refresh();
        _notificationService.Initialize();
        _tickTimer.Start();
        await ApplyStartupSettingAsync();
        ShowStatus();
        _notificationService.ShowInfo("番茄钟已启动", "已常驻托盘。右键托盘图标可开始专注或打开设置。", false, _tray);
    }

    public async void StartFocus()
    {
        _timer.StartFocus();
        await _logStore.AppendAsync(ActivityLogEvent.Started(TimerPhase.Focus, _clock.Now, _timer.Snapshot.PlannedDuration));
        _notificationService.ShowStarted(TimerPhase.Focus, _settings.NotificationSoundEnabled, _tray);
        RefreshUi();
    }

    public async void StartBreakAfterCompletedFocus()
    {
        _timer.StartBreakAfterCompletedFocus();
        await _logStore.AppendAsync(ActivityLogEvent.Started(_timer.Snapshot.Phase, _clock.Now, _timer.Snapshot.PlannedDuration));
        _notificationService.ShowStarted(_timer.Snapshot.Phase, _settings.NotificationSoundEnabled, _tray);
        RefreshUi();
    }

    public async void PauseOrResume()
    {
        var snapshot = _timer.Snapshot;
        if (snapshot.Phase == TimerPhase.Idle)
        {
            return;
        }

        if (snapshot.IsPaused)
        {
            _timer.Resume();
            await _logStore.AppendAsync(ActivityLogEvent.Resumed(snapshot.Phase, _clock.Now, snapshot.PlannedDuration, snapshot.PlannedDuration - snapshot.Remaining));
        }
        else
        {
            _timer.Pause();
            await _logStore.AppendAsync(ActivityLogEvent.Paused(snapshot.Phase, _clock.Now, snapshot.PlannedDuration, snapshot.PlannedDuration - snapshot.Remaining));
        }

        RefreshUi();
    }

    public async void EndCurrentPeriod()
    {
        var snapshot = _timer.Snapshot;
        if (snapshot.Phase == TimerPhase.Idle)
        {
            return;
        }

        await _logStore.AppendAsync(ActivityLogEvent.Cancelled(snapshot.Phase, _clock.Now, snapshot.PlannedDuration, snapshot.PlannedDuration - snapshot.Remaining));
        _timer.CancelCurrentPeriod();
        _notificationService.ShowInfo("计时已结束", "当前专注已结束。", _settings.NotificationSoundEnabled, _tray);
        RefreshUi();
    }

    public async void ShowSettings()
    {
        if (_settingsWindow is { IsVisible: true })
        {
            _settingsWindow.Activate();
            return;
        }

        var todayCount = await _logStore.CountCompletedFocusForDateAsync(DateOnly.FromDateTime(DateTime.Now));
        _settingsWindow = new SettingsWindow(_settings, todayCount);
        _settingsWindow.SettingsSaved += async (_, settings) =>
        {
            _settings = settings;
            _timer.UpdateSettings(settings);
            await _settingsStore.SaveAsync(settings);
            await ApplyStartupSettingAsync();
            RefreshUi();
        };
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    public void ShowStatus()
    {
        if (_statusWindow is { IsVisible: true })
        {
            _statusWindow.Activate();
        }
        else
        {
            if (_statusWindow is null)
            {
                _statusWindow = new StatusWindow(this);
                _statusWindow.Closed += (_, _) => _statusWindow = null;
            }

            _statusWindow.Show();
            _statusWindow.Activate();
        }

        _statusWindow.Refresh();
    }

    public void Exit()
    {
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _tickTimer.Stop();
        _timer.Completed -= OnTimerCompleted;
        _notificationService.Dispose();
        _tray?.Dispose();
    }

    private void Tick()
    {
        _timer.Tick();
        RefreshUi();
    }

    private async void OnTimerCompleted(object? sender, TimerCompletedEventArgs e)
    {
        await _logStore.AppendAsync(ActivityLogEvent.Completed(e.CompletedPhase, _clock.Now, e.PlannedDuration, e.ActualDuration));
        _notificationService.ShowCompleted(e.CompletedPhase, _settings.NotificationSoundEnabled, _tray);
        RefreshUi();
    }

    private async Task ApplyStartupSettingAsync()
    {
        await _startupService.SetEnabledAsync(_settings.StartWithWindows);
    }

    private void RefreshUi()
    {
        _tray?.Refresh();
        _statusWindow?.Refresh();
    }
}
