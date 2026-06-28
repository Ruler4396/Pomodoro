using System.Media;
using MinimalPomodoro.Core;

namespace MinimalPomodoro.App;

public sealed class NotificationService : IDisposable
{
    public void Initialize()
    {
    }

    public void ShowInfo(string title, string text, bool soundEnabled, TrayController? tray)
    {
        tray?.ShowBalloon(title, text);
        if (soundEnabled)
        {
            SystemSounds.Exclamation.Play();
        }
    }

    public void ShowStarted(TimerPhase phase, bool soundEnabled, TrayController? tray)
    {
        var title = phase == TimerPhase.Focus ? "专注已开始" : "休息已开始";
        var text = phase == TimerPhase.Focus ? "番茄钟正在计时。" : "休息时间正在计时。";
        ShowInfo(title, text, soundEnabled, tray);
    }

    public void ShowCompleted(TimerPhase phase, bool soundEnabled, TrayController? tray)
    {
        if (phase == TimerPhase.Focus)
        {
            ShowInfo("该休息了", "专注时间已完成。右键托盘可开始休息。", soundEnabled, tray);
        }
        else
        {
            ShowInfo("休息结束", "可以开始下一段专注。右键托盘可开始专注。", soundEnabled, tray);
        }
    }

    public void Dispose()
    {
    }
}
