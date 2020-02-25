using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SerialCATCommand {
    /// <summary>
    /// Lógica de interacción para Dial.xaml
    /// </summary>
    public partial class Dial : UserControl {
        const double OneTwentyDegreesInRadians = (System.Math.PI + System.Math.PI) / 3;

        // Hardcoded to be the same as the actual controlwidth in DIU
        private static double ControlWidth = 200;

        public Dial() {
            InitializeComponent();

            _fontBrush = Brushes.Black;
            MouseLeftButtonDown += RotaryControl_MouseLeftButtonDown;
            MouseLeftButtonUp += RotaryControl_MouseLeftButtonUp;
            MouseMove += RotaryControl_MouseMove;

            CreateControl();
        }

        private Size MeasureString(string candidate, Label label) {
            var formattedText = new FormattedText(
                candidate,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch),
                label.FontSize,
                Brushes.Black,
                new NumberSubstitution(), TextFormattingMode.Display);

            return new Size(formattedText.Width, formattedText.Height);
        }

        Brush _fontBrush;
        System.Collections.Generic.List<Label> _labels;

        private void CreateControl() {
            // Remove everything apart from the ellipses 

            for (int i = _grid.Children.Count - 1; i >= 0; --i) {
                if (!(_grid.Children[i] is Ellipse)) {
                    _grid.Children.RemoveAt(i);
                }
            }

            /*
             * Draw marker lines and labels: always 5 minor ticks
             */

            double tickStart = _ellipseInnerDial.Width / 2 + 5;
            double majorTickLength = 10;
            double majorTickEnd = tickStart + majorTickLength;
            double minorTickLength = 5;
            double minorTickEnd = tickStart + minorTickLength;
            // The angle in radians subtended by adjacent major ticks
            double majorArc = (5 * System.Math.PI) / (3 * NumberOfMajorTicks);
            // The angle in radians subtended by adjacent minor ticks
            double minorArc = majorArc / 5;

            _labels = new System.Collections.Generic.List<Label>();

            // Angles are measured relative to 3 o'clock. We start at 7 which is 120 degrees
            double majorAngleInRadians = OneTwentyDegreesInRadians;

            for (int iMajor = 0; iMajor <= NumberOfMajorTicks; ++iMajor, majorAngleInRadians += majorArc) {

                // Major tick

                Polyline polyline = new Polyline();

                double cosineMajorAngle = System.Math.Cos(majorAngleInRadians);
                double sineMajorAngle = System.Math.Sin(majorAngleInRadians);

                double x = _ellipseInnerDial.Width + tickStart * cosineMajorAngle;
                double y = _ellipseInnerDial.Width + tickStart * sineMajorAngle;
                polyline.Points.Add(new Point(x, y));

                x = _ellipseInnerDial.Width + majorTickEnd * System.Math.Cos(majorAngleInRadians);
                y = _ellipseInnerDial.Width + majorTickEnd * System.Math.Sin(majorAngleInRadians);
                polyline.Points.Add(new Point(x, y));

                polyline.Stroke = System.Windows.Media.Brushes.White;
                polyline.StrokeThickness = 2;

                _grid.Children.Add(polyline);

                // Minor ticks

                if (iMajor != NumberOfMajorTicks) {
                    double minorAngleInRadians = majorAngleInRadians;
                    for (int iMinor = 1; iMinor < 5; ++iMinor) {
                        minorAngleInRadians += minorArc;

                        polyline = new Polyline();

                        x = _ellipseInnerDial.Width + tickStart * System.Math.Cos(minorAngleInRadians);
                        y = _ellipseInnerDial.Width + tickStart * System.Math.Sin(minorAngleInRadians);
                        polyline.Points.Add(new Point(x, y));

                        x = _ellipseInnerDial.Width + minorTickEnd * System.Math.Cos(minorAngleInRadians);
                        y = _ellipseInnerDial.Width + minorTickEnd * System.Math.Sin(minorAngleInRadians);
                        polyline.Points.Add(new Point(x, y));

                        polyline.Stroke = System.Windows.Media.Brushes.White;
                        polyline.StrokeThickness = 2;

                        _grid.Children.Add(polyline);
                    }
                }

                // Major tick label

                Label label = new Label();
                string text = (iMajor * MajorTickIncrement).ToString();
                label.Content = text;
                label.Foreground = _fontBrush;
                label.FontSize = 18;

                Size controlSize = MeasureString(text, label);

                x = ControlWidth / 2 - 0.8 * Math.Abs(controlSize.Width) + 0.6 * _ellipseOuterDial.Width * System.Math.Cos(majorAngleInRadians);
                y = (0.5 * ControlWidth - 0.7 * controlSize.Height) + 0.6 * _ellipseOuterDial.Width * System.Math.Sin(majorAngleInRadians);

                label.RenderTransform = new TranslateTransform(x, y);
                _grid.Children.Add(label);
                _labels.Add(label);
            }
        }

        public void PositionMarkerFromControlPosition(Point pointInControl) {
            double dX = pointInControl.X - 0.5 * _ellipseOuterDial.Width;
            double dY = pointInControl.Y - 0.5 * _ellipseOuterDial.Height;

            // The angle from the 12 o'clock position
            double angleInDegrees = -(180 * System.Math.Atan(dX / dY)) / System.Math.PI;
            if (dY > 0) {
                if (dX > 0) {
                    angleInDegrees += 180;
                } else {
                    angleInDegrees -= 180;
                }
            }

            // Add 150 because we measure angle from the 7 o'clock position
            Value = (NumberOfMajorTicks * MajorTickIncrement * (angleInDegrees + 150)) / 300;
        }

        public bool PositionMarker(double xPositionOnScreen, double yPositionOnScreen) {
            Point pointOnScreen = new Point(xPositionOnScreen, yPositionOnScreen);
            if (_ellipseOuterDial.InputHitTest(_ellipseOuterDial.PointFromScreen(pointOnScreen)) == null) {
                return false;
            }

            PositionMarkerFromControlPosition(_ellipseOuterDial.PointFromScreen(pointOnScreen));

            return true;
        }

        #region Value dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(Dial), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnValueChanged)));

        private const double constMinimumReading = 0;

        public event EventHandler<RoutedPropertyChangedEventArgs<Double>> ValueChanged;

        public double Value {
            get {
                return (double)GetValue(ValueProperty);
            }
            set {
                SetValue(ValueProperty, Math.Min(Math.Max(value, constMinimumReading), NumberOfMajorTicks * MajorTickIncrement));
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((Dial)d).OnValueChanged(e);
        }

        protected virtual void OnValueChanged(DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != null) {
                Value = (double)e.NewValue;
                if (ValueChanged != null)
                    ValueChanged(this, new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue));

                double majorAngleInRadians = OneTwentyDegreesInRadians + Value * (5 * System.Math.PI) / (3 * NumberOfMajorTicks * MajorTickIncrement);
                double x = 35 * System.Math.Cos(majorAngleInRadians);
                double y = 35 * System.Math.Sin(majorAngleInRadians);

                _markerTranslation.X = x;
                _markerTranslation.Y = y;
            }
        }

        #endregion

        #region NumberOfMajorTicks dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty NumberOfMajorTicksProperty = DependencyProperty.Register("NumberOfMajorTicks", typeof(int), typeof(Dial), new FrameworkPropertyMetadata(10, new PropertyChangedCallback(OnNumberOfMajorTicksChanged)));

        private const int constMinimumNumberOfMajorTicks = 3;
        private const int constMaximumNumberOfMajorTicks = 20;

        public int NumberOfMajorTicks {
            get {
                return (int)GetValue(NumberOfMajorTicksProperty);
            }
            set {
                SetValue(NumberOfMajorTicksProperty, Math.Min(Math.Max(value, constMinimumNumberOfMajorTicks), constMaximumNumberOfMajorTicks));
            }
        }

        private static void OnNumberOfMajorTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((Dial)d).OnNumberOfMajorTicksChanged(e);
        }

        protected virtual void OnNumberOfMajorTicksChanged(DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != null) {
                NumberOfMajorTicks = (int)e.NewValue;
                CreateControl();
            }
        }

        #endregion

        #region MajorTickIncrement dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty MajorTickIncrementProperty = DependencyProperty.Register("MajorTickIncrement", typeof(double), typeof(Dial), new FrameworkPropertyMetadata(10.0, new PropertyChangedCallback(OnMajorTickIncrementChanged)));

        private const double constMinimumMajorTickIncrement = 0.1;
        private const double constMaximumMajorTickIncrement = 10000;

        public double MajorTickIncrement {
            get {
                return (double)GetValue(MajorTickIncrementProperty);
            }
            set {
                SetValue(MajorTickIncrementProperty, Math.Min(Math.Max(value, constMinimumMajorTickIncrement), constMaximumMajorTickIncrement));
            }
        }

        private static void OnMajorTickIncrementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((Dial)d).OnMajorTickIncrementChanged(e);
        }

        protected virtual void OnMajorTickIncrementChanged(DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != null) {
                MajorTickIncrement = (double)e.NewValue;
                CreateControl();
            }
        }

        #endregion

        #region FontBrush dependency property

        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register("FontBrush", typeof(Brush), typeof(Dial), new FrameworkPropertyMetadata(System.Windows.Media.Brushes.Black, new PropertyChangedCallback(OnFontBrushChanged)));

        public Brush FontBrush {
            get {
                return (Brush)GetValue(FontBrushProperty);
            }
            set {
                SetValue(FontBrushProperty, value);
            }
        }

        private static void OnFontBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((Dial)d).OnFontBrushChanged(e);
        }

        protected virtual void OnFontBrushChanged(DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != null) {
                _fontBrush = e.NewValue as Brush;
                foreach (var label in _labels) {
                    label.Foreground = _fontBrush;
                }
            }
        }

        #endregion

        private void RotaryControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (_ellipseOuterDial.InputHitTest(e.GetPosition(_ellipseOuterDial)) == null) {
                return;
            }

            PositionMarkerFromControlPosition(e.GetPosition(_ellipseOuterDial));

            _ellipseOuterDial.CaptureMouse();
        }

        private void RotaryControl_MouseMove(object sender, MouseEventArgs e) {
            if (_ellipseOuterDial.IsMouseCaptured) {
                PositionMarkerFromControlPosition(e.GetPosition(_ellipseOuterDial));

                _ellipseOuterDial.CaptureMouse();
            }
        }

        private void RotaryControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (_ellipseOuterDial.IsMouseCaptured) {
                _ellipseOuterDial.ReleaseMouseCapture();
            }
        }
    }
}
