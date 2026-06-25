using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OPNX.UI.WPF.Controls
{
    [TemplatePart(Name = IncreaseButtonPartName, Type = typeof(ButtonBase))]
    [TemplatePart(Name = DecreaseButtonPartName, Type = typeof(ButtonBase))]
    public class OpnxNumericBox : OpnxTextBox
    {
        private const string IncreaseButtonPartName = "PART_IncreaseButton";
        private const string DecreaseButtonPartName = "PART_DecreaseButton";

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(OpnxNumericBox),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged,
                CoerceValue));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(int),
            typeof(OpnxNumericBox),
            new PropertyMetadata(0, OnRangePropertyChanged));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(int),
            typeof(OpnxNumericBox),
            new PropertyMetadata(int.MaxValue, OnRangePropertyChanged, CoerceMaximum));

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step),
            typeof(int),
            typeof(OpnxNumericBox),
            new PropertyMetadata(1, OnStepChanged, CoerceStep));

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>),
            typeof(OpnxNumericBox));

        public static readonly RoutedEvent IncreaseClickedEvent = EventManager.RegisterRoutedEvent(
            nameof(IncreaseClicked),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(OpnxNumericBox));

        public static readonly RoutedEvent DecreaseClickedEvent = EventManager.RegisterRoutedEvent(
            nameof(DecreaseClicked),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(OpnxNumericBox));

        private ButtonBase? _increaseButton;
        private ButtonBase? _decreaseButton;
        private bool _isUpdatingText;

        static OpnxNumericBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxNumericBox),
                new FrameworkPropertyMetadata(typeof(OpnxNumericBox)));
        }

        public OpnxNumericBox()
        {
            DataObject.AddPastingHandler(this, OnPaste);
        }

        public event RoutedPropertyChangedEventHandler<int> ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        public event RoutedEventHandler IncreaseClicked
        {
            add => AddHandler(IncreaseClickedEvent, value);
            remove => RemoveHandler(IncreaseClickedEvent, value);
        }

        public event RoutedEventHandler DecreaseClicked
        {
            add => AddHandler(DecreaseClickedEvent, value);
            remove => RemoveHandler(DecreaseClickedEvent, value);
        }

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public override void OnApplyTemplate()
        {
            if (_increaseButton is not null)
            {
                _increaseButton.Click -= IncreaseButton_Click;
            }

            if (_decreaseButton is not null)
            {
                _decreaseButton.Click -= DecreaseButton_Click;
            }

            base.OnApplyTemplate();

            _increaseButton = GetTemplateChild(IncreaseButtonPartName) as ButtonBase;
            _decreaseButton = GetTemplateChild(DecreaseButtonPartName) as ButtonBase;

            if (_increaseButton is not null)
            {
                _increaseButton.Click += IncreaseButton_Click;
            }

            if (_decreaseButton is not null)
            {
                _decreaseButton.Click += DecreaseButton_Click;
            }

            UpdateTextFromValue();
            UpdateButtonState();
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            e.Handled = !CanApplyText(e.Text);
            base.OnPreviewTextInput(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (Increase())
                    {
                        RaiseEvent(new RoutedEventArgs(IncreaseClickedEvent));
                    }
                    e.Handled = true;
                    return;

                case Key.Down:
                    if (Decrease())
                    {
                        RaiseEvent(new RoutedEventArgs(DecreaseClickedEvent));
                    }
                    e.Handled = true;
                    return;
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (_isUpdatingText)
                return;

            if (IsTransientText(Text))
            {
                UpdateButtonState();
                return;
            }

            if (int.TryParse(Text, out int textValue))
            {
                Value = Clamp(textValue, Minimum, Maximum);
            }
            else
            {
                UpdateTextFromValue();
            }

            UpdateButtonState();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            UpdateTextFromValue();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericBox = (OpnxNumericBox)d;
            int oldValue = (int)e.OldValue;
            int newValue = (int)e.NewValue;

            numericBox.UpdateTextFromValue();
            numericBox.UpdateButtonState();
            numericBox.RaiseEvent(new RoutedPropertyChangedEventArgs<int>(
                oldValue,
                newValue,
                ValueChangedEvent));
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var numericBox = (OpnxNumericBox)d;
            return Clamp((int)baseValue, numericBox.Minimum, numericBox.Maximum);
        }

        private static void OnRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericBox = (OpnxNumericBox)d;
            numericBox.CoerceValue(MaximumProperty);
            numericBox.CoerceValue(ValueProperty);
            numericBox.UpdateTextFromValue();
            numericBox.UpdateButtonState();
        }

        private static object CoerceMaximum(DependencyObject d, object baseValue)
        {
            var numericBox = (OpnxNumericBox)d;
            return Math.Max(numericBox.Minimum, (int)baseValue);
        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((OpnxNumericBox)d).UpdateButtonState();
        }

        private static object CoerceStep(DependencyObject d, object baseValue)
        {
            return Math.Max(1, (int)baseValue);
        }

        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Increase())
            {
                RaiseEvent(new RoutedEventArgs(IncreaseClickedEvent));
            }
            e.Handled = true;
        }

        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Decrease())
            {
                RaiseEvent(new RoutedEventArgs(DecreaseClickedEvent));
            }
            e.Handled = true;
        }

        private bool Increase()
        {
            int oldValue = Value;
            Value = Clamp((long)Value + Step, Minimum, Maximum);
            return Value != oldValue;
        }

        private bool Decrease()
        {
            int oldValue = Value;
            Value = Clamp((long)Value - Step, Minimum, Maximum);
            return Value != oldValue;
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
            {
                e.CancelCommand();
                return;
            }

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            if (string.IsNullOrWhiteSpace(text) || !CanApplyText(text.Trim()))
            {
                e.CancelCommand();
            }
        }

        private bool CanApplyText(string input)
        {
            string nextText = Text
                .Remove(SelectionStart, SelectionLength)
                .Insert(SelectionStart, input);

            if (IsTransientText(nextText))
            {
                return Minimum < 0;
            }

            return int.TryParse(nextText, out int value) &&
                   value >= Minimum &&
                   value <= Maximum;
        }

        private void UpdateTextFromValue()
        {
            string valueText = Value.ToString();
            if (Text == valueText)
                return;

            _isUpdatingText = true;
            Text = valueText;
            CaretIndex = Text.Length;
            _isUpdatingText = false;
        }

        private void UpdateButtonState()
        {
            if (_increaseButton is not null)
            {
                _increaseButton.IsEnabled = IsEnabled && Value < Maximum;
            }

            if (_decreaseButton is not null)
            {
                _decreaseButton.IsEnabled = IsEnabled && Value > Minimum;
            }
        }

        private static bool IsTransientText(string text)
        {
            return string.IsNullOrEmpty(text) || text == "-";
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            return Clamp((long)value, minimum, maximum);
        }

        private static int Clamp(long value, int minimum, int maximum)
        {
            return (int)Math.Min(Math.Max(value, minimum), maximum);
        }
    }
}
