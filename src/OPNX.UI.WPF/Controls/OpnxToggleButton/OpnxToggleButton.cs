using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxToggleButton : ToggleButton
    {
        static OpnxToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxToggleButton),
                new FrameworkPropertyMetadata(typeof(OpnxToggleButton)));
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(OpnxToggleButton),
            new PropertyMetadata(new CornerRadius(0)));

        public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.Register(
            nameof(MouseOverBackground),
            typeof(Brush),
            typeof(OpnxToggleButton),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffc000"))));

        public static readonly DependencyProperty MouseOverBorderBrushProperty = DependencyProperty.Register(
            nameof(MouseOverBorderBrush),
            typeof(Brush),
            typeof(OpnxToggleButton),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffc000"))));

        public static readonly DependencyProperty MouseOverForegroundProperty = DependencyProperty.Register(
            nameof(MouseOverForeground),
            typeof(Brush),
            typeof(OpnxToggleButton),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
            nameof(PressedBackground),
            typeof(Brush),
            typeof(OpnxToggleButton),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1495a3"))));

        public static readonly DependencyProperty PressedBorderBrushProperty = DependencyProperty.Register(
            nameof(PressedBorderBrush),
            typeof(Brush),
            typeof(OpnxToggleButton),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1495a3"))));

        public static readonly DependencyProperty PressedForegroundProperty = DependencyProperty.Register(
            nameof(PressedForeground),
            typeof(Brush),
            typeof(OpnxToggleButton),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty CheckedBackgroundProperty = DependencyProperty.Register(
            nameof(CheckedBackground),
            typeof(Brush),
            typeof(OpnxToggleButton),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1495a3"))));

        public static readonly DependencyProperty CheckedBorderBrushProperty = DependencyProperty.Register(
            nameof(CheckedBorderBrush),
            typeof(Brush),
            typeof(OpnxToggleButton),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1495a3"))));

        public static readonly DependencyProperty CheckedForegroundProperty = DependencyProperty.Register(
            nameof(CheckedForeground),
            typeof(Brush),
            typeof(OpnxToggleButton),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty MouseOverOpacityProperty = DependencyProperty.Register(
            nameof(MouseOverOpacity),
            typeof(double),
            typeof(OpnxToggleButton),
            new PropertyMetadata(1.0d));

        public static readonly DependencyProperty PressedOpacityProperty = DependencyProperty.Register(
            nameof(PressedOpacity),
            typeof(double),
            typeof(OpnxToggleButton),
            new PropertyMetadata(1.0d));

        public static readonly DependencyProperty CheckedOpacityProperty = DependencyProperty.Register(
            nameof(CheckedOpacity),
            typeof(double),
            typeof(OpnxToggleButton),
            new PropertyMetadata(1.0d));

        public static readonly DependencyProperty DisabledOpacityProperty = DependencyProperty.Register(
            nameof(DisabledOpacity),
            typeof(double),
            typeof(OpnxToggleButton),
            new PropertyMetadata(0.5d));

        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(OpnxToggleButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(
            nameof(IconWidth),
            typeof(double),
            typeof(OpnxToggleButton),
            new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(
            nameof(IconHeight),
            typeof(double),
            typeof(OpnxToggleButton),
            new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register(
            nameof(IconMargin),
            typeof(Thickness),
            typeof(OpnxToggleButton),
            new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty IconStretchProperty = DependencyProperty.Register(
            nameof(IconStretch),
            typeof(Stretch),
            typeof(OpnxToggleButton),
            new PropertyMetadata(Stretch.Uniform));

        [Bindable(true), Category("Appearance")]
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush MouseOverBackground
        {
            get => (Brush)GetValue(MouseOverBackgroundProperty);
            set => SetValue(MouseOverBackgroundProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush MouseOverBorderBrush
        {
            get => (Brush)GetValue(MouseOverBorderBrushProperty);
            set => SetValue(MouseOverBorderBrushProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush MouseOverForeground
        {
            get => (Brush)GetValue(MouseOverForegroundProperty);
            set => SetValue(MouseOverForegroundProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush PressedBorderBrush
        {
            get => (Brush)GetValue(PressedBorderBrushProperty);
            set => SetValue(PressedBorderBrushProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush PressedForeground
        {
            get => (Brush)GetValue(PressedForegroundProperty);
            set => SetValue(PressedForegroundProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush CheckedBackground
        {
            get => (Brush)GetValue(CheckedBackgroundProperty);
            set => SetValue(CheckedBackgroundProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush CheckedBorderBrush
        {
            get => (Brush)GetValue(CheckedBorderBrushProperty);
            set => SetValue(CheckedBorderBrushProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush CheckedForeground
        {
            get => (Brush)GetValue(CheckedForegroundProperty);
            set => SetValue(CheckedForegroundProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public double MouseOverOpacity
        {
            get => (double)GetValue(MouseOverOpacityProperty);
            set => SetValue(MouseOverOpacityProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public double PressedOpacity
        {
            get => (double)GetValue(PressedOpacityProperty);
            set => SetValue(PressedOpacityProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public double CheckedOpacity
        {
            get => (double)GetValue(CheckedOpacityProperty);
            set => SetValue(CheckedOpacityProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public double DisabledOpacity
        {
            get => (double)GetValue(DisabledOpacityProperty);
            set => SetValue(DisabledOpacityProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public ImageSource? IconSource
        {
            get => (ImageSource?)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public double IconWidth
        {
            get => (double)GetValue(IconWidthProperty);
            set => SetValue(IconWidthProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public double IconHeight
        {
            get => (double)GetValue(IconHeightProperty);
            set => SetValue(IconHeightProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Thickness IconMargin
        {
            get => (Thickness)GetValue(IconMarginProperty);
            set => SetValue(IconMarginProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Stretch IconStretch
        {
            get => (Stretch)GetValue(IconStretchProperty);
            set => SetValue(IconStretchProperty, value);
        }
    }
}
