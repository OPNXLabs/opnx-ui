using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxNavigatorItem : OpnxToggleButton
    {
        #region Fields
        private BadgeAdorner? _badgeAdorner;
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty IsBadgeVisibleProperty = DependencyProperty.Register(
            nameof(IsBadgeVisible),
            typeof(bool),
            typeof(OpnxNavigatorItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnBadgePropertyChanged));

        public static readonly DependencyProperty BadgeBrushProperty = DependencyProperty.Register(
            nameof(BadgeBrush),
            typeof(Brush),
            typeof(OpnxNavigatorItem),
            new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender, OnBadgePropertyChanged));

        public static readonly DependencyProperty BadgeSizeProperty = DependencyProperty.Register(
            nameof(BadgeSize),
            typeof(double),
            typeof(OpnxNavigatorItem),
            new FrameworkPropertyMetadata(8d, FrameworkPropertyMetadataOptions.AffectsRender, OnBadgePropertyChanged),
            IsValidBadgeSize);

        public static readonly DependencyProperty BadgeHorizontalAlignmentProperty = DependencyProperty.Register(
            nameof(BadgeHorizontalAlignment),
            typeof(HorizontalAlignment),
            typeof(OpnxNavigatorItem),
            new FrameworkPropertyMetadata(HorizontalAlignment.Right, FrameworkPropertyMetadataOptions.AffectsRender, OnBadgePropertyChanged));

        public static readonly DependencyProperty BadgeVerticalAlignmentProperty = DependencyProperty.Register(
            nameof(BadgeVerticalAlignment),
            typeof(VerticalAlignment),
            typeof(OpnxNavigatorItem),
            new FrameworkPropertyMetadata(VerticalAlignment.Top, FrameworkPropertyMetadataOptions.AffectsRender, OnBadgePropertyChanged));

        public static readonly DependencyProperty BadgeMarginProperty = DependencyProperty.Register(
            nameof(BadgeMargin),
            typeof(Thickness),
            typeof(OpnxNavigatorItem),
            new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsRender, OnBadgePropertyChanged));
        #endregion

        #region Constructors
        public OpnxNavigatorItem()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
        }
        #endregion

        #region Properties
        [Bindable(true), Category("Badge")]
        public bool IsBadgeVisible
        {
            get => (bool)GetValue(IsBadgeVisibleProperty);
            set => SetValue(IsBadgeVisibleProperty, value);
        }

        [Bindable(true), Category("Badge")]
        public Brush BadgeBrush
        {
            get => (Brush)GetValue(BadgeBrushProperty);
            set => SetValue(BadgeBrushProperty, value);
        }

        [Bindable(true), Category("Badge")]
        public double BadgeSize
        {
            get => (double)GetValue(BadgeSizeProperty);
            set => SetValue(BadgeSizeProperty, value);
        }

        [Bindable(true), Category("Badge")]
        public HorizontalAlignment BadgeHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(BadgeHorizontalAlignmentProperty);
            set => SetValue(BadgeHorizontalAlignmentProperty, value);
        }

        [Bindable(true), Category("Badge")]
        public VerticalAlignment BadgeVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(BadgeVerticalAlignmentProperty);
            set => SetValue(BadgeVerticalAlignmentProperty, value);
        }

        [Bindable(true), Category("Badge")]
        public Thickness BadgeMargin
        {
            get => (Thickness)GetValue(BadgeMarginProperty);
            set => SetValue(BadgeMarginProperty, value);
        }
        #endregion

        #region Private Methods
        private static bool IsValidBadgeSize(object value) => value is double size && double.IsFinite(size) && size >= 0;

        private static void OnBadgePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxNavigatorItem item)
                item.UpdateBadge();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => _ = Dispatcher.BeginInvoke(DispatcherPriority.Loaded, AttachBadgeAdorner);

        private void OnUnloaded(object sender, RoutedEventArgs e) => DetachBadgeAdorner();

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) => _badgeAdorner?.InvalidateVisual();

        private void AttachBadgeAdorner()
        {
            if (_badgeAdorner != null || !IsLoaded)
                return;

            AdornerLayer? adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer == null)
                return;

            _badgeAdorner = new BadgeAdorner(this);
            adornerLayer.Add(_badgeAdorner);
            UpdateBadge();
        }

        private void DetachBadgeAdorner()
        {
            if (_badgeAdorner == null)
                return;

            AdornerLayer.GetAdornerLayer(this)?.Remove(_badgeAdorner);
            _badgeAdorner = null;
        }

        private void UpdateBadge()
        {
            if (_badgeAdorner == null && IsLoaded)
                AttachBadgeAdorner();

            if (_badgeAdorner == null)
                return;

            _badgeAdorner.Visibility = IsBadgeVisible ? Visibility.Visible : Visibility.Collapsed;
            _badgeAdorner.InvalidateVisual();
        }
        #endregion

        #region Nested Types
        private sealed class BadgeAdorner : Adorner
        {
            public BadgeAdorner(OpnxNavigatorItem adornedElement) : base(adornedElement)
            {
                IsHitTestVisible = false;
            }

            private OpnxNavigatorItem Item => (OpnxNavigatorItem)AdornedElement;

            protected override void OnRender(DrawingContext drawingContext)
            {
                double size = Item.BadgeSize;
                if (size <= 0 || Item.BadgeBrush == null)
                    return;

                Thickness margin = Item.BadgeMargin;
                double x = GetHorizontalPosition(size, margin);
                double y = GetVerticalPosition(size, margin);
                drawingContext.DrawEllipse(Item.BadgeBrush, null, new Point(x + size / 2, y + size / 2), size / 2, size / 2);
            }

            private double GetHorizontalPosition(double size, Thickness margin) => Item.BadgeHorizontalAlignment switch
            {
                HorizontalAlignment.Left => margin.Left,
                HorizontalAlignment.Center or HorizontalAlignment.Stretch => (ActualWidth - size + margin.Left - margin.Right) / 2,
                _ => ActualWidth - size - margin.Right
            };

            private double GetVerticalPosition(double size, Thickness margin) => Item.BadgeVerticalAlignment switch
            {
                VerticalAlignment.Top => margin.Top,
                VerticalAlignment.Center or VerticalAlignment.Stretch => (ActualHeight - size + margin.Top - margin.Bottom) / 2,
                _ => ActualHeight - size - margin.Bottom
            };
        }
        #endregion
    }
}
