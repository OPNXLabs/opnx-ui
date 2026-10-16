using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public partial class OpnxTimePicker : UserControl
    {
        private static readonly TimeSpan LastTimeOfDay = new(23, 59, 59);

        public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
            nameof(SelectedTime), typeof(TimeSpan), typeof(OpnxTimePicker),
            new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeChanged, CoerceSelectedTime));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(TimeSpan), typeof(OpnxTimePicker),
            new PropertyMetadata(TimeSpan.Zero, OnRangeChanged, CoerceMinimum));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(TimeSpan), typeof(OpnxTimePicker),
            new PropertyMetadata(LastTimeOfDay, OnRangeChanged, CoerceMaximum));

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            nameof(IsDropDownOpen), typeof(bool), typeof(OpnxTimePicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty StaysOpenProperty = DependencyProperty.Register(
            nameof(StaysOpen), typeof(bool), typeof(OpnxTimePicker), new PropertyMetadata(false));

        public static readonly DependencyProperty DropDownButtonVisibilityProperty = DependencyProperty.Register(
            nameof(DropDownButtonVisibility), typeof(Visibility), typeof(OpnxTimePicker),
            new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty DropDownButtonWidthProperty = DependencyProperty.Register(
            nameof(DropDownButtonWidth), typeof(double), typeof(OpnxTimePicker),
            new PropertyMetadata(24d));

        public static readonly DependencyProperty DropDownButtonBackgroundProperty = DependencyProperty.Register(
            nameof(DropDownButtonBackground), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty DropDownButtonBorderBrushProperty = DependencyProperty.Register(
            nameof(DropDownButtonBorderBrush), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty DropDownButtonBorderThicknessProperty = DependencyProperty.Register(
            nameof(DropDownButtonBorderThickness), typeof(Thickness), typeof(OpnxTimePicker),
            new PropertyMetadata(default(Thickness)));

        public static readonly DependencyProperty DropDownButtonMouseOverBackgroundProperty = DependencyProperty.Register(
            nameof(DropDownButtonMouseOverBackground), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x33, 0x4A, 0x6F, 0xE3))));

        public static readonly DependencyProperty DropDownButtonMouseOverBorderBrushProperty = DependencyProperty.Register(
            nameof(DropDownButtonMouseOverBorderBrush), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty DropDownButtonPressedBackgroundProperty = DependencyProperty.Register(
            nameof(DropDownButtonPressedBackground), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x66, 0x4A, 0x6F, 0xE3))));

        public static readonly DependencyProperty DropDownButtonPressedBorderBrushProperty = DependencyProperty.Register(
            nameof(DropDownButtonPressedBorderBrush), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty DropDownButtonCheckedBackgroundProperty = DependencyProperty.Register(
            nameof(DropDownButtonCheckedBackground), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x33, 0x4A, 0x6F, 0xE3))));

        public static readonly DependencyProperty DropDownButtonCheckedBorderBrushProperty = DependencyProperty.Register(
            nameof(DropDownButtonCheckedBorderBrush), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty DropDownButtonCornerRadiusProperty = DependencyProperty.Register(
            nameof(DropDownButtonCornerRadius), typeof(CornerRadius), typeof(OpnxTimePicker),
            new PropertyMetadata(default(CornerRadius)));

        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
            nameof(IconBrush), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA))));

        public static readonly DependencyProperty PopupBackgroundProperty = DependencyProperty.Register(
            nameof(PopupBackground), typeof(Brush), typeof(OpnxTimePicker), new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty PopupForegroundProperty = DependencyProperty.Register(
            nameof(PopupForeground), typeof(Brush), typeof(OpnxTimePicker), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty PopupBorderBrushProperty = DependencyProperty.Register(
            nameof(PopupBorderBrush), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC))));

        public static readonly DependencyProperty PopupItemBackgroundProperty = DependencyProperty.Register(
            nameof(PopupItemBackground), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty PopupItemBorderBrushProperty = DependencyProperty.Register(
            nameof(PopupItemBorderBrush), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty PopupItemMouseOverBackgroundProperty = DependencyProperty.Register(
            nameof(PopupItemMouseOverBackground), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x33, 0x4A, 0x6F, 0xE3))));

        public static readonly DependencyProperty PopupItemMouseOverBorderBrushProperty = DependencyProperty.Register(
            nameof(PopupItemMouseOverBorderBrush), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x4A, 0x6F, 0xE3))));

        public static readonly DependencyProperty PopupSelectedItemBackgroundProperty = DependencyProperty.Register(
            nameof(PopupSelectedItemBackground), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x4A, 0x6F, 0xE3))));

        public static readonly DependencyProperty PopupSelectedItemForegroundProperty = DependencyProperty.Register(
            nameof(PopupSelectedItemForeground), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty PopupSelectedItemBorderBrushProperty = DependencyProperty.Register(
            nameof(PopupSelectedItemBorderBrush), typeof(Brush), typeof(OpnxTimePicker),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x4A, 0x6F, 0xE3))));

        public static readonly DependencyProperty PopupItemCornerRadiusProperty = DependencyProperty.Register(
            nameof(PopupItemCornerRadius), typeof(CornerRadius), typeof(OpnxTimePicker),
            new PropertyMetadata(new CornerRadius(2)));

        public static readonly DependencyProperty PopupItemMarginProperty = DependencyProperty.Register(
            nameof(PopupItemMargin), typeof(Thickness), typeof(OpnxTimePicker),
            new PropertyMetadata(new Thickness(2, 1, 2, 1)));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(OpnxTimePicker),
            new PropertyMetadata(default(CornerRadius)));

        public static readonly DependencyProperty PopupCornerRadiusProperty = DependencyProperty.Register(
            nameof(PopupCornerRadius), typeof(CornerRadius), typeof(OpnxTimePicker),
            new PropertyMetadata(new CornerRadius(4)));

        public static readonly DependencyProperty HourHeaderProperty = DependencyProperty.Register(
            nameof(HourHeader), typeof(string), typeof(OpnxTimePicker), new PropertyMetadata("시"));

        public static readonly DependencyProperty MinuteHeaderProperty = DependencyProperty.Register(
            nameof(MinuteHeader), typeof(string), typeof(OpnxTimePicker), new PropertyMetadata("분"));

        public static readonly DependencyProperty SecondHeaderProperty = DependencyProperty.Register(
            nameof(SecondHeader), typeof(string), typeof(OpnxTimePicker), new PropertyMetadata("초"));

        private readonly TextBox[] _segments;
        private bool _isUpdatingSegments;
        private bool _isUpdatingSelectors;

        public OpnxTimePicker()
        {
            Hours = new ReadOnlyCollection<int>(Enumerable.Range(0, 24).ToArray());
            Minutes = new ReadOnlyCollection<int>(Enumerable.Range(0, 60).ToArray());
            Seconds = new ReadOnlyCollection<int>(Enumerable.Range(0, 60).ToArray());

            InitializeComponent();
            _segments = [HourSegment, MinuteSegment, SecondSegment];
            UpdateSegments();
            UpdateSelectors();
        }

        public TimeSpan SelectedTime { get => (TimeSpan)GetValue(SelectedTimeProperty); set => SetValue(SelectedTimeProperty, value); }
        public TimeSpan Minimum { get => (TimeSpan)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
        public TimeSpan Maximum { get => (TimeSpan)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
        public bool IsDropDownOpen { get => (bool)GetValue(IsDropDownOpenProperty); set => SetValue(IsDropDownOpenProperty, value); }
        public bool StaysOpen { get => (bool)GetValue(StaysOpenProperty); set => SetValue(StaysOpenProperty, value); }
        public Visibility DropDownButtonVisibility { get => (Visibility)GetValue(DropDownButtonVisibilityProperty); set => SetValue(DropDownButtonVisibilityProperty, value); }
        public double DropDownButtonWidth { get => (double)GetValue(DropDownButtonWidthProperty); set => SetValue(DropDownButtonWidthProperty, value); }
        public Brush DropDownButtonBackground { get => (Brush)GetValue(DropDownButtonBackgroundProperty); set => SetValue(DropDownButtonBackgroundProperty, value); }
        public Brush DropDownButtonBorderBrush { get => (Brush)GetValue(DropDownButtonBorderBrushProperty); set => SetValue(DropDownButtonBorderBrushProperty, value); }
        public Thickness DropDownButtonBorderThickness { get => (Thickness)GetValue(DropDownButtonBorderThicknessProperty); set => SetValue(DropDownButtonBorderThicknessProperty, value); }
        public Brush DropDownButtonMouseOverBackground { get => (Brush)GetValue(DropDownButtonMouseOverBackgroundProperty); set => SetValue(DropDownButtonMouseOverBackgroundProperty, value); }
        public Brush DropDownButtonMouseOverBorderBrush { get => (Brush)GetValue(DropDownButtonMouseOverBorderBrushProperty); set => SetValue(DropDownButtonMouseOverBorderBrushProperty, value); }
        public Brush DropDownButtonPressedBackground { get => (Brush)GetValue(DropDownButtonPressedBackgroundProperty); set => SetValue(DropDownButtonPressedBackgroundProperty, value); }
        public Brush DropDownButtonPressedBorderBrush { get => (Brush)GetValue(DropDownButtonPressedBorderBrushProperty); set => SetValue(DropDownButtonPressedBorderBrushProperty, value); }
        public Brush DropDownButtonCheckedBackground { get => (Brush)GetValue(DropDownButtonCheckedBackgroundProperty); set => SetValue(DropDownButtonCheckedBackgroundProperty, value); }
        public Brush DropDownButtonCheckedBorderBrush { get => (Brush)GetValue(DropDownButtonCheckedBorderBrushProperty); set => SetValue(DropDownButtonCheckedBorderBrushProperty, value); }
        public CornerRadius DropDownButtonCornerRadius { get => (CornerRadius)GetValue(DropDownButtonCornerRadiusProperty); set => SetValue(DropDownButtonCornerRadiusProperty, value); }
        public Brush IconBrush { get => (Brush)GetValue(IconBrushProperty); set => SetValue(IconBrushProperty, value); }
        public Brush PopupBackground { get => (Brush)GetValue(PopupBackgroundProperty); set => SetValue(PopupBackgroundProperty, value); }
        public Brush PopupForeground { get => (Brush)GetValue(PopupForegroundProperty); set => SetValue(PopupForegroundProperty, value); }
        public Brush PopupBorderBrush { get => (Brush)GetValue(PopupBorderBrushProperty); set => SetValue(PopupBorderBrushProperty, value); }
        public Brush PopupItemBackground { get => (Brush)GetValue(PopupItemBackgroundProperty); set => SetValue(PopupItemBackgroundProperty, value); }
        public Brush PopupItemBorderBrush { get => (Brush)GetValue(PopupItemBorderBrushProperty); set => SetValue(PopupItemBorderBrushProperty, value); }
        public Brush PopupItemMouseOverBackground { get => (Brush)GetValue(PopupItemMouseOverBackgroundProperty); set => SetValue(PopupItemMouseOverBackgroundProperty, value); }
        public Brush PopupItemMouseOverBorderBrush { get => (Brush)GetValue(PopupItemMouseOverBorderBrushProperty); set => SetValue(PopupItemMouseOverBorderBrushProperty, value); }
        public Brush PopupSelectedItemBackground { get => (Brush)GetValue(PopupSelectedItemBackgroundProperty); set => SetValue(PopupSelectedItemBackgroundProperty, value); }
        public Brush PopupSelectedItemForeground { get => (Brush)GetValue(PopupSelectedItemForegroundProperty); set => SetValue(PopupSelectedItemForegroundProperty, value); }
        public Brush PopupSelectedItemBorderBrush { get => (Brush)GetValue(PopupSelectedItemBorderBrushProperty); set => SetValue(PopupSelectedItemBorderBrushProperty, value); }
        public CornerRadius PopupItemCornerRadius { get => (CornerRadius)GetValue(PopupItemCornerRadiusProperty); set => SetValue(PopupItemCornerRadiusProperty, value); }
        public Thickness PopupItemMargin { get => (Thickness)GetValue(PopupItemMarginProperty); set => SetValue(PopupItemMarginProperty, value); }
        public CornerRadius CornerRadius { get => (CornerRadius)GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }
        public CornerRadius PopupCornerRadius { get => (CornerRadius)GetValue(PopupCornerRadiusProperty); set => SetValue(PopupCornerRadiusProperty, value); }
        public string HourHeader { get => (string)GetValue(HourHeaderProperty); set => SetValue(HourHeaderProperty, value); }
        public string MinuteHeader { get => (string)GetValue(MinuteHeaderProperty); set => SetValue(MinuteHeaderProperty, value); }
        public string SecondHeader { get => (string)GetValue(SecondHeaderProperty); set => SetValue(SecondHeaderProperty, value); }

        public IReadOnlyList<int> Hours { get; }
        public IReadOnlyList<int> Minutes { get; }
        public IReadOnlyList<int> Seconds { get; }

        private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not OpnxTimePicker picker || picker._segments is null)
                return;

            picker.UpdateSegments();
            picker.UpdateSelectors();
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not OpnxTimePicker picker)
                return;

            picker.CoerceValue(SelectedTimeProperty);
            picker.UpdateSegments();
            picker.UpdateSelectors();
        }

        private static object CoerceSelectedTime(DependencyObject d, object baseValue)
        {
            var picker = (OpnxTimePicker)d;
            var value = ClampToTimeOfDay((TimeSpan)baseValue);
            var minimum = ClampToTimeOfDay(picker.Minimum);
            var maximum = ClampToTimeOfDay(picker.Maximum);
            if (maximum < minimum)
                maximum = minimum;

            return value < minimum ? minimum : value > maximum ? maximum : value;
        }

        private static object CoerceMinimum(DependencyObject d, object baseValue)
        {
            var picker = (OpnxTimePicker)d;
            var value = ClampToTimeOfDay((TimeSpan)baseValue);
            return value > picker.Maximum ? ClampToTimeOfDay(picker.Maximum) : value;
        }

        private static object CoerceMaximum(DependencyObject d, object baseValue)
        {
            var picker = (OpnxTimePicker)d;
            var value = ClampToTimeOfDay((TimeSpan)baseValue);
            return value < picker.Minimum ? ClampToTimeOfDay(picker.Minimum) : value;
        }

        private static TimeSpan ClampToTimeOfDay(TimeSpan value) =>
            value < TimeSpan.Zero ? TimeSpan.Zero : value > LastTimeOfDay ? LastTimeOfDay : value;

        private void Segment_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
                e.Handled = !e.Text.All(char.IsDigit) || !CanApplyText(textBox, e.Text);
        }

        private void Segment_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            switch (e.Key)
            {
                case Key.OemSemicolon:
                case Key.Decimal:
                    e.Handled = MoveToNextSegment(textBox);
                    break;
                case Key.Right when textBox.CaretIndex == textBox.Text.Length:
                    e.Handled = MoveToNextSegment(textBox);
                    break;
                case Key.Left when textBox.CaretIndex == 0:
                    e.Handled = MoveToPreviousSegment(textBox);
                    break;
                case Key.Up:
                    e.Handled = true;
                    ChangeSegmentValue(textBox, 1);
                    break;
                case Key.Down:
                    e.Handled = true;
                    ChangeSegmentValue(textBox, -1);
                    break;
                case Key.Enter:
                    e.Handled = true;
                    CommitSegments();
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    break;
            }
        }

        private void Segment_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox { IsKeyboardFocusWithin: false } textBox)
            {
                e.Handled = true;
                textBox.Focus();
            }
        }

        private void Segment_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                textBox.SelectAll();
        }

        private void Segment_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
                CommitSegments();
        }

        private void Segment_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            ChangeSegmentValue(textBox, e.Delta > 0 ? 1 : -1);
            e.Handled = true;
        }

        private void Segment_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingSegments || sender is not TextBox textBox)
                return;

            if (_segments.All(segment => segment.Text.Length == 2) && TryGetSegments(out TimeSpan value))
                SetCurrentValue(SelectedTimeProperty, value);

            if (textBox.Text.Length == 2 && textBox.SelectionLength == 0)
                MoveToNextSegment(textBox);
        }

        private void Segment_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not TextBox textBox || !e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
            {
                e.CancelCommand();
                return;
            }

            string? text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                e.CancelCommand();
                return;
            }

            text = text.Trim();
            if (text.Contains(':'))
            {
                e.CancelCommand();
                if (TimeSpan.TryParseExact(text, [@"h\:m", @"h\:m\:s", @"hh\:mm", @"hh\:mm\:ss"], null, out TimeSpan value))
                    SetCurrentValue(SelectedTimeProperty, value);
                return;
            }

            if (!text.All(char.IsDigit) || !CanApplyText(textBox, text))
                e.CancelCommand();
        }

        private void Popup_Opened(object? sender, EventArgs e)
        {
            UpdateSelectors();
            HourList.ScrollIntoView(HourList.SelectedItem);
            MinuteList.ScrollIntoView(MinuteList.SelectedItem);
            SecondList.ScrollIntoView(SecondList.SelectedItem);
        }

        private void TimeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelectors || HourList.SelectedItem is not int hour ||
                MinuteList.SelectedItem is not int minute || SecondList.SelectedItem is not int second)
                return;

            SetCurrentValue(SelectedTimeProperty, new TimeSpan(hour, minute, second));
        }

        private void CommitSegments()
        {
            if (TryGetSegments(out TimeSpan value))
                SetCurrentValue(SelectedTimeProperty, value);
            UpdateSegments();
        }

        private bool TryGetSegments(out TimeSpan value)
        {
            value = default;
            if (!int.TryParse(HourSegment.Text, out int hour) || hour is < 0 or > 23 ||
                !int.TryParse(MinuteSegment.Text, out int minute) || minute is < 0 or > 59 ||
                !int.TryParse(SecondSegment.Text, out int second) || second is < 0 or > 59)
                return false;

            value = new TimeSpan(hour, minute, second);
            return true;
        }

        private void UpdateSegments()
        {
            if (_segments is null)
                return;

            _isUpdatingSegments = true;
            HourSegment.Text = SelectedTime.Hours.ToString("00");
            MinuteSegment.Text = SelectedTime.Minutes.ToString("00");
            SecondSegment.Text = SelectedTime.Seconds.ToString("00");
            _isUpdatingSegments = false;
        }

        private void UpdateSelectors()
        {
            if (_segments is null)
                return;

            _isUpdatingSelectors = true;
            HourList.SelectedItem = SelectedTime.Hours;
            MinuteList.SelectedItem = SelectedTime.Minutes;
            SecondList.SelectedItem = SelectedTime.Seconds;
            _isUpdatingSelectors = false;
        }

        private void ChangeSegmentValue(TextBox textBox, int change)
        {
            TimeSpan delta = ReferenceEquals(textBox, HourSegment)
                ? TimeSpan.FromHours(change)
                : ReferenceEquals(textBox, MinuteSegment) ? TimeSpan.FromMinutes(change) : TimeSpan.FromSeconds(change);

            TimeSpan next = SelectedTime + delta;
            if (next < Minimum)
                next = Maximum;
            else if (next > Maximum)
                next = Minimum;

            SetCurrentValue(SelectedTimeProperty, next);
            textBox.Focus();
            textBox.SelectAll();
        }

        private bool MoveToPreviousSegment(TextBox current)
        {
            int index = Array.IndexOf(_segments, current);
            if (index <= 0)
                return false;

            _segments[index - 1].Focus();
            return true;
        }

        private bool MoveToNextSegment(TextBox current)
        {
            int index = Array.IndexOf(_segments, current);
            if (index < 0 || index >= _segments.Length - 1)
                return false;

            _segments[index + 1].Focus();
            return true;
        }

        private bool CanApplyText(TextBox textBox, string input)
        {
            string next = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                .Insert(textBox.SelectionStart, input);
            if (!int.TryParse(next, out int value))
                return false;

            return ReferenceEquals(textBox, HourSegment) ? value <= 23 : value <= 59;
        }
    }
}
