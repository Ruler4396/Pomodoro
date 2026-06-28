using System.IO;
using System.Drawing;
using System.Windows.Forms;
using MinimalPomodoro.Core;

namespace MinimalPomodoro.App;

public sealed class TrayController : IDisposable
{
    private readonly PomodoroAppController _controller;
    private readonly NotifyIcon _notifyIcon;
    private TrayMenuSignature? _lastMenuSignature;

    public TrayController(PomodoroAppController controller)
    {
        _controller = controller;
        _notifyIcon = new NotifyIcon
        {
            Icon = LoadIcon(),
            Visible = true,
            Text = "番茄钟"
        };
        _notifyIcon.DoubleClick += (_, _) => _controller.ShowStatus();
    }

    public void Refresh()
    {
        var snapshot = _controller.Snapshot;
        var tooltip = BuildTooltip(snapshot);
        if (_notifyIcon.Text != tooltip)
        {
            _notifyIcon.Text = tooltip;
        }
        var signature = TrayMenuSignature.FromSnapshot(snapshot);
        if (_notifyIcon.ContextMenuStrip is null || _lastMenuSignature != signature)
        {
            _notifyIcon.ContextMenuStrip?.Dispose();
            _notifyIcon.ContextMenuStrip = BuildMenu(snapshot);
            _lastMenuSignature = signature;
        }
    }

    public void ShowBalloon(string title, string text)
    {
        _notifyIcon.ShowBalloonTip(10000, title, text, ToolTipIcon.Info);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
    }

    private ContextMenuStrip BuildMenu(TimerSnapshot snapshot)
    {
        var menu = new ContextMenuStrip();
        if (snapshot.Phase == TimerPhase.Idle)
        {
            if (snapshot.LastCompletedPhase == TimerPhase.Focus)
            {
                menu.Items.Add("开始休息", null, (_, _) => _controller.StartBreakAfterCompletedFocus());
            }
            else
            {
                menu.Items.Add("开始专注", null, (_, _) => _controller.StartFocus());
            }
        }
        else
        {
            menu.Items.Add(snapshot.IsPaused ? "继续" : "暂停", null, (_, _) => _controller.PauseOrResume());
            menu.Items.Add("结束当前计时", null, (_, _) => _controller.EndCurrentPeriod());
            menu.Items.Add("打开状态", null, (_, _) => _controller.ShowStatus());
        }

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("设置...", null, (_, _) => _controller.ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => _controller.Exit());
        return menu;
    }

    private static string BuildTooltip(TimerSnapshot snapshot)
    {
        if (snapshot.Phase == TimerPhase.Idle)
        {
            return "番茄钟 - 未运行";
        }

        var label = snapshot.Phase switch
        {
            TimerPhase.Focus => "专注中",
            TimerPhase.ShortBreak => "短休息",
            TimerPhase.LongBreak => "长休息",
            _ => "番茄钟"
        };
        if (snapshot.IsPaused)
        {
            label += " 已暂停";
        }

        return $"番茄钟 - {label} {snapshot.Remaining:mm\\:ss}";
    }

    private static Icon LoadIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "tray.ico");
        return File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application;
    }
}
