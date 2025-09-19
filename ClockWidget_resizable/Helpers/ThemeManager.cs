using System;
using System.Linq;
using System.Windows;

namespace ClockWidget.Helpers
{
    public static class ThemeManager
    {
        private const string ThemeFolder = "Themes/";
        private static readonly string[] KnownThemes = { "Light", "Dark" };

        public static void ApplyTheme(string themeName)
        {
            if (!KnownThemes.Contains(themeName, StringComparer.OrdinalIgnoreCase))
                themeName = "Light"; // fallback

            string uri = $"pack://application:,,,/{ThemeFolder}{themeName}Theme.xaml";

            try
            {
                var themeDict = new ResourceDictionary { Source = new Uri(uri) };

                var appResources = Application.Current.Resources.MergedDictionaries;

                // Remove existing theme dictionaries
                for (int i = appResources.Count - 1; i >= 0; i--)
                {
                    var dict = appResources[i];
                    if (dict.Source != null && dict.Source.OriginalString.StartsWith(ThemeFolder))
                        appResources.RemoveAt(i);
                }

                appResources.Add(themeDict);
                System.Diagnostics.Debug.WriteLine($"Theme applied: {themeName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply theme '{themeName}': {ex.Message}");
            }
        }

        public static string[] GetAvailableThemes() => KnownThemes;
    }
}