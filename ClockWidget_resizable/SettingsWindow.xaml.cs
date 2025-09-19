using ClockWidget.Helpers;
using System;
using System.Windows;

namespace ClockWidget
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            ThemeManager.ApplyTheme(Config.Current.Theme); // before InitializeComponent

            InitializeComponent();

            ThemeComboBox.SelectedIndex = Config.Current.Theme.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            OpacitySlider.Value = Config.Current.ClockOpacity;
            ClickThroughCheckBox.IsChecked = Config.Current.ClickThroughEnabled;
            ThemeComboBox.SelectionChanged += (s, e) =>
            {
                var theme = (ThemeComboBox.SelectedIndex == 1) ? "Dark" : "Light";
                Config.Current.Theme = theme;
                ThemeManager.ApplyTheme(theme);
                Config.Current.Save();
            };

            OpacitySlider.ValueChanged += (s, e) =>
            {
                Config.Current.ClockOpacity = Math.Max(0.3, Math.Min(1.0, OpacitySlider.Value));
                if (Application.Current.MainWindow is ClockWindow cw)
                    cw.SetOpacity(Config.Current.ClockOpacity);
                Config.Current.Save();
            };

            ClickThroughCheckBox.Checked += (s, e) => UpdateClickThrough(true);
            ClickThroughCheckBox.Unchecked += (s, e) => UpdateClickThrough(false);

            StartFocusButton.Click += (s, e) =>
            {
                if (int.TryParse(FocusDurationBox.Text, out int minutes))
                {
                    minutes = Math.Max(1, Math.Min(250, minutes)); // clamp

                    if (Application.Current.MainWindow is ClockWindow cw)
                        cw.StartFocusSession(minutes);

                    Close(); // optional
                }
                else
                {
                    MessageBox.Show("Please enter a valid number between 1 and 250.");
                }
            };

            OverlayModeCheckBox.IsChecked = Config.Current.IsOverlayMode;

            OverlayModeCheckBox.Checked += (s, e) => UpdateOverlayMode(true);
            OverlayModeCheckBox.Unchecked += (s, e) => UpdateOverlayMode(false);
        }

        private void UpdateOverlayMode(bool isOverlay)
        {
            Config.Current.IsOverlayMode = isOverlay;
            Config.Current.Save();

            if (Application.Current.MainWindow is ClockWindow cw)
                cw.ApplyOverlayMode(isOverlay);
        }
        private void UpdateClickThrough(bool enabled)
        {
            Config.Current.ClickThroughEnabled = enabled;

            if (Application.Current.MainWindow != null)
                Win32Helper.SetClickThrough(Application.Current.MainWindow, enabled);

            (Application.Current as App)?.SetTrayClickThroughChecked(enabled);
            Config.Current.Save();

            if (Application.Current.MainWindow is ClockWindow cw)
                cw.ApplyClickThrough(enabled);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Config.Current.Theme = "Light";
            Config.Current.ClockOpacity = 1.0;
            Config.Current.ClickThroughEnabled = false;
            Config.Current.Save();

            (Application.Current as App)?.ApplyTheme("Light");
            if (Application.Current.MainWindow is ClockWindow cw)
                cw.SetOpacity(1.0);
            if (Application.Current.MainWindow != null)
                Win32Helper.SetClickThrough(Application.Current.MainWindow, false);
            (Application.Current as App)?.SetTrayClickThroughChecked(false);
            Close();
        }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

    }
}