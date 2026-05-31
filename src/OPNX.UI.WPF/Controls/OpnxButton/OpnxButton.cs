using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxButton : System.Windows.Controls.Button
    {
        static OpnxButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxButton),
                new FrameworkPropertyMetadata(typeof(OpnxButton)));
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(OpnxButton),
            new PropertyMetadata(new CornerRadius(0)));

        public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.Register(
            nameof(MouseOverBackground),
            typeof(Brush),
            typeof(OpnxButton),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffc000"))));

        public static readonly DependencyProperty MouseOverBorderBrushProperty = DependencyProperty.Register(
            nameof(MouseOverBorderBrush),
            typeof(Brush),
            typeof(OpnxButton),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffc000"))));

        public static readonly DependencyProperty MouseOverForegroundProperty = DependencyProperty.Register(
            nameof(MouseOverForeground),
            typeof(Brush),
            typeof(OpnxButton),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
            nameof(PressedBackground),
            typeof(Brush),
            typeof(OpnxButton),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1495a3"))));

        public static readonly DependencyProperty PressedBorderBrushProperty = DependencyProperty.Register(
            nameof(PressedBorderBrush),
            typeof(Brush),
            typeof(OpnxButton),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1495a3"))));

        public static readonly DependencyProperty PressedForegroundProperty = DependencyProperty.Register(
            nameof(PressedForeground),
            typeof(Brush),
            typeof(OpnxButton),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty MouseOverOpacityProperty = DependencyProperty.Register(
            nameof(MouseOverOpacity),
            typeof(double),
            typeof(OpnxButton),
            new PropertyMetadata(1.0d));

        public static readonly DependencyProperty PressedOpacityProperty = DependencyProperty.Register(
            nameof(PressedOpacity),
            typeof(double),
            typeof(OpnxButton),
            new PropertyMetadata(1.0d));

        public static readonly DependencyProperty DisabledOpacityProperty = DependencyProperty.Register(
            nameof(DisabledOpacity),
            typeof(double),
            typeof(OpnxButton),
            new PropertyMetadata(0.5d));

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
        public double DisabledOpacity
        {
            get => (double)GetValue(DisabledOpacityProperty);
            set => SetValue(DisabledOpacityProperty, value);
        }
    }
}
