using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    [TemplatePart(Name = ClearButtonPartName, Type = typeof(ButtonBase))]
    public class OpnxTextBox : TextBox
    {
        private const string ClearButtonPartName = "PART_ClearButton";

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(OpnxTextBox),
            new PropertyMetadata(new CornerRadius(0)));

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(OpnxTextBox),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PlaceholderEnabledProperty = DependencyProperty.Register(
            nameof(PlaceholderEnabled),
            typeof(bool),
            typeof(OpnxTextBox),
            new PropertyMetadata(true));

        public static readonly DependencyProperty PlaceholderForegroundProperty = DependencyProperty.Register(
            nameof(PlaceholderForeground),
            typeof(Brush),
            typeof(OpnxTextBox),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(135, 255, 255, 255))));

        public static readonly DependencyProperty ClearButtonEnabledProperty = DependencyProperty.Register(
            nameof(ClearButtonEnabled),
            typeof(bool),
            typeof(OpnxTextBox),
            new PropertyMetadata(true));

        private ButtonBase? _clearButton;

        static OpnxTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxTextBox),
                new FrameworkPropertyMetadata(typeof(OpnxTextBox)));
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public bool PlaceholderEnabled
        {
            get => (bool)GetValue(PlaceholderEnabledProperty);
            set => SetValue(PlaceholderEnabledProperty, value);
        }

        public Brush PlaceholderForeground
        {
            get => (Brush)GetValue(PlaceholderForegroundProperty);
            set => SetValue(PlaceholderForegroundProperty, value);
        }

        public bool ClearButtonEnabled
        {
            get => (bool)GetValue(ClearButtonEnabledProperty);
            set => SetValue(ClearButtonEnabledProperty, value);
        }

        public override void OnApplyTemplate()
        {
            if (_clearButton is not null)
            {
                _clearButton.Click -= ClearButton_Click;
            }

            base.OnApplyTemplate();

            _clearButton = GetTemplateChild(ClearButtonPartName) as ButtonBase;

            if (_clearButton is not null)
            {
                _clearButton.Click += ClearButton_Click;
            }

            UpdatePlaceholder();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            UpdatePlaceholder();
        }

        protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            UpdatePlaceholder();
        }

        protected override void OnLostKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            UpdatePlaceholder();
        }

        protected virtual void OnClearButtonClick()
        {
            if (Text.Length > 0)
            {
                Clear();
                Focus();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            OnClearButtonClick();
            e.Handled = true;
        }

        private void UpdatePlaceholder()
        {
            bool shouldEnablePlaceholder = Text.Length == 0;
            if (PlaceholderEnabled != shouldEnablePlaceholder)
            {
                PlaceholderEnabled = shouldEnablePlaceholder;
            }
        }
    }
}
