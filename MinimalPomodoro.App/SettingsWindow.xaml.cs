using System.Windows;
using System.Windows.Input;
using MinimalPomodoro.Core;

namespace MinimalPomodoro.App;

public partial class SettingsWindow : Window
{
    public SettingsWindow(PomodoroSettings settings, int todayCount)
    {
        InitializeComponent();
        TodayCountText.Text = $"今天已完成 {todayCount} 个番茄";
        FocusMinutesBox.Text = settings.FocusMinutes.ToString();
        ShortBreakMinutesBox.Text = settings.ShortBreakMinutes.ToString();
        LongBreakMinutesBox.Text = settings.LongBreakMinutes.ToString();
        LongBreakIntervalBox.Text = settings.LongBreakInterval.ToString();
        SoundCheckBox.IsChecked = settings.NotificationSoundEnabled;
        StartupCheckBox.IsChecked = settings.StartWithWindows;
    }

    public event EventHandler<PomodoroSettings>? SettingsSaved;

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadInt(FocusMinutesBox.Text, 1, 180, out var focus)
            || !TryReadInt(ShortBreakMinutesBox.Text, 1, 60, out var shortBreak)
            || !TryReadInt(LongBreakMinutesBox.Text, 1, 120, out var longBreak)
            || !TryReadInt(LongBreakIntervalBox.Text, 1, 12, out var interval))
        {
            System.Windows.MessageBox.Show(this, "请输入有效的分钟数和间隔。", "设置", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SettingsSaved?.Invoke(this, new PomodoroSettings
        {
            FocusMinutes = focus,
            ShortBreakMinutes = shortBreak,
            LongBreakMinutes = longBreak,
            LongBreakInterval = interval,
            NotificationSoundEnabled = SoundCheckBox.IsChecked == true,
            StartWithWindows = StartupCheckBox.IsChecked == true
        });
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Chrome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private static bool TryReadInt(string text, int min, int max, out int value)
    {
        return int.TryParse(text, out value) && value >= min && value <= max;
    }
}
