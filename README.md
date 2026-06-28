# Minimal Pomodoro

A tray-first Windows Pomodoro timer built with .NET, WPF, and WinForms NotifyIcon.

## Features

- Starts in the system tray and opens a compact status window on launch.
- Tray menu supports starting, pausing, ending the current timer, opening status, settings, and exit.
- Configurable focus, short break, long break, and long break interval.
- Windows tray notifications with optional system sound.
- Lightweight local JSON settings and JSONL activity log.
- Minimal custom WPF UI with polished app and tray icons.

## Run

Use the published app:

```powershell
dist\MinimalPomodoro\MinimalPomodoro.App.exe
```

Or build from source:

```powershell
dotnet test MinimalPomodoro.Tests\MinimalPomodoro.Tests.csproj
dotnet publish MinimalPomodoro.App\MinimalPomodoro.App.csproj -c Release -r win-x64 --self-contained false -o dist\MinimalPomodoro
```

The release package contains the published application folder as a zip archive.
