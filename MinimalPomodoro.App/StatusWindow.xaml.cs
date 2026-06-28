using System.Windows;
using System.Windows.Input;
using MinimalPomodoro.Core;

namespace MinimalPomodoro.App;

public partial class StatusWindow : Window
{
    private readonly PomodoroAppController _controller;

    public StatusWindow(PomodoroAppController controller)
    {
        _controller = controller;
        InitializeComponent();
        Refresh();
    }

    public void Refresh()
    {
        var snapshot = _controller.Snapshot;
        PhaseText.Text = snapshot.Phase switch
        {
            TimerPhase.Focus => snapshot.IsPaused ? "专注已暂停" : "专注中",
            TimerPhase.ShortBreak => snapshot.IsPaused ? "短休息已暂停" : "短休息",
            TimerPhase.LongBreak => snapshot.IsPaused ? "长休息已暂停" : "长休息",
            _ => "未运行"
        };
        RemainingText.Text = snapshot.Remaining.ToString("mm\\:ss");
        CompletedText.Text = $"当前已完成 {snapshot.CompletedFocusCount} 个番茄";
        PrimaryButton.Content = snapshot.Phase == TimerPhase.Idle
            ? snapshot.LastCompletedPhase == TimerPhase.Focus ? "开始休息" : "开始专注"
            : snapshot.IsPaused ? "继续" : "暂停";
        TopmostButton.Content = Topmost ? "取消置顶" : "置顶";
    }

    private void PrimaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_controller.Snapshot.Phase == TimerPhase.Idle)
        {
            if (_controller.Snapshot.LastCompletedPhase == TimerPhase.Focus)
            {
                _controller.StartBreakAfterCompletedFocus();
            }
            else
            {
                _controller.StartFocus();
            }
        }
        else
        {
            _controller.PauseOrResume();
        }
    }

    private void TopmostButton_Click(object sender, RoutedEventArgs e)
    {
        Topmost = !Topmost;
        Refresh();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void Chrome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}
