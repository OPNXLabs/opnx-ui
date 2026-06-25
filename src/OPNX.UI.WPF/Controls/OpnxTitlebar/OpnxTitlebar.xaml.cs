using OPNX.Lib.Common.Platform.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public partial class OpnxTitlebar : UserControl, INotifyPropertyChanged
    {
        private Window? _ownerWindow;
        private HwndSource? _hwndSource;
        private bool _isWindowMaximized;

        public OpnxTitlebar()
        {
            InitializeComponent();

            Loaded += OpnxTitlebar_Loaded;
            Unloaded += OpnxTitlebar_Unloaded;
        }

        public static readonly DependencyProperty LogoSourceProperty = DependencyProperty.Register(
            nameof(LogoSource),
            typeof(ImageSource),
            typeof(OpnxTitlebar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty LogoWidthProperty = DependencyProperty.Register(
            nameof(LogoWidth),
            typeof(double),
            typeof(OpnxTitlebar),
            new PropertyMetadata(160d));

        public static readonly DependencyProperty LogoHeightProperty = DependencyProperty.Register(
            nameof(LogoHeight),
            typeof(double),
            typeof(OpnxTitlebar),
            new PropertyMetadata(32d));

        public static readonly DependencyProperty LogoMarginProperty = DependencyProperty.Register(
            nameof(LogoMargin),
            typeof(Thickness),
            typeof(OpnxTitlebar),
            new PropertyMetadata(new Thickness(16, 0, 16, 0)));

        public static readonly DependencyProperty TitleContentProperty = DependencyProperty.Register(
            nameof(TitleContent),
            typeof(object),
            typeof(OpnxTitlebar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TitleContentTemplateProperty = DependencyProperty.Register(
            nameof(TitleContentTemplate),
            typeof(DataTemplate),
            typeof(OpnxTitlebar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TitleContentMarginProperty = DependencyProperty.Register(
            nameof(TitleContentMargin),
            typeof(Thickness),
            typeof(OpnxTitlebar),
            new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty RightContentProperty = DependencyProperty.Register(
            nameof(RightContent),
            typeof(object),
            typeof(OpnxTitlebar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty RightContentTemplateProperty = DependencyProperty.Register(
            nameof(RightContentTemplate),
            typeof(DataTemplate),
            typeof(OpnxTitlebar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty RightContentMarginProperty = DependencyProperty.Register(
            nameof(RightContentMargin),
            typeof(Thickness),
            typeof(OpnxTitlebar),
            new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty ControlBoxMarginProperty = DependencyProperty.Register(
            nameof(ControlBoxMargin),
            typeof(Thickness),
            typeof(OpnxTitlebar),
            new PropertyMetadata(new Thickness(24, 0, 12, 0)));

        public static readonly DependencyProperty ButtonSizeProperty = DependencyProperty.Register(
            nameof(ButtonSize),
            typeof(double),
            typeof(OpnxTitlebar),
            new PropertyMetadata(28d));

        public static readonly DependencyProperty ButtonImageSizeProperty = DependencyProperty.Register(
            nameof(ButtonImageSize),
            typeof(double),
            typeof(OpnxTitlebar),
            new PropertyMetadata(16d));

        public static readonly DependencyProperty ButtonMarginProperty = DependencyProperty.Register(
            nameof(ButtonMargin),
            typeof(Thickness),
            typeof(OpnxTitlebar),
            new PropertyMetadata(new Thickness(4, 0, 0, 0)));

        public static readonly DependencyProperty ButtonForegroundProperty = DependencyProperty.Register(
            nameof(ButtonForeground),
            typeof(Brush),
            typeof(OpnxTitlebar),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty ButtonMouseOverOpacityProperty = DependencyProperty.Register(
            nameof(ButtonMouseOverOpacity),
            typeof(double),
            typeof(OpnxTitlebar),
            new PropertyMetadata(0.65d));

        public static readonly DependencyProperty ButtonPressedOpacityProperty = DependencyProperty.Register(
            nameof(ButtonPressedOpacity),
            typeof(double),
            typeof(OpnxTitlebar),
            new PropertyMetadata(0.45d));

        public static readonly DependencyProperty ButtonDisabledOpacityProperty = DependencyProperty.Register(
            nameof(ButtonDisabledOpacity),
            typeof(double),
            typeof(OpnxTitlebar),
            new PropertyMetadata(0.3d));

        public static readonly DependencyProperty FallbackGlyphSizeProperty = DependencyProperty.Register(
            nameof(FallbackGlyphSize),
            typeof(double),
            typeof(OpnxTitlebar),
            new PropertyMetadata(13d));

        public static readonly DependencyProperty ShowMinimizeButtonProperty = DependencyProperty.Register(
            nameof(ShowMinimizeButton),
            typeof(bool),
            typeof(OpnxTitlebar),
            new PropertyMetadata(true));

        public static readonly DependencyProperty ShowMaximizeButtonProperty = DependencyProperty.Register(
            nameof(ShowMaximizeButton),
            typeof(bool),
            typeof(OpnxTitlebar),
            new PropertyMetadata(true));

        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register(
            nameof(ShowCloseButton),
            typeof(bool),
            typeof(OpnxTitlebar),
            new PropertyMetadata(true));

        public static readonly DependencyProperty MinimizeImageSourceProperty = DependencyProperty.Register(
            nameof(MinimizeImageSource),
            typeof(ImageSource),
            typeof(OpnxTitlebar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty MaximizeImageSourceProperty = DependencyProperty.Register(
            nameof(MaximizeImageSource),
            typeof(ImageSource),
            typeof(OpnxTitlebar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty RestoreImageSourceProperty = DependencyProperty.Register(
            nameof(RestoreImageSource),
            typeof(ImageSource),
            typeof(OpnxTitlebar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CloseImageSourceProperty = DependencyProperty.Register(
            nameof(CloseImageSource),
            typeof(ImageSource),
            typeof(OpnxTitlebar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty MinimizeToolTipProperty = DependencyProperty.Register(
            nameof(MinimizeToolTip),
            typeof(object),
            typeof(OpnxTitlebar),
            new PropertyMetadata("Minimize"));

        public static readonly DependencyProperty MaximizeToolTipProperty = DependencyProperty.Register(
            nameof(MaximizeToolTip),
            typeof(object),
            typeof(OpnxTitlebar),
            new PropertyMetadata("Maximize"));

        public static readonly DependencyProperty RestoreToolTipProperty = DependencyProperty.Register(
            nameof(RestoreToolTip),
            typeof(object),
            typeof(OpnxTitlebar),
            new PropertyMetadata("Restore"));

        public static readonly DependencyProperty CloseToolTipProperty = DependencyProperty.Register(
            nameof(CloseToolTip),
            typeof(object),
            typeof(OpnxTitlebar),
            new PropertyMetadata("Close"));

        public ImageSource? LogoSource
        {
            get => (ImageSource?)GetValue(LogoSourceProperty);
            set => SetValue(LogoSourceProperty, value);
        }

        public double LogoWidth
        {
            get => (double)GetValue(LogoWidthProperty);
            set => SetValue(LogoWidthProperty, value);
        }

        public double LogoHeight
        {
            get => (double)GetValue(LogoHeightProperty);
            set => SetValue(LogoHeightProperty, value);
        }

        public Thickness LogoMargin
        {
            get => (Thickness)GetValue(LogoMarginProperty);
            set => SetValue(LogoMarginProperty, value);
        }

        public object? TitleContent
        {
            get => GetValue(TitleContentProperty);
            set => SetValue(TitleContentProperty, value);
        }

        public DataTemplate? TitleContentTemplate
        {
            get => (DataTemplate?)GetValue(TitleContentTemplateProperty);
            set => SetValue(TitleContentTemplateProperty, value);
        }

        public Thickness TitleContentMargin
        {
            get => (Thickness)GetValue(TitleContentMarginProperty);
            set => SetValue(TitleContentMarginProperty, value);
        }

        public object? RightContent
        {
            get => GetValue(RightContentProperty);
            set => SetValue(RightContentProperty, value);
        }

        public DataTemplate? RightContentTemplate
        {
            get => (DataTemplate?)GetValue(RightContentTemplateProperty);
            set => SetValue(RightContentTemplateProperty, value);
        }

        public Thickness RightContentMargin
        {
            get => (Thickness)GetValue(RightContentMarginProperty);
            set => SetValue(RightContentMarginProperty, value);
        }

        public Thickness ControlBoxMargin
        {
            get => (Thickness)GetValue(ControlBoxMarginProperty);
            set => SetValue(ControlBoxMarginProperty, value);
        }

        public double ButtonSize
        {
            get => (double)GetValue(ButtonSizeProperty);
            set => SetValue(ButtonSizeProperty, value);
        }

        public double ButtonImageSize
        {
            get => (double)GetValue(ButtonImageSizeProperty);
            set => SetValue(ButtonImageSizeProperty, value);
        }

        public Thickness ButtonMargin
        {
            get => (Thickness)GetValue(ButtonMarginProperty);
            set => SetValue(ButtonMarginProperty, value);
        }

        public Brush ButtonForeground
        {
            get => (Brush)GetValue(ButtonForegroundProperty);
            set => SetValue(ButtonForegroundProperty, value);
        }

        public double ButtonMouseOverOpacity
        {
            get => (double)GetValue(ButtonMouseOverOpacityProperty);
            set => SetValue(ButtonMouseOverOpacityProperty, value);
        }

        public double ButtonPressedOpacity
        {
            get => (double)GetValue(ButtonPressedOpacityProperty);
            set => SetValue(ButtonPressedOpacityProperty, value);
        }

        public double ButtonDisabledOpacity
        {
            get => (double)GetValue(ButtonDisabledOpacityProperty);
            set => SetValue(ButtonDisabledOpacityProperty, value);
        }

        public double FallbackGlyphSize
        {
            get => (double)GetValue(FallbackGlyphSizeProperty);
            set => SetValue(FallbackGlyphSizeProperty, value);
        }

        public bool ShowMinimizeButton
        {
            get => (bool)GetValue(ShowMinimizeButtonProperty);
            set => SetValue(ShowMinimizeButtonProperty, value);
        }

        public bool ShowMaximizeButton
        {
            get => (bool)GetValue(ShowMaximizeButtonProperty);
            set => SetValue(ShowMaximizeButtonProperty, value);
        }

        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        public ImageSource? MinimizeImageSource
        {
            get => (ImageSource?)GetValue(MinimizeImageSourceProperty);
            set => SetValue(MinimizeImageSourceProperty, value);
        }

        public ImageSource? MaximizeImageSource
        {
            get => (ImageSource?)GetValue(MaximizeImageSourceProperty);
            set => SetValue(MaximizeImageSourceProperty, value);
        }

        public ImageSource? RestoreImageSource
        {
            get => (ImageSource?)GetValue(RestoreImageSourceProperty);
            set => SetValue(RestoreImageSourceProperty, value);
        }

        public ImageSource? CloseImageSource
        {
            get => (ImageSource?)GetValue(CloseImageSourceProperty);
            set => SetValue(CloseImageSourceProperty, value);
        }

        public object? MinimizeToolTip
        {
            get => GetValue(MinimizeToolTipProperty);
            set => SetValue(MinimizeToolTipProperty, value);
        }

        public object? MaximizeToolTip
        {
            get => GetValue(MaximizeToolTipProperty);
            set => SetValue(MaximizeToolTipProperty, value);
        }

        public object? RestoreToolTip
        {
            get => GetValue(RestoreToolTipProperty);
            set => SetValue(RestoreToolTipProperty, value);
        }

        public object? CloseToolTip
        {
            get => GetValue(CloseToolTipProperty);
            set => SetValue(CloseToolTipProperty, value);
        }

        public bool IsWindowMaximized
        {
            get => _isWindowMaximized;
            private set
            {
                if (_isWindowMaximized == value)
                    return;

                _isWindowMaximized = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentMaximizeToolTip));
            }
        }

        public object? CurrentMaximizeToolTip => IsWindowMaximized ? RestoreToolTip : MaximizeToolTip;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OpnxTitlebar_Loaded(object sender, RoutedEventArgs e)
        {
            _ownerWindow = Window.GetWindow(this);
            if (_ownerWindow == null)
                return;

            _ownerWindow.SourceInitialized += OwnerWindow_SourceInitialized;
            _ownerWindow.StateChanged += OwnerWindow_StateChanged;
            AttachWindowHook();
            UpdateWindowState();
        }

        private void OpnxTitlebar_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_ownerWindow != null)
            {
                _ownerWindow.SourceInitialized -= OwnerWindow_SourceInitialized;
                _ownerWindow.StateChanged -= OwnerWindow_StateChanged;
            }

            DetachWindowHook();
            _ownerWindow = null;
        }

        private void OwnerWindow_SourceInitialized(object? sender, EventArgs e)
        {
            AttachWindowHook();
        }

        private void OwnerWindow_StateChanged(object? sender, EventArgs e)
        {
            UpdateWindowState();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = GetOwnerWindow();
            window?.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            GetOwnerWindow()?.Close();
        }

        private void Titlebar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (e.ClickCount == 2)
            {
                ToggleMaximize();
                return;
            }

            try
            {
                GetOwnerWindow()?.DragMove();
            }
            catch
            {
            }
        }

        private Window? GetOwnerWindow()
        {
            return _ownerWindow ??= Window.GetWindow(this);
        }

        private void ToggleMaximize()
        {
            var window = GetOwnerWindow();
            if (window == null)
                return;

            if (window.ResizeMode is not (ResizeMode.CanResize or ResizeMode.CanResizeWithGrip))
                return;

            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void UpdateWindowState()
        {
            IsWindowMaximized = _ownerWindow?.WindowState == WindowState.Maximized;
        }

        private void AttachWindowHook()
        {
            if (_ownerWindow == null || _hwndSource != null)
                return;

            var windowHandle = new WindowInteropHelper(_ownerWindow).Handle;
            if (windowHandle == IntPtr.Zero)
                return;

            _hwndSource = HwndSource.FromHwnd(windowHandle);
            _hwndSource?.AddHook(WindowProc);
        }

        private void DetachWindowHook()
        {
            _hwndSource?.RemoveHook(WindowProc);
            _hwndSource = null;
        }

        private IntPtr WindowProc(
            IntPtr hwnd,
            int message,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (message == Win32.WM_GETMINMAXINFO)
            {
                UpdateMaximizedSize(hwnd, lParam);
                handled = true;
            }

            return IntPtr.Zero;
        }

        private static void UpdateMaximizedSize(IntPtr hwnd, IntPtr lParam)
        {
            var monitor = Win32.MonitorFromWindow(
                hwnd,
                Win32.MonitorFromWindowFlags.DefaultToNearest);

            if (monitor == IntPtr.Zero ||
                !Win32.TryGetMonitorInfo(monitor, out var monitorInfo))
            {
                return;
            }

            var minMaxInfo = Marshal.PtrToStructure<Win32.MinMaxInfo>(lParam);

            minMaxInfo.PtMaxPosition.X =
                monitorInfo.RcWork.Left - monitorInfo.RcMonitor.Left;

            minMaxInfo.PtMaxPosition.Y =
                monitorInfo.RcWork.Top - monitorInfo.RcMonitor.Top;

            minMaxInfo.PtMaxSize.X =
                monitorInfo.RcWork.Right - monitorInfo.RcWork.Left;

            minMaxInfo.PtMaxSize.Y =
                monitorInfo.RcWork.Bottom - monitorInfo.RcWork.Top;

            Marshal.StructureToPtr(minMaxInfo, lParam, true);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
