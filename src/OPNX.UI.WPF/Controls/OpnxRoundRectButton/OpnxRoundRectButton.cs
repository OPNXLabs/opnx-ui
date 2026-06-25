using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxRoundRectButton : Button
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(OpnxRoundRectButton),
                new PropertyMetadata(new CornerRadius(20), OnGeometryChanged));

        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.Register(nameof(ButtonWidth), typeof(double), typeof(OpnxRoundRectButton),
                new PropertyMetadata(120.0, OnGeometryChanged));

        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(OpnxRoundRectButton),
                new PropertyMetadata(60.0, OnGeometryChanged));

        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.Register(nameof(Geometry), typeof(Geometry), typeof(OpnxRoundRectButton));

        static OpnxRoundRectButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxRoundRectButton),
                new FrameworkPropertyMetadata(typeof(OpnxRoundRectButton)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public double ButtonWidth
        {
            get => (double)GetValue(ButtonWidthProperty);
            set => SetValue(ButtonWidthProperty, value);
        }

        public double ButtonHeight
        {
            get => (double)GetValue(ButtonHeightProperty);
            set => SetValue(ButtonHeightProperty, value);
        }

        public Geometry Geometry
        {
            get => (Geometry)GetValue(GeometryProperty);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateGeometry();
        }

        private static void OnGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxRoundRectButton button)
                button.UpdateGeometry();
        }

        private void UpdateGeometry()
        {
            var geometry = new RectangleGeometry(
                new Rect(0, 0, ButtonWidth, ButtonHeight),
                CornerRadius.TopLeft,
                CornerRadius.TopLeft);

            SetValue(GeometryProperty, geometry);

            Width = ButtonWidth;
            Height = ButtonHeight;
        }
    }
}
