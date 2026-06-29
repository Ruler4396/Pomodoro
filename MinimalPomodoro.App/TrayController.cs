using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        var menu = new ContextMenuStrip
        {
            BackColor = Color.FromArgb(250, 251, 253),
            ForeColor = Color.FromArgb(32, 36, 42),
            Font = new Font("Segoe UI Variable Text", 9F, FontStyle.Regular, GraphicsUnit.Point),
            Padding = new Padding(6, 7, 6, 7),
            ShowImageMargin = false,
            RenderMode = ToolStripRenderMode.Professional,
            Renderer = new MacLikeToolStripRenderer()
        };
        if (snapshot.Phase == TimerPhase.Idle)
        {
            if (snapshot.LastCompletedPhase == TimerPhase.Focus)
            {
                AddMenuItem(menu, "开始休息", (_, _) => _controller.StartBreakAfterCompletedFocus(), true);
            }
            else
            {
                AddMenuItem(menu, "开始专注", (_, _) => _controller.StartFocus(), true);
            }
        }
        else
        {
            AddMenuItem(menu, snapshot.IsPaused ? "继续" : "暂停", (_, _) => _controller.PauseOrResume(), true);
            AddMenuItem(menu, "结束当前计时", (_, _) => _controller.EndCurrentPeriod());
            AddMenuItem(menu, "打开状态", (_, _) => _controller.ShowStatus());
        }

        menu.Items.Add(new ToolStripSeparator());
        AddMenuItem(menu, "设置...", (_, _) => _controller.ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        AddMenuItem(menu, "退出", (_, _) => _controller.Exit());
        return menu;
    }

    private static void AddMenuItem(ContextMenuStrip menu, string text, EventHandler onClick, bool isPrimary = false)
    {
        var item = new ToolStripMenuItem(text, null, onClick)
        {
            AutoSize = false,
            Size = new Size(156, 32),
            Padding = new Padding(12, 0, 12, 0),
            Margin = new Padding(0, 1, 0, 1),
            Tag = isPrimary
        };
        menu.Items.Add(item);
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

    private sealed class MacLikeToolStripRenderer : ToolStripProfessionalRenderer
    {
        private static readonly Color MenuBack = Color.FromArgb(250, 251, 253);
        private static readonly Color HoverBack = Color.FromArgb(235, 240, 247);
        private static readonly Color PrimaryHoverBack = Color.FromArgb(232, 64, 46);
        private static readonly Color TextColor = Color.FromArgb(32, 36, 42);
        private static readonly Color SeparatorColor = Color.FromArgb(226, 231, 238);

        public MacLikeToolStripRenderer() : base(new ProfessionalColorTable())
        {
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(MenuBack);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item is not ToolStripMenuItem item || !item.Selected)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var isPrimary = item.Tag is true;
            using var brush = new SolidBrush(isPrimary ? PrimaryHoverBack : HoverBack);
            var rect = new Rectangle(5, 2, item.Width - 10, item.Height - 4);
            using var path = RoundedRectangle(rect, 7);
            e.Graphics.FillPath(brush, path);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            var isPrimaryHover = e.Item is ToolStripMenuItem { Selected: true, Tag: true };
            e.TextColor = isPrimaryHover ? Color.White : TextColor;
            e.TextFont = new Font("Segoe UI Variable Text", 9F, FontStyle.Regular, GraphicsUnit.Point);
            e.TextRectangle = new Rectangle(e.TextRectangle.Left + 4, e.TextRectangle.Top, e.TextRectangle.Width, e.TextRectangle.Height);
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using var pen = new Pen(SeparatorColor);
            var y = e.Item.Height / 2;
            e.Graphics.DrawLine(pen, 10, y, e.Item.Width - 10, y);
        }

        private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
        {
            var diameter = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
