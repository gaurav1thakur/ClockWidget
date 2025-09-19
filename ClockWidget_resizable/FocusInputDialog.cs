using System.Windows;
using System.Windows.Controls;

namespace ClockWidget
{
    public class FocusInputDialog : Window
    {
        private TextBox inputBox;
        public int DurationMinutes { get; private set; }

        public FocusInputDialog()
        {
            Title = "Start Focus Session";
            Width = 280;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var textBlock = new TextBlock { Text = "Duration (1–250 minutes):", Margin = new Thickness(0, 0, 0, 8) };
            grid.Children.Add(textBlock);

            inputBox = new TextBox { Text = Config.Current.LastFocusMinutes.ToString(), Width = 60 };
            Grid.SetRow(inputBox, 1);
            grid.Children.Add(inputBox);

            var panel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "Start", Width = 70, Margin = new Thickness(0, 8, 8, 0) };
            var cancelButton = new Button { Content = "Cancel", Width = 70, Margin = new Thickness(0, 8, 0, 0) };
            panel.Children.Add(okButton);
            panel.Children.Add(cancelButton);
            Grid.SetRow(panel, 2);
            grid.Children.Add(panel);

            okButton.Click += (s, e) =>
            {
                if (int.TryParse(inputBox.Text, out int minutes))
                {
                    minutes = System.Math.Max(1, System.Math.Min(250, minutes));
                    DurationMinutes = minutes;
                    Config.Current.LastFocusMinutes = minutes;
                    Config.Current.Save();
                    inputBox.Text = minutes.ToString();
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Enter a number between 1 and 250.");
                }
            };

            cancelButton.Click += (s, e) => DialogResult = false;

            Content = grid;
        }
    }
}
