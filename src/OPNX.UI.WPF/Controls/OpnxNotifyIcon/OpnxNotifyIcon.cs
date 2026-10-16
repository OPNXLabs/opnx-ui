using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace OPNX.UI.WPF.Controls
{
    public sealed class OpnxNotifyIcon : IDisposable
    {
        #region Fields

        private const int MaximumToolTipLength = 63;

        private readonly OpnxNotifyIconOptions _options;
        private Window? _window;
        private Action? _exitApplication;
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private Popup? _contextMenu;
        private bool _isExitRequested;
        private bool _isDisposed;

        #endregion

        #region Constructors

        public OpnxNotifyIcon()
            : this(new OpnxNotifyIconOptions())
        {
        }

        public OpnxNotifyIcon(OpnxNotifyIconOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _options = options;
        }

        #endregion

        #region Properties

        public bool IsAttached => _window != null;

        public bool IsVisible
        {
            get => _notifyIcon?.Visible == true;
            set
            {
                ObjectDisposedException.ThrowIf(_isDisposed, this);
                if (_notifyIcon != null)
                    _notifyIcon.Visible = value;
            }
        }

        public string ToolTipText
        {
            get => _notifyIcon?.Text ?? _options.ToolTipText;
            set
            {
                ObjectDisposedException.ThrowIf(_isDisposed, this);
                _options.ToolTipText = value ?? string.Empty;
                if (_notifyIcon != null)
                    _notifyIcon.Text = NormalizeToolTipText(_options.ToolTipText);
            }
        }

        #endregion

        #region Public Methods

        public void Attach(Window window, Action exitApplication)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            ArgumentNullException.ThrowIfNull(window);
            ArgumentNullException.ThrowIfNull(exitApplication);

            if (IsAttached)
                throw new InvalidOperationException("The notification icon is already attached to a window.");

            _window = window;
            _exitApplication = exitApplication;
            _window.Closing += Window_Closing;

            _contextMenu = CreateContextMenu();
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = _options.Icon ?? SystemIcons.Application,
                Text = NormalizeToolTipText(_options.ToolTipText),
                Visible = true
            };
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            _notifyIcon.MouseUp += NotifyIcon_MouseUp;
        }

        public void ShowWindow()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            InvokeOnDispatcher(ShowWindowCore);
        }

        public void HideWindow()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            InvokeOnDispatcher(() => _window?.Hide());
        }

        public void ExitApplication()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            InvokeOnDispatcher(() =>
            {
                _isExitRequested = true;
                _exitApplication?.Invoke();
            });
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_window != null)
                _window.Closing -= Window_Closing;

            if (_notifyIcon != null)
            {
                _notifyIcon.DoubleClick -= NotifyIcon_DoubleClick;
                _notifyIcon.MouseUp -= NotifyIcon_MouseUp;
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }

            if (_contextMenu != null)
                _contextMenu.IsOpen = false;
            _notifyIcon = null;
            _contextMenu = null;
            _window = null;
            _exitApplication = null;
        }

        #endregion

        #region Event Handlers

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            if (_isExitRequested || !_options.HideWindowOnClose)
                return;

            e.Cancel = true;
            _window?.Hide();
        }

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            if (_options.RestoreWindowOnDoubleClick)
                ShowWindow();
        }

        private void NotifyIcon_MouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right || _contextMenu == null)
                return;

            InvokeOnDispatcher(() =>
            {
                _contextMenu.IsOpen = false;
                _contextMenu.IsOpen = true;
            });
        }

        #endregion

        #region Private Methods

        private Popup? CreateContextMenu()
        {
            if (!_options.ShowOpenMenuItem && !_options.ShowExitMenuItem)
                return null;

            var menu = new OpnxNotifyIconMenu(_options);
            menu.OpenRequested += (_, _) =>
            {
                CloseContextMenu();
                ShowWindow();
            };
            menu.ExitRequested += (_, _) =>
            {
                CloseContextMenu();
                ExitApplication();
            };

            return new Popup
            {
                AllowsTransparency = true,
                Child = menu,
                Placement = PlacementMode.MousePoint,
                PopupAnimation = PopupAnimation.Fade,
                StaysOpen = false
            };
        }

        private void CloseContextMenu()
        {
            if (_contextMenu != null)
                _contextMenu.IsOpen = false;
        }

        private void InvokeOnDispatcher(Action action)
        {
            Window? window = _window;
            if (window == null)
                return;

            if (window.Dispatcher.CheckAccess())
                action();
            else
                window.Dispatcher.Invoke(action);
        }

        private void ShowWindowCore()
        {
            if (_window == null)
                return;

            if (_window.WindowState == WindowState.Minimized)
                _window.WindowState = WindowState.Normal;

            _window.Show();
            _window.Activate();
        }

        private static string NormalizeToolTipText(string? value)
        {
            string toolTipText = value ?? string.Empty;
            return toolTipText.Length <= MaximumToolTipLength
                ? toolTipText
                : toolTipText[..MaximumToolTipLength];
        }

        #endregion
    }
}
