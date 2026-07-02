using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace OPNX.UI.WPF.Controls
{
    public partial class OpnxImageViewer : UserControl
    {
        private const double ZoomStep = 0.1d;

        private Point _dragStartPoint;
        private Point _dragStartOffset;
        private bool _isDragging;

        public OpnxImageViewer()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                nameof(ImageSource),
                typeof(BitmapSource),
                typeof(OpnxImageViewer),
                new PropertyMetadata(null, OnImageSourceChanged));

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(
                nameof(Scale),
                typeof(double),
                typeof(OpnxImageViewer),
                new PropertyMetadata(1d));

        public static readonly DependencyProperty RotationAngleProperty =
            DependencyProperty.Register(
                nameof(RotationAngle),
                typeof(double),
                typeof(OpnxImageViewer),
                new PropertyMetadata(0d));

        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.Register(
                nameof(MinScale),
                typeof(double),
                typeof(OpnxImageViewer),
                new PropertyMetadata(0.05d));

        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.Register(
                nameof(MaxScale),
                typeof(double),
                typeof(OpnxImageViewer),
                new PropertyMetadata(8d));

        public BitmapSource? ImageSource
        {
            get => (BitmapSource?)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, CoerceScale(value));
        }

        public double RotationAngle
        {
            get => (double)GetValue(RotationAngleProperty);
            set => SetValue(RotationAngleProperty, NormalizeAngle(value));
        }

        public double MinScale
        {
            get => (double)GetValue(MinScaleProperty);
            set => SetValue(MinScaleProperty, value);
        }

        public double MaxScale
        {
            get => (double)GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, value);
        }

        public void ZoomIn()
        {
            ZoomAtCenter(Scale + ZoomStep);
        }

        public void ZoomOut()
        {
            ZoomAtCenter(Scale - ZoomStep);
        }

        public void ZoomOriginal()
        {
            ZoomAtCenter(1d);
        }

        public void ZoomToFit()
        {
            if (ImageSource == null)
                return;

            var viewportWidth = scrollHost.ViewportWidth > 0 ? scrollHost.ViewportWidth : ActualWidth;
            var viewportHeight = scrollHost.ViewportHeight > 0 ? scrollHost.ViewportHeight : ActualHeight;

            if (viewportWidth <= 0 || viewportHeight <= 0)
                return;

            var imageWidth = ImageSource.PixelWidth;
            var imageHeight = ImageSource.PixelHeight;

            if (IsQuarterTurn())
                (imageWidth, imageHeight) = (imageHeight, imageWidth);

            if (imageWidth <= 0 || imageHeight <= 0)
                return;

            var scaleX = viewportWidth / imageWidth;
            var scaleY = viewportHeight / imageHeight;
            ZoomAtCenter(Math.Min(scaleX, scaleY));
        }

        public void RotateLeft()
        {
            RotationAngle -= 90d;
            UpdateImageHostSize();
        }

        public void RotateRight()
        {
            RotationAngle += 90d;
            UpdateImageHostSize();
        }

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not OpnxImageViewer viewer)
                return;

            viewer.Scale = 1d;
            viewer.RotationAngle = 0d;
            viewer.Dispatcher.BeginInvoke(viewer.ZoomToFit);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateImageHostSize();
            ZoomToFit();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateImageHostSize();
        }

        private void ScrollHost_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ImageSource == null)
                return;

            var nextScale = e.Delta > 0
                ? Scale + ZoomStep
                : Scale - ZoomStep;

            ZoomAt(e.GetPosition(imageHost), nextScale);
            e.Handled = true;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (scrollHost.ScrollableWidth <= 0 && scrollHost.ScrollableHeight <= 0)
                return;

            _isDragging = true;
            _dragStartPoint = e.GetPosition(scrollHost);
            _dragStartOffset = new Point(scrollHost.HorizontalOffset, scrollHost.VerticalOffset);
            image.CaptureMouse();
            image.Cursor = Cursors.Hand;
            e.Handled = true;
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging)
                return;

            var position = e.GetPosition(scrollHost);
            var delta = _dragStartPoint - position;

            scrollHost.ScrollToHorizontalOffset(_dragStartOffset.X + delta.X);
            scrollHost.ScrollToVerticalOffset(_dragStartOffset.Y + delta.Y);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopDragging();
        }

        private void Image_LostMouseCapture(object sender, MouseEventArgs e)
        {
            StopDragging();
        }

        private void StopDragging()
        {
            if (!_isDragging)
                return;

            _isDragging = false;
            image.ReleaseMouseCapture();
            image.Cursor = Cursors.Arrow;
        }

        private void ZoomAtCenter(double scale)
        {
            ZoomAt(new Point(scrollHost.HorizontalOffset + scrollHost.ViewportWidth / 2d, scrollHost.VerticalOffset + scrollHost.ViewportHeight / 2d), scale);
        }

        private void ZoomAt(Point contentPoint, double scale)
        {
            var oldScale = Scale;
            var nextScale = CoerceScale(scale);

            if (Math.Abs(oldScale - nextScale) < 0.0001d)
                return;

            var horizontalRatio = scrollHost.ViewportWidth > 0
                ? (contentPoint.X + scrollHost.HorizontalOffset) / oldScale
                : 0d;
            var verticalRatio = scrollHost.ViewportHeight > 0
                ? (contentPoint.Y + scrollHost.VerticalOffset) / oldScale
                : 0d;

            Scale = nextScale;

            Dispatcher.BeginInvoke(() =>
            {
                scrollHost.ScrollToHorizontalOffset(horizontalRatio * nextScale - contentPoint.X);
                scrollHost.ScrollToVerticalOffset(verticalRatio * nextScale - contentPoint.Y);
                UpdateImageHostSize();
            });
        }

        private double CoerceScale(double scale)
        {
            return Math.Clamp(scale, MinScale, MaxScale);
        }

        private static double NormalizeAngle(double angle)
        {
            var normalized = angle % 360d;
            return normalized < 0d ? normalized + 360d : normalized;
        }

        private bool IsQuarterTurn()
        {
            return Math.Abs(RotationAngle - 90d) < 0.0001d ||
                   Math.Abs(RotationAngle - 270d) < 0.0001d;
        }

        private void UpdateImageHostSize()
        {
            imageHost.MinWidth = Math.Max(0d, scrollHost.ViewportWidth);
            imageHost.MinHeight = Math.Max(0d, scrollHost.ViewportHeight);
        }
    }
}
