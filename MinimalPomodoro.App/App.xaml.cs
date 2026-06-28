using System.IO;
using System.Windows;
using MinimalPomodoro.Core;

namespace MinimalPomodoro.App;

public partial class App : System.Windows.Application
{
    private PomodoroAppController? _controller;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MinimalPomodoro");
        var settingsStore = new JsonSettingsStore(Path.Combine(appData, "settings.json"));
        var logStore = new JsonLinesActivityLogStore(Path.Combine(appData, "events.jsonl"));
        var settings = await settingsStore.LoadAsync();

        _controller = new PomodoroAppController(
            settings,
            settingsStore,
            logStore,
            new SystemClock(),
            new NotificationService(),
            new StartupService("MinimalPomodoro", Environment.ProcessPath ?? string.Empty));
        _controller.ExitRequested += (_, _) => Shutdown();
        _controller.Start();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _controller?.Dispose();
        base.OnExit(e);
    }
}
