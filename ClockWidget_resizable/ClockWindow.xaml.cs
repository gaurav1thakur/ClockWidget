using ClockWidget.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace ClockWidget
{
    public partial class ClockWindow : Window
    {
        // ===== Fields =====
        private bool _isRendering;
        private DateTime _focusStartUtc;
        private TimeSpan _focusDuration = TimeSpan.Zero;

        private double _size;     // current size
        private Point _center;    // center point
        private double _radius;   // outer radius

        private bool _draggingEnabled = true;
        private bool _clickThroughEnabled;

        private DispatcherTimer _blinkTimer;
        private int _blinkCount;
        private bool _isBlinking;


        // DependencyProperty for animated resizing (keep existing)
        public static readonly DependencyProperty TargetSizeProperty =
            DependencyProperty.Register(
                nameof(TargetSize),
                typeof(double),
                typeof(ClockWindow),
                new PropertyMetadata(350.0, (d, e) => ((ClockWindow)d).RecomputeLayout())
            );

        public double TargetSize
        {
            get => (double)GetValue(TargetSizeProperty);
            set => SetValue(TargetSizeProperty, value);
        }

        public ClockWindow()
        {
            this.LocationChanged += (s, e) => ClampToScreen();
            ThemeManager.ApplyTheme(Config.Current.Theme);

            InitializeComponent();

            // Start smooth rendering
            CompositionTarget.Rendering += OnRendering;
            _isRendering = true;

            Loaded += (s, e) => RecomputeLayout();
            SizeChanged += (s, e) => RecomputeLayout();
        }

        // ===== Rendering / Layout =====
        private void OnRendering(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            // Update date/time text
            DateText.Text = now.ToString("hh : mm"); // or "ddd dd" for TUE 16

            double ms = now.TimeOfDay.TotalMilliseconds % 1000.0;
            double seconds = now.Second + (ms / 1000.0);
            double minutes = now.Minute + (seconds / 60.0);
            double hours = (now.Hour % 12) + (minutes / 60.0);

            double secondAngle = 6.0 * seconds;
            double minuteAngle = 6.0 * minutes;
            double hourAngle = 30.0 * hours;

            DrawHand(HourHand, _radius * 0.55, 4, hourAngle);
            DrawHand(MinuteHand, _radius * 0.75, 3, minuteAngle);
            DrawHand(SecondHand, _radius * 0.85, 1.5, secondAngle);

            if (CenterCap != null)
            {
                double cap = Math.Max(4, _radius * 0.04);
                Canvas.SetLeft(CenterCap, _center.X - cap);
                Canvas.SetTop(CenterCap, _center.Y - cap);
                CenterCap.Width = CenterCap.Height = cap * 2;
            }

            // NEW: Adjust date position based on hour hand
            AdjustDatePosition(hourAngle);

            if (_focusDuration > TimeSpan.Zero)
                UpdateProgressFill();
        }

        private void RecomputeLayout()
        {
            _size = Math.Max(ActualWidth, ActualHeight);
            _center = new Point(_size / 2.0, _size / 2.0);
            _radius = (_size / 2.0) - 2;
            UpdateProgressGeometry();
        }

        private void DrawHand(Line line, double length, double thickness, double angleDeg)
        {
            if (line == null) return;
            double tail = length * 0.1;
            double rad = (Math.PI / 180.0) * (angleDeg - 90.0);
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            double x1 = _center.X - cos * tail;
            double y1 = _center.Y - sin * tail;
            double x2 = _center.X + cos * length;
            double y2 = _center.Y + sin * length;
            line.X1 = x1; line.Y1 = y1;
            line.X2 = x2; line.Y2 = y2;
            line.StrokeThickness = thickness;
            Panel.SetZIndex(line, 10);
        }

        private void AdjustDatePosition(double hourAngle)
        {
            // Flip the angle to place DateText opposite the hour hand
            double safeAngle = (hourAngle + 180.0) % 360.0;
            double angleRad = safeAngle * Math.PI / 180.0;

            double radiusOffset = _radius * 0.6; // Adjust as needed
            double offsetX = Math.Cos(angleRad) * radiusOffset;
            double offsetY = Math.Sin(angleRad) * radiusOffset;

            if (DateText != null)
            {
                double width = DateText.ActualWidth > 0 ? DateText.ActualWidth : DateText.RenderSize.Width;
                double height = DateText.ActualHeight > 0 ? DateText.ActualHeight : DateText.RenderSize.Height;

                Canvas.SetLeft(DateText, _center.X + offsetX - width / 2);
                Canvas.SetTop(DateText, _center.Y + offsetY - height / 2);
            }

        }


        // ===== Focus progress arc =====
        private void UpdateProgressGeometry()
        {
            if (ProgressArc == null) return;
            double inset = Math.Max(8, _radius * 0.08);
            double r = Math.Max(2, _radius - inset);
            ProgressArc.Tag = r;

            var geo = new PathGeometry();
            var fig = new PathFigure { IsFilled = false, IsClosed = false };
            Point start = PolarPoint(_center, r, -90);
            fig.StartPoint = start;
            fig.Segments.Add(new ArcSegment(start, new Size(r, r), 0, false, SweepDirection.Clockwise, true));
            geo.Figures.Add(fig);
            ProgressArc.Data = geo;
        }

        private void UpdateProgressFill()
        {
            if (ProgressArc == null || _focusDuration <= TimeSpan.Zero) return;

            double r = (double)(ProgressArc.Tag ?? Math.Max(2, _radius * 0.85));
            double elapsed = Math.Max(0, (DateTime.UtcNow - _focusStartUtc).TotalSeconds);
            double total = _focusDuration.TotalSeconds;
            double pct = Math.Min(1.0, elapsed / total);
            double sweepDeg = pct * 360.0;
            double endDeg = -90 + sweepDeg;

            Point start = PolarPoint(_center, r, -90);
            Point end = PolarPoint(_center, r, endDeg);
            bool large = sweepDeg > 180.0;

            var geo = new PathGeometry();
            var fig = new PathFigure { StartPoint = start, IsClosed = false, IsFilled = false };
            var arc = new ArcSegment { Point = end, Size = new Size(r, r), IsLargeArc = large, SweepDirection = SweepDirection.Clockwise };
            fig.Segments.Add(arc);
            geo.Figures.Add(fig);
            ProgressArc.Data = geo;

            if (pct >= 0.9 && !_isBlinking)
            {
                StartBlinkingUntilComplete();
            }

        }
        private void StartBlinkingUntilComplete()
        {
            if (_isBlinking || ProgressArc == null) return;

            _isBlinking = true;

            _blinkTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };

            _blinkTimer.Tick += (s, e) =>
            {
                ProgressArc.Visibility = (ProgressArc.Visibility == Visibility.Visible)
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                double elapsed = (DateTime.UtcNow - _focusStartUtc).TotalSeconds;
                double total = _focusDuration.TotalSeconds;

                if (elapsed >= total)
                {
                    _blinkTimer.Stop();
                    ProgressArc.Visibility = Visibility.Visible;
                    _isBlinking = false;

                    // Optional: final flash
                    var anim = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromMilliseconds(150),
                        AutoReverse = true
                    };
                    ProgressArc.BeginAnimation(OpacityProperty, anim);

                    _focusDuration = TimeSpan.Zero;
                }
            };

            _blinkTimer.Start();
        }
        private static Point PolarPoint(Point c, double r, double deg)
        {
            double rad = deg * Math.PI / 180.0;
            return new Point(c.X + Math.Cos(rad) * r, c.Y + Math.Sin(rad) * r);
        }

        public void StartFocusSession(int minutes)
        {
            _focusDuration = TimeSpan.FromMinutes(Math.Max(1, minutes));
            _focusStartUtc = DateTime.UtcNow;
            UpdateProgressGeometry();
            UpdateProgressFill();
        }

        // ===== Size / opacity (existing API kept) =====
        public void ApplySize(string size)
        {
            double target = size switch
            {
                "Small" => 90,
                "Medium" => 140,
                _ => 210
            };
            TargetSize = target;
            Width = Height = target;
            RecomputeLayout();
            ClampToScreen();
        }
        public void SetOpacity(double value)
        {
            ClockRoot.Opacity = Math.Max(0.2, Math.Min(1.0, value));
        }

        // ===== Click-through handling =====
        public void ApplyClickThrough(bool enabled, double minOpacity = 0.3)
        {
            _clickThroughEnabled = enabled;
            if (enabled)
            {
                SetOpacity(minOpacity);
                IsHitTestVisible = false;
                Helpers.Win32Helper.SetClickThrough(this, true);
                _draggingEnabled = false;
            }
            else
            {
                IsHitTestVisible = true;
                Helpers.Win32Helper.SetClickThrough(this, false);
                _draggingEnabled = true;
                SetOpacity(1);
            }
        }


        private void ClampToScreen()
        {
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var screenHeight = SystemParameters.VirtualScreenHeight;

            if (this.Left < 0)
                this.Left = 0;
            if (this.Top < 0)
                this.Top = 0;
            if (this.Left + this.Width > screenWidth)
                this.Left = screenWidth - this.Width;
            if (this.Top + this.Height > screenHeight)
                this.Top = screenHeight - this.Height;
        }


        public void ApplyDragging(bool enabled)
        {
            _draggingEnabled = enabled && !Config.Current.ClickThroughEnabled;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_draggingEnabled && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        public void ApplyOverlayMode(bool isOverlay)
        {
            if (isOverlay)
            {
                Topmost = true;
                ApplyClickThrough(Config.Current.ClickThroughEnabled, 0.3);
                WindowLayerHelper.ApplyOverlayMode(this);
            }
            else
            {
                Topmost = false;
                ApplyClickThrough(true, 0.3);
                WindowLayerHelper.ApplyDesktopMode(this);
            }
        }
    }

}