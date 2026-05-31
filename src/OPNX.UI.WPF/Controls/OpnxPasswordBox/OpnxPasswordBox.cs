using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace OPNX.UI.WPF.Controls
{
    [TemplatePart(Name = RevealButtonPartName, Type = typeof(ButtonBase))]
    public class OpnxPasswordBox : OpnxTextBox
    {
        private const string RevealButtonPartName = "PART_RevealButton";

        private bool _lockUpdatingContents;
        private ButtonBase? _revealButton;

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(OpnxPasswordBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

        public static readonly DependencyProperty PasswordCharProperty = DependencyProperty.Register(
            nameof(PasswordChar),
            typeof(char),
            typeof(OpnxPasswordBox),
            new PropertyMetadata('*', OnPasswordCharPropertyChanged));

        public static readonly DependencyProperty IsPasswordRevealedProperty = DependencyProperty.Register(
            nameof(IsPasswordRevealed),
            typeof(bool),
            typeof(OpnxPasswordBox),
            new PropertyMetadata(false, OnPasswordRevealModePropertyChanged));

        public static readonly DependencyProperty RevealButtonEnabledProperty = DependencyProperty.Register(
            nameof(RevealButtonEnabled),
            typeof(bool),
            typeof(OpnxPasswordBox),
            new PropertyMetadata(true, OnRevealButtonStatePropertyChanged));

        public static readonly DependencyProperty ShowRevealButtonProperty = DependencyProperty.Register(
            nameof(ShowRevealButton),
            typeof(bool),
            typeof(OpnxPasswordBox),
            new PropertyMetadata(false));

        public static readonly RoutedEvent PasswordChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(PasswordChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(OpnxPasswordBox));

        static OpnxPasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxPasswordBox),
                new FrameworkPropertyMetadata(typeof(OpnxPasswordBox)));
        }

        public event RoutedEventHandler PasswordChanged
        {
            add => AddHandler(PasswordChangedEvent, value);
            remove => RemoveHandler(PasswordChangedEvent, value);
        }

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public char PasswordChar
        {
            get => (char)GetValue(PasswordCharProperty);
            set => SetValue(PasswordCharProperty, value);
        }

        public bool IsPasswordRevealed
        {
            get => (bool)GetValue(IsPasswordRevealedProperty);
            set => SetValue(IsPasswordRevealedProperty, value);
        }

        public bool RevealButtonEnabled
        {
            get => (bool)GetValue(RevealButtonEnabledProperty);
            set => SetValue(RevealButtonEnabledProperty, value);
        }

        public bool ShowRevealButton
        {
            get => (bool)GetValue(ShowRevealButtonProperty);
            set => SetValue(ShowRevealButtonProperty, value);
        }

        public override void OnApplyTemplate()
        {
            if (_revealButton is not null)
            {
                _revealButton.Click -= RevealButton_Click;
            }

            base.OnApplyTemplate();

            _revealButton = GetTemplateChild(RevealButtonPartName) as ButtonBase;

            if (_revealButton is not null)
            {
                _revealButton.Click += RevealButton_Click;
            }

            UpdateRevealButton();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (_lockUpdatingContents)
            {
                base.OnTextChanged(e);
                return;
            }

            UpdatePasswordFromText();
            base.OnTextChanged(e);
            UpdateRevealButton();
        }

        protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            UpdateRevealButton();
        }

        protected override void OnLostKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            UpdateRevealButton();
        }

        protected virtual void OnRevealButtonClick()
        {
            IsPasswordRevealed = !IsPasswordRevealed;
            Focus();
            CaretIndex = Text.Length;
        }

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPasswordBox control)
            {
                control.UpdateTextForReveal();
                control.UpdateRevealButton();
            }
        }

        private static void OnPasswordCharPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPasswordBox control && !control.IsPasswordRevealed)
            {
                control.UpdateTextForReveal();
            }
        }

        private static void OnPasswordRevealModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPasswordBox control)
            {
                control.UpdateTextForReveal();
                control.UpdateRevealButton();
            }
        }

        private static void OnRevealButtonStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPasswordBox control)
            {
                control.UpdateRevealButton();
            }
        }

        private void RevealButton_Click(object sender, RoutedEventArgs e)
        {
            OnRevealButtonClick();
            e.Handled = true;
        }

        private void UpdatePasswordFromText()
        {
            if (_lockUpdatingContents)
            {
                return;
            }

            _lockUpdatingContents = true;

            try
            {
                if (IsPasswordRevealed)
                {
                    Password = Text;
                }
                else
                {
                    int caret = CaretIndex;
                    string maskedText = Text;

                    if (maskedText.Length > Password.Length)
                    {
                        int addedLength = maskedText.Length - Password.Length;
                        int insertIndex = Math.Max(0, caret - addedLength);
                        string added = maskedText.Substring(insertIndex, addedLength);
                        Password = Password.Insert(insertIndex, added);
                    }
                    else if (maskedText.Length < Password.Length)
                    {
                        int removeCount = Password.Length - maskedText.Length;
                        int removeIndex = Math.Min(caret, Password.Length - removeCount);
                        Password = Password.Remove(removeIndex, removeCount);
                    }

                    Text = new string(PasswordChar, Password.Length);
                    CaretIndex = Math.Min(caret, Text.Length);
                }

                RaiseEvent(new RoutedEventArgs(PasswordChangedEvent));
            }
            finally
            {
                _lockUpdatingContents = false;
            }
        }

        private void UpdateTextForReveal()
        {
            if (_lockUpdatingContents)
            {
                return;
            }

            _lockUpdatingContents = true;

            try
            {
                string displayPassword = Password ?? string.Empty;
                string displayText = IsPasswordRevealed
                    ? displayPassword
                    : new string(PasswordChar, displayPassword.Length);

                if (Text != displayText)
                {
                    Text = displayText;
                }

                CaretIndex = Text.Length;
            }
            finally
            {
                _lockUpdatingContents = false;
            }
        }

        private void UpdateRevealButton()
        {
            bool shouldShowRevealButton = RevealButtonEnabled && IsKeyboardFocusWithin && Password.Length > 0;

            if (ShowRevealButton != shouldShowRevealButton)
            {
                ShowRevealButton = shouldShowRevealButton;
            }
        }
    }
}
