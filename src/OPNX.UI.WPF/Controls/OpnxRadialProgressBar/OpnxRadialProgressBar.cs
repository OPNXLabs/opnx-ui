using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxRadialProgressBar : ProgressBar
    {
        public static readonly DependencyProperty IndicatorThicknessProperty = DependencyProperty.Register(nameof(IndicatorThickness), typeof(double), typeof(OpnxRadialProgressBar), new FrameworkPropertyMetadata(6d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceNonNegative));

        public static readonly DependencyProperty StartAngleProperty = DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(OpnxRadialProgressBar), new FrameworkPropertyMetadata(-90d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(nameof(Direction), typeof(SweepDirection), typeof(OpnxRadialProgressBar), new FrameworkPropertyMetadata(SweepDirection.Clockwise, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IndeterminateArcLengthProperty = DependencyProperty.Register(nameof(IndeterminateArcLength), typeof(double), typeof(OpnxRadialProgressBar), new FrameworkPropertyMetadata(90d, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceArcLength));

        public static readonly DependencyProperty IndeterminateSpeedProperty = DependencyProperty.Register(nameof(IndeterminateSpeed), typeof(double), typeof(OpnxRadialProgressBar), new FrameworkPropertyMetadata(240d, FrameworkPropertyMetadataOptions.None, null, CoerceNonNegative));

        public static readonly DependencyProperty UseRoundedCapsProperty = DependencyProperty.Register(nameof(UseRoundedCaps), typeof(bool), typeof(OpnxRadialProgressBar), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        private bool _isRendering;
        private long _lastRenderingTimestamp;
        private double _indeterminateAngle;

        static OpnxRadialProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OpnxRadialProgressBar), new FrameworkPropertyMetadata(typeof(OpnxRadialProgressBar)));
            IsIndeterminateProperty.OverrideMetadata(typeof(OpnxRadialProgressBar), new FrameworkPropertyMetadata(false, OnIsIndeterminateChanged));
        }

        public OpnxRadialProgressBar()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            IsVisibleChanged += OnIsVisibleChanged;
        }

        public double IndicatorThickness
        {
            get => (double)GetValue(IndicatorThicknessProperty);
            set => SetValue(IndicatorThicknessProperty, value);
        }

        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        public SweepDirection Direction
        {
            get => (SweepDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public double IndeterminateArcLength
        {
            get => (double)GetValue(IndeterminateArcLengthProperty);
            set => SetValue(IndeterminateArcLengthProperty, value);
        }

        public double IndeterminateSpeed
        {
            get => (double)GetValue(IndeterminateSpeedProperty);
            set => SetValue(IndeterminateSpeedProperty, value);
        }

        public bool UseRoundedCaps
        {
            get => (bool)GetValue(UseRoundedCapsProperty);
            set => SetValue(UseRoundedCapsProperty, value);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double width = double.IsInfinity(constraint.Width) ? 32d : constraint.Width;
            double height = double.IsInfinity(constraint.Height) ? 32d : constraint.Height;
            double size = Math.Max(IndicatorThickness, Math.Min(width, height));
            return new Size(size, size);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double thickness = Math.Min(IndicatorThickness, Math.Min(ActualWidth, ActualHeight));
            double radius = Math.Max(0d, (Math.Min(ActualWidth, ActualHeight) - thickness) / 2d);
            if (thickness <= 0d || radius <= 0d)
                return;

            Point center = new(ActualWidth / 2d, ActualHeight / 2d);
            Pen trackPen = CreatePen(Background, thickness);
            Pen indicatorPen = CreatePen(Foreground, thickness);

            if (trackPen.Brush != null && trackPen.Brush.Opacity > 0d)
                drawingContext.DrawEllipse(null, trackPen, center, radius, radius);

            double sweepAngle;
            double startAngle;
            if (IsIndeterminate)
            {
                startAngle = StartAngle + _indeterminateAngle;
                sweepAngle = IndeterminateArcLength;
            }
            else
            {
                startAngle = StartAngle;
                double range = Maximum - Minimum;
                double progress = range <= 0d ? 0d : Math.Clamp((Value - Minimum) / range, 0d, 1d);
                sweepAngle = progress * 360d;
            }

            if (sweepAngle <= 0d || indicatorPen.Brush == null || indicatorPen.Brush.Opacity <= 0d)
                return;

            if (sweepAngle >= 359.999d)
            {
                drawingContext.DrawEllipse(null, indicatorPen, center, radius, radius);
                return;
            }

            drawingContext.DrawGeometry(null, indicatorPen, CreateArcGeometry(center, radius, startAngle, sweepAngle));
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            InvalidateVisual();
        }

        protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            base.OnMinimumChanged(oldMinimum, newMinimum);
            InvalidateVisual();
        }

        protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            InvalidateVisual();
        }

        [SuppressMessage("Performance", "CA1859:Use concrete types when possible", Justification = "WPF CoerceValueCallback requires an object return type.")]
        private static object CoerceNonNegative(DependencyObject d, object baseValue) => Math.Max(0d, (double)baseValue);

        [SuppressMessage("Performance", "CA1859:Use concrete types when possible", Justification = "WPF CoerceValueCallback requires an object return type.")]
        private static object CoerceArcLength(DependencyObject d, object baseValue) => Math.Clamp((double)baseValue, 0d, 359.999d);

        private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressBar = (OpnxRadialProgressBar)d;
            progressBar.UpdateRenderingSubscription();
            progressBar.InvalidateVisual();
        }

        private Pen CreatePen(Brush? brush, double thickness)
        {
            var pen = new Pen(brush, thickness)
            {
                StartLineCap = UseRoundedCaps ? PenLineCap.Round : PenLineCap.Flat,
                EndLineCap = UseRoundedCaps ? PenLineCap.Round : PenLineCap.Flat
            };

            if (pen.CanFreeze)
                pen.Freeze();

            return pen;
        }

        private StreamGeometry CreateArcGeometry(Point center, double radius, double startAngle, double sweepAngle)
        {
            double direction = Direction == SweepDirection.Clockwise ? 1d : -1d;
            Point startPoint = PointOnCircle(center, radius, startAngle);
            Point endPoint = PointOnCircle(center, radius, startAngle + (sweepAngle * direction));
            var geometry = new StreamGeometry();

            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(startPoint, false, false);
                context.ArcTo(endPoint, new Size(radius, radius), 0d, sweepAngle > 180d, Direction, true, false);
            }

            if (geometry.CanFreeze)
                geometry.Freeze();

            return geometry;
        }

        private static Point PointOnCircle(Point center, double radius, double angle)
        {
            double radians = angle * Math.PI / 180d;
            return new Point(center.X + (radius * Math.Cos(radians)), center.Y + (radius * Math.Sin(radians)));
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => UpdateRenderingSubscription();

        private void OnUnloaded(object sender, RoutedEventArgs e) => StopRendering();

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) => UpdateRenderingSubscription();

        private void UpdateRenderingSubscription()
        {
            if (IsLoaded && IsVisible && IsIndeterminate)
                StartRendering();
            else
                StopRendering();
        }

        private void StartRendering()
        {
            if (_isRendering)
                return;

            _lastRenderingTimestamp = Stopwatch.GetTimestamp();
            CompositionTarget.Rendering += OnRendering;
            _isRendering = true;
        }

        private void StopRendering()
        {
            if (!_isRendering)
                return;

            CompositionTarget.Rendering -= OnRendering;
            _isRendering = false;
            _lastRenderingTimestamp = 0;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            long timestamp = Stopwatch.GetTimestamp();
            double elapsedSeconds = Stopwatch.GetElapsedTime(_lastRenderingTimestamp, timestamp).TotalSeconds;
            _lastRenderingTimestamp = timestamp;
            _indeterminateAngle = (_indeterminateAngle + (elapsedSeconds * IndeterminateSpeed)) % 360d;
            InvalidateVisual();
        }
    }
}
