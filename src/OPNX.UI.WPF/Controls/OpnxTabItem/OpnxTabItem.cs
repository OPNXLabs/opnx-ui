using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class OpnxTabItem : TabItem
    {
        public static readonly DependencyProperty TabHeaderWidthProperty = DependencyProperty.Register(
            nameof(TabHeaderWidth),
            typeof(double),
            typeof(OpnxTabItem),
            new PropertyMetadata(100d));

        public static readonly DependencyProperty TabHeaderHeightProperty = DependencyProperty.Register(
            nameof(TabHeaderHeight),
            typeof(double),
            typeof(OpnxTabItem),
            new PropertyMetadata(30d));

        public static readonly DependencyProperty HorizontalHeaderContentAlignmentProperty = DependencyProperty.Register(
            nameof(HorizontalHeaderContentAlignment),
            typeof(HorizontalAlignment),
            typeof(OpnxTabItem),
            new PropertyMetadata(HorizontalAlignment.Center));

        public static readonly DependencyProperty VerticalHeaderContentAlignmentProperty = DependencyProperty.Register(
            nameof(VerticalHeaderContentAlignment),
            typeof(VerticalAlignment),
            typeof(OpnxTabItem),
            new PropertyMetadata(VerticalAlignment.Center));

        public static readonly DependencyProperty MouseOverForegroundProperty = DependencyProperty.Register(
            nameof(MouseOverForeground),
            typeof(Brush),
            typeof(OpnxTabItem),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty MouseOverBackgroundProperty = DependencyProperty.Register(
            nameof(MouseOverBackground),
            typeof(Brush),
            typeof(OpnxTabItem),
            new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty SelectedForegroundProperty = DependencyProperty.Register(
            nameof(SelectedForeground),
            typeof(Brush),
            typeof(OpnxTabItem),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty SelectedBackgroundProperty = DependencyProperty.Register(
            nameof(SelectedBackground),
            typeof(Brush),
            typeof(OpnxTabItem),
            new PropertyMetadata(Brushes.White));

        static OpnxTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxTabItem),
                new FrameworkPropertyMetadata(typeof(OpnxTabItem)));
        }

        public double TabHeaderWidth
        {
            get => (double)GetValue(TabHeaderWidthProperty);
            set => SetValue(TabHeaderWidthProperty, value);
        }

        public double TabHeaderHeight
        {
            get => (double)GetValue(TabHeaderHeightProperty);
            set => SetValue(TabHeaderHeightProperty, value);
        }

        public HorizontalAlignment HorizontalHeaderContentAlignment
        {
            get => (HorizontalAlignment)GetValue(HorizontalHeaderContentAlignmentProperty);
            set => SetValue(HorizontalHeaderContentAlignmentProperty, value);
        }

        public VerticalAlignment VerticalHeaderContentAlignment
        {
            get => (VerticalAlignment)GetValue(VerticalHeaderContentAlignmentProperty);
            set => SetValue(VerticalHeaderContentAlignmentProperty, value);
        }

        public Brush MouseOverForeground
        {
            get => (Brush)GetValue(MouseOverForegroundProperty);
            set => SetValue(MouseOverForegroundProperty, value);
        }

        public Brush MouseOverBackground
        {
            get => (Brush)GetValue(MouseOverBackgroundProperty);
            set => SetValue(MouseOverBackgroundProperty, value);
        }

        public Brush SelectedForeground
        {
            get => (Brush)GetValue(SelectedForegroundProperty);
            set => SetValue(SelectedForegroundProperty, value);
        }

        public Brush SelectedBackground
        {
            get => (Brush)GetValue(SelectedBackgroundProperty);
            set => SetValue(SelectedBackgroundProperty, value);
        }
    }
}
