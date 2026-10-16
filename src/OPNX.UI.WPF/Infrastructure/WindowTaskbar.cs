using OPNX.Lib.Common.Platform.Windows;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace OPNX.UI.WPF.Infrastructure
{
    public static class WindowTaskbar
    {
        #region Public Methods
        public static void SetApplicationID(Window window, string applicationID)
        {
            ArgumentNullException.ThrowIfNull(window);
            ArgumentException.ThrowIfNullOrWhiteSpace(applicationID);
            window.Dispatcher.VerifyAccess();

            IntPtr handle = new WindowInteropHelper(window).EnsureHandle();
            Win32.SetWindowApplicationID(handle, applicationID);
            if (HwndSource.FromHwnd(handle) is { } source)
            {
                source.RemoveHook(WindowMessage);
                source.AddHook(WindowMessage);
            }
        }

        public static void Flash(Window window, bool enabled)
        {
            ArgumentNullException.ThrowIfNull(window);
            window.Dispatcher.VerifyAccess();
            Win32.FlashWindow(new WindowInteropHelper(window).Handle, enabled);
        }
        #endregion

        #region Private Methods
        private static IntPtr WindowMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WindowDestroyMessage = 0x0002;
            if (message == WindowDestroyMessage)
            {
                try
                {
                    Win32.SetWindowApplicationID(handle, null);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Failed to clear the window application ID: {0}", ex.Message);
                }
            }
            return IntPtr.Zero;
        }
        #endregion
    }
}
