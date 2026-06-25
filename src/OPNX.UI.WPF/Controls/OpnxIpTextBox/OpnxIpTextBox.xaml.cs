using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OPNX.UI.WPF.Controls
{
    public partial class OpnxIpTextBox : UserControl
    {
        public static readonly DependencyProperty AddressProperty = DependencyProperty.Register(
            nameof(Address),
            typeof(string),
            typeof(OpnxIpTextBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnAddressChanged));

        private readonly TextBox[] _segments;
        private bool _isUpdatingAddress;
        private bool _isUpdatingSegments;

        public OpnxIpTextBox()
        {
            InitializeComponent();

            _segments =
            [
                FirstSegment,
                SecondSegment,
                ThirdSegment,
                FourthSegment
            ];
        }

        public string Address
        {
            get => (string)GetValue(AddressProperty);
            set => SetValue(AddressProperty, value);
        }

        private static void OnAddressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not OpnxIpTextBox ipTextBox || ipTextBox._isUpdatingAddress)
                return;

            ipTextBox.SetSegmentsFromAddress(e.NewValue as string);
        }

        private void Segment_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            e.Handled = !IsDigitText(e.Text) || !CanApplyText(textBox, e.Text);
        }

        private void Segment_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            switch (e.Key)
            {
                case Key.OemPeriod:
                case Key.Decimal:
                    e.Handled = true;
                    MoveToNextSegment(textBox);
                    break;

                case Key.Right when textBox.CaretIndex == textBox.Text.Length:
                    e.Handled = MoveToNextSegment(textBox);
                    break;

                case Key.Left when textBox.CaretIndex == 0:
                    e.Handled = MoveToPreviousSegment(textBox);
                    break;

                case Key.Back when textBox.CaretIndex == 0 && textBox.SelectionLength == 0:
                    MoveToPreviousSegment(textBox);
                    break;
            }
        }

        private void Segment_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingSegments || sender is not TextBox textBox)
                return;

            string normalizedText = NormalizeSegmentText(textBox.Text);
            if (textBox.Text != normalizedText)
            {
                int caretIndex = Math.Min(textBox.CaretIndex, normalizedText.Length);

                _isUpdatingSegments = true;
                textBox.Text = normalizedText;
                textBox.CaretIndex = caretIndex;
                _isUpdatingSegments = false;
            }

            UpdateAddressFromSegments();

            if (textBox.Text.Length == 3 && textBox.CaretIndex == textBox.Text.Length)
            {
                MoveToNextSegment(textBox);
            }
        }

        private void Segment_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not TextBox textBox ||
                !e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
            {
                e.CancelCommand();
                return;
            }

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                e.CancelCommand();
                return;
            }

            text = text.Trim();

            if (text.Contains('.'))
            {
                e.CancelCommand();

                if (TryParseAddress(text, out var parts))
                {
                    SetSegments(parts);
                    UpdateAddressFromSegments();
                    FourthSegment.Focus();
                    FourthSegment.CaretIndex = FourthSegment.Text.Length;
                }

                return;
            }

            if (!IsDigitText(text) || !CanApplyText(textBox, text))
            {
                e.CancelCommand();
            }
        }

        private void SetSegmentsFromAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                SetSegments(["", "", "", ""]);
                return;
            }

            if (TryParseAddress(address, out var parts))
            {
                SetSegments(parts);
                return;
            }

            var splitParts = address.Split('.');
            var safeParts = new string[4];

            for (int i = 0; i < safeParts.Length; i++)
            {
                safeParts[i] = i < splitParts.Length
                    ? NormalizeSegmentText(splitParts[i])
                    : string.Empty;
            }

            SetSegments(safeParts);
        }

        private void SetSegments(IReadOnlyList<string> parts)
        {
            _isUpdatingSegments = true;

            for (int i = 0; i < _segments.Length; i++)
            {
                _segments[i].Text = i < parts.Count ? parts[i] : string.Empty;
            }

            _isUpdatingSegments = false;
        }

        private void UpdateAddressFromSegments()
        {
            _isUpdatingAddress = true;
            SetCurrentValue(AddressProperty, string.Join(".", _segments.Select(segment => segment.Text)));
            _isUpdatingAddress = false;
        }

        private bool MoveToPreviousSegment(TextBox currentTextBox)
        {
            int index = Array.IndexOf(_segments, currentTextBox);
            if (index <= 0)
                return false;

            var previousSegment = _segments[index - 1];
            previousSegment.Focus();
            previousSegment.CaretIndex = previousSegment.Text.Length;
            return true;
        }

        private bool MoveToNextSegment(TextBox currentTextBox)
        {
            int index = Array.IndexOf(_segments, currentTextBox);
            if (index < 0 || index >= _segments.Length - 1)
                return false;

            var nextSegment = _segments[index + 1];
            nextSegment.Focus();
            nextSegment.CaretIndex = nextSegment.Text.Length;
            return true;
        }

        private static bool TryParseAddress(string address, out string[] parts)
        {
            parts = [];
            var splitParts = address.Split('.');

            if (splitParts.Length != 4)
            {
                return false;
            }

            var parsedParts = new string[4];

            for (int i = 0; i < splitParts.Length; i++)
            {
                if (!IsDigitText(splitParts[i]) ||
                    !int.TryParse(splitParts[i], out int value) ||
                    value > 255)
                {
                    return false;
                }

                parsedParts[i] = value.ToString();
            }

            parts = parsedParts;
            return true;
        }

        private static string NormalizeSegmentText(string text)
        {
            string digits = new(text.Where(char.IsDigit).Take(3).ToArray());

            if (digits.Length == 0)
                return string.Empty;

            return int.TryParse(digits, out int value)
                ? Math.Min(value, 255).ToString()
                : string.Empty;
        }

        private static bool CanApplyText(TextBox textBox, string text)
        {
            string nextText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                .Insert(textBox.SelectionStart, text);

            return int.TryParse(nextText, out int value) && value <= 255;
        }

        private static bool IsDigitText(string text)
        {
            return text.All(char.IsDigit);
        }
    }
}
