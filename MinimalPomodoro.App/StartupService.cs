using Microsoft.Win32;

namespace MinimalPomodoro.App;

public sealed class StartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private readonly string _appName;
    private readonly string _executablePath;

    public StartupService(string appName, string executablePath)
    {
        _appName = appName;
        _executablePath = executablePath;
    }

    public Task SetEnabledAsync(bool enabled)
    {
        if (string.IsNullOrWhiteSpace(_executablePath))
        {
            return Task.CompletedTask;
        }

        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        if (key is null)
        {
            return Task.CompletedTask;
        }

        if (enabled)
        {
            key.SetValue(_appName, $"\"{_executablePath}\"");
        }
        else
        {
            key.DeleteValue(_appName, throwOnMissingValue: false);
        }

        return Task.CompletedTask;
    }
}
