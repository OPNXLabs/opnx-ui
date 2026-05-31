using System.Windows;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxCheckBox : System.Windows.Controls.CheckBox
    {
        public static readonly DependencyProperty CheckBoxWidthProperty = DependencyProperty.Register(
            nameof(CheckBoxWidth),
            typeof(double),
            typeof(OpnxCheckBox),
            new PropertyMetadata(20.0d));

        public static readonly DependencyProperty CheckBoxHeightProperty = DependencyProperty.Register(
            nameof(CheckBoxHeight),
            typeof(double),
            typeof(OpnxCheckBox),
            new PropertyMetadata(20.0d));

        public static readonly DependencyProperty CheckBoxIConWidthProperty = DependencyProperty.Register(
            nameof(CheckBoxIConWidth),
            typeof(double),
            typeof(OpnxCheckBox),
            new PropertyMetadata(15.0d));

        public static readonly DependencyProperty CheckBoxIConHeightProperty = DependencyProperty.Register(
            nameof(CheckBoxIConHeight),
            typeof(double),
            typeof(OpnxCheckBox),
            new PropertyMetadata(15.0d));

        public static readonly DependencyProperty CheckBoxIConForegroundProperty = DependencyProperty.Register(
            nameof(CheckBoxIConForeground),
            typeof(Brush),
            typeof(OpnxCheckBox),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x16, 0xab, 0xbd))));

        public static readonly DependencyProperty TextMarginProperty = DependencyProperty.Register(
            nameof(TextMargin),
            typeof(Thickness),
            typeof(OpnxCheckBox),
            new PropertyMetadata(new Thickness()));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(OpnxCheckBox),
            new PropertyMetadata(new CornerRadius(0)));

        static OpnxCheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxCheckBox),
                new FrameworkPropertyMetadata(typeof(OpnxCheckBox)));
        }

        public double CheckBoxWidth
        {
            get => (double)GetValue(CheckBoxWidthProperty);
            set => SetValue(CheckBoxWidthProperty, value);
        }

        public double CheckBoxHeight
        {
            get => (double)GetValue(CheckBoxHeightProperty);
            set => SetValue(CheckBoxHeightProperty, value);
        }

        public double CheckBoxIConWidth
        {
            get => (double)GetValue(CheckBoxIConWidthProperty);
            set => SetValue(CheckBoxIConWidthProperty, value);
        }

        public double CheckBoxIConHeight
        {
            get => (double)GetValue(CheckBoxIConHeightProperty);
            set => SetValue(CheckBoxIConHeightProperty, value);
        }

        public Brush CheckBoxIConForeground
        {
            get => (Brush)GetValue(CheckBoxIConForegroundProperty);
            set => SetValue(CheckBoxIConForegroundProperty, value);
        }

        public Thickness TextMargin
        {
            get => (Thickness)GetValue(TextMarginProperty);
            set => SetValue(TextMarginProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
