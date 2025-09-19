using ClockWidget.Helpers;
using System;
using System.Windows;

namespace ClockWidget
{
    public partial class App : Application
    {
        private TrayIconHelper _tray;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var clock = new ClockWindow();
            MainWindow = clock;
            clock.Show();

            this.ApplyTheme(Config.Current.Theme);

            clock.ApplyClickThrough(Config.Current.ClickThroughEnabled, 0.3);
            clock.SetOpacity(Config.Current.ClockOpacity);
            clock.ApplySize(Config.Current.ClockSize);
            clock.ApplyOverlayMode(Config.Current.IsOverlayMode);

            _tray = new TrayIconHelper(
                "ClockWidget.Assets.clock.ico", // Embedded resource name
                "Clock Widget"
            );

            // Clock size submenu
            _tray.AddSubMenu("Clock Size", sub =>
            {
                sub.AddMenuItem("Small", (s, a) => ChangeSize("Small"), isChecked: Config.Current.ClockSize == "Small");
                sub.AddMenuItem("Medium", (s, a) => ChangeSize("Medium"), isChecked: Config.Current.ClockSize == "Medium");
                sub.AddMenuItem("Large", (s, a) => ChangeSize("Large"), isChecked: Config.Current.ClockSize == "Large");
            });

            // Theme
            _tray.AddSubMenu("Theme", sub =>
            {
                sub.AddMenuItem("Light", (s, a) => ApplyTheme("Light"), isChecked: Config.Current.Theme == "Light");
                sub.AddMenuItem("Dark", (s, a) => ApplyTheme("Dark"), isChecked: Config.Current.Theme == "Dark");
                
            });

            // Focus session
            _tray.AddMenuItem("Start Focus Session", (s, a) =>
            {
                var dlg = new FocusInputDialog { Owner = Current.MainWindow };
                if (dlg.ShowDialog() == true && Current.MainWindow is ClockWindow cw)
                    cw.StartFocusSession(dlg.DurationMinutes);
            });

            // Settings
            _tray.AddMenuItem("Settings", (s, a) => OpenSettingsWindow());

            _tray.AddSeparator();

            // Click-through toggle
            _tray.AddMenuItem("Click-Through Mode", (s, a) => ToggleClickThrough(),
                checkable: true, isChecked: Config.Current.ClickThroughEnabled);

            _tray.AddSeparator();

            // Exit
            _tray.AddMenuItem("Exit", (s, a) => Shutdown());
        }

        private void ToggleClickThrough()
        {
            Config.Current.ClickThroughEnabled = !Config.Current.ClickThroughEnabled;
            if (MainWindow is ClockWindow cw)
                cw.ApplyClickThrough(Config.Current.ClickThroughEnabled, 0.3);
            Config.Current.Save();

            _tray.SetMenuChecked("Click-Through Mode", Config.Current.ClickThroughEnabled);
        }

        private void OpenSettingsWindow()
        {
            foreach (Window w in Current.Windows)
                if (w is SettingsWindow existing) { existing.Activate(); return; }

            var sw = new SettingsWindow { Owner = MainWindow };
            sw.Show();
        }

        private void ChangeSize(string size)
        {
            Config.Current.ClockSize = size;
            if (MainWindow is ClockWindow cw)
                cw.ApplySize(size);
            Config.Current.Save();

            _tray.SetSubMenuChecked("Clock Size", size);
        }

        public void ApplyTheme(string theme)
        {
            string dictPath = theme.Equals("Dark", StringComparison.OrdinalIgnoreCase)
                ? "pack://application:,,,/Themes/DarkTheme.xaml"
                : "pack://application:,,,/Themes/LightTheme.xaml";

            var themeDict = new ResourceDictionary { Source = new Uri(dictPath) };

            // Clear and apply globally
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(themeDict);
            
            System.Diagnostics.Debug.WriteLine("Applied theme: " + themeDict.Source);
            System.Diagnostics.Debug.WriteLine("Merged count: " + Application.Current.Resources.MergedDictionaries.Count);

            Config.Current.Theme = theme;
            Config.Current.Save();
            _tray?.SetSubMenuChecked("Theme", theme);

        }

        /// <summary>
        /// Restored for SettingsWindow compatibility — updates tray menu check state.
        /// </summary>
        public void SetTrayClickThroughChecked(bool value)
        {
            _tray?.SetMenuChecked("Click-Through Mode", value);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _tray?.Dispose();
            Config.Current.Save();
            base.OnExit(e);
        }
    }
}