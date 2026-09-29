using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxRadioButton : RadioButton
    {
        public static readonly DependencyProperty MouseOverOpacityProperty = DependencyProperty.Register(
            nameof(MouseOverOpacity),
            typeof(double),
            typeof(OpnxRadioButton),
            new PropertyMetadata(0.7d));

        public static readonly DependencyProperty IndicatorWidthProperty = DependencyProperty.Register(
            nameof(IndicatorWidth),
            typeof(double),
            typeof(OpnxRadioButton),
            new PropertyMetadata(16.0d));

        public static readonly DependencyProperty IndicatorHeightProperty = DependencyProperty.Register(
            nameof(IndicatorHeight),
            typeof(double),
            typeof(OpnxRadioButton),
            new PropertyMetadata(16.0d));

        public static readonly DependencyProperty IndicatorBorderThicknessProperty = DependencyProperty.Register(
            nameof(IndicatorBorderThickness),
            typeof(double),
            typeof(OpnxRadioButton),
            new PropertyMetadata(1.0d));

        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(
            nameof(ContentMargin),
            typeof(Thickness),
            typeof(OpnxRadioButton),
            new PropertyMetadata(new Thickness(8, 0, 0, 0)));

        public static readonly DependencyProperty CheckedBackgroundProperty = DependencyProperty.Register(
            nameof(CheckedBackground),
            typeof(Brush),
            typeof(OpnxRadioButton),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x4A, 0x6F, 0xE3))));

        public static readonly DependencyProperty CheckedForegroundProperty = DependencyProperty.Register(
            nameof(CheckedForeground),
            typeof(Brush),
            typeof(OpnxRadioButton),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty CheckedBorderBrushProperty = DependencyProperty.Register(
            nameof(CheckedBorderBrush),
            typeof(Brush),
            typeof(OpnxRadioButton),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x4A, 0x6F, 0xE3))));

        public static readonly DependencyProperty DisabledOpacityProperty = DependencyProperty.Register(
            nameof(DisabledOpacity),
            typeof(double),
            typeof(OpnxRadioButton),
            new PropertyMetadata(0.5d));

        static OpnxRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxRadioButton),
                new FrameworkPropertyMetadata(typeof(OpnxRadioButton)));
        }

        [Bindable(true), Category("Appearance")]
        public double MouseOverOpacity
        {
            get => (double)GetValue(MouseOverOpacityProperty);
            set => SetValue(MouseOverOpacityProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public double IndicatorWidth
        {
            get => (double)GetValue(IndicatorWidthProperty);
            set => SetValue(IndicatorWidthProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public double IndicatorHeight
        {
            get => (double)GetValue(IndicatorHeightProperty);
            set => SetValue(IndicatorHeightProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public double IndicatorBorderThickness
        {
            get => (double)GetValue(IndicatorBorderThicknessProperty);
            set => SetValue(IndicatorBorderThicknessProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Thickness ContentMargin
        {
            get => (Thickness)GetValue(ContentMarginProperty);
            set => SetValue(ContentMarginProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush CheckedBackground
        {
            get => (Brush)GetValue(CheckedBackgroundProperty);
            set => SetValue(CheckedBackgroundProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush CheckedForeground
        {
            get => (Brush)GetValue(CheckedForegroundProperty);
            set => SetValue(CheckedForegroundProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush CheckedBorderBrush
        {
            get => (Brush)GetValue(CheckedBorderBrushProperty);
            set => SetValue(CheckedBorderBrushProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public double DisabledOpacity
        {
            get => (double)GetValue(DisabledOpacityProperty);
            set => SetValue(DisabledOpacityProperty, value);
        }
    }
}
