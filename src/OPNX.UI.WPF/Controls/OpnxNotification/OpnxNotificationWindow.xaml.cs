using OPNX.UI.WPF.Controls.OpnxNotification.Models;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls.OpnxNotification;

internal partial class OpnxNotificationWindow : Window
{
    #region Fields
    private const uint MonitorDefaultToNearest = 2;
    private readonly OpnxNotificationServiceOptions _options;
    #endregion

    #region Constructors
    internal OpnxNotificationWindow(ObservableCollection<OpnxNotification> notifications, OpnxNotificationServiceOptions options)
    {
        Notifications = notifications;
        _options = options;
        ItemMargin = new Thickness(0, 0, 0, options.ItemSpacing);
        Width = options.Width;

        InitializeComponent();
        SizeChanged += (_, _) => UpdatePosition();
    }
    #endregion

    #region Properties
    public ObservableCollection<OpnxNotification> Notifications { get; }

    public Thickness ItemMargin { get; }
    #endregion

    #region Internal Methods
    internal void UpdatePositionAfterLayout() => Dispatcher.BeginInvoke(UpdatePosition, DispatcherPriority.Loaded);
    #endregion

    #region Private Methods
    private void OnNotificationMouseEnter(object sender, MouseEventArgs e)
    {
        if (_options.PauseOnMouseOver && sender is FrameworkElement { DataContext: OpnxNotification notification })
            notification.Pause();
    }

    private void OnNotificationMouseLeave(object sender, MouseEventArgs e)
    {
        if (_options.PauseOnMouseOver && sender is FrameworkElement { DataContext: OpnxNotification notification })
            notification.Resume();
    }

    private void UpdatePosition()
    {
        Rect workArea = GetWorkArea();
        double margin = Math.Max(0, _options.ScreenEdgeMargin);

        Left = _options.Position is OpnxNotificationPosition.TopLeft or OpnxNotificationPosition.BottomLeft
            ? workArea.Left + margin
            : workArea.Right - ActualWidth - margin;
        Top = _options.Position is OpnxNotificationPosition.TopLeft or OpnxNotificationPosition.TopRight
            ? workArea.Top + margin
            : workArea.Bottom - ActualHeight - margin;
    }

    private Rect GetWorkArea()
    {
        Window? mainWindow = Application.Current?.MainWindow;
        if (_options.Screen != OpnxNotificationScreen.ApplicationWindow || mainWindow is null)
            return SystemParameters.WorkArea;

        IntPtr windowHandle = new WindowInteropHelper(mainWindow).Handle;
        if (windowHandle == IntPtr.Zero)
            return SystemParameters.WorkArea;

        IntPtr monitorHandle = MonitorFromWindow(windowHandle, MonitorDefaultToNearest);
        MonitorInfo monitorInfo = new() { Size = Marshal.SizeOf<MonitorInfo>() };
        if (monitorHandle == IntPtr.Zero || !GetMonitorInfo(monitorHandle, ref monitorInfo))
            return SystemParameters.WorkArea;

        PresentationSource? source = PresentationSource.FromVisual(mainWindow);
        double scaleX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1;
        double scaleY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1;
        NativeRect area = monitorInfo.WorkArea;

        return new Rect(
            area.Left / scaleX,
            area.Top / scaleY,
            (area.Right - area.Left) / scaleX,
            (area.Bottom - area.Top) / scaleY);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr windowHandle, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitorHandle, ref MonitorInfo monitorInfo);
    #endregion

    #region Nested Types
    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRect Monitor;
        public NativeRect WorkArea;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    #endregion
}
