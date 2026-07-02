using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public partial class OpnxDatePicker : UserControl
    {
        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime),
            typeof(OpnxDatePicker),
            new FrameworkPropertyMetadata(
                DateTime.Today,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedDateChanged));

        public static readonly DependencyProperty DisplayFormatProperty = DependencyProperty.Register(
            nameof(DisplayFormat),
            typeof(string),
            typeof(OpnxDatePicker),
            new PropertyMetadata("yyyy-MM-dd", OnDisplayFormatChanged));

        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
            nameof(IconBrush),
            typeof(Brush),
            typeof(OpnxDatePicker),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA))));

        public static readonly DependencyProperty CalendarBackgroundProperty = DependencyProperty.Register(
            nameof(CalendarBackground),
            typeof(Brush),
            typeof(OpnxDatePicker),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty CalendarBorderBrushProperty = DependencyProperty.Register(
            nameof(CalendarBorderBrush),
            typeof(Brush),
            typeof(OpnxDatePicker),
            new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty StaysOpenProperty = DependencyProperty.Register(
            nameof(StaysOpen),
            typeof(bool),
            typeof(OpnxDatePicker),
            new PropertyMetadata(false));

        public static readonly DependencyProperty CalendarButtonContentProperty = DependencyProperty.Register(
            nameof(CalendarButtonContent),
            typeof(object),
            typeof(OpnxDatePicker),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CalendarButtonContentTemplateProperty = DependencyProperty.Register(
            nameof(CalendarButtonContentTemplate),
            typeof(DataTemplate),
            typeof(OpnxDatePicker),
            new PropertyMetadata(null));

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register(
            nameof(DisplayText),
            typeof(string),
            typeof(OpnxDatePicker),
            new PropertyMetadata(string.Empty));

        public OpnxDatePicker()
        {
            InitializeComponent();
            UpdateDisplayText();
        }

        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public string DisplayFormat
        {
            get => (string)GetValue(DisplayFormatProperty);
            set => SetValue(DisplayFormatProperty, value);
        }

        public Brush IconBrush
        {
            get => (Brush)GetValue(IconBrushProperty);
            set => SetValue(IconBrushProperty, value);
        }

        public Brush CalendarBackground
        {
            get => (Brush)GetValue(CalendarBackgroundProperty);
            set => SetValue(CalendarBackgroundProperty, value);
        }

        public Brush CalendarBorderBrush
        {
            get => (Brush)GetValue(CalendarBorderBrushProperty);
            set => SetValue(CalendarBorderBrushProperty, value);
        }

        public bool StaysOpen
        {
            get => (bool)GetValue(StaysOpenProperty);
            set => SetValue(StaysOpenProperty, value);
        }

        public object? CalendarButtonContent
        {
            get => GetValue(CalendarButtonContentProperty);
            set => SetValue(CalendarButtonContentProperty, value);
        }

        public DataTemplate? CalendarButtonContentTemplate
        {
            get => (DataTemplate?)GetValue(CalendarButtonContentTemplateProperty);
            set => SetValue(CalendarButtonContentTemplateProperty, value);
        }

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            private set => SetValue(DisplayTextProperty, value);
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxDatePicker control)
                control.UpdateDisplayText();
        }

        private static void OnDisplayFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxDatePicker control)
                control.UpdateDisplayText();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PART_CalendarPopup.IsOpen = true;
        }

        private void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (!StaysOpen && PART_CalendarPopup.IsOpen)
                PART_CalendarPopup.IsOpen = false;
        }

        private void UpdateDisplayText()
        {
            DisplayText = string.IsNullOrWhiteSpace(DisplayFormat)
                ? SelectedDate.ToString(CultureInfo.CurrentCulture)
                : SelectedDate.ToString(DisplayFormat, CultureInfo.CurrentCulture);
        }
    }
}
