using SharpDX;
using SharpDX.Direct3D9;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Infrastructure.Rendering
{
    public sealed class D3DManager : IDisposable
    {
        #region Fields
        private Direct3DEx? _d3dContext;
        private DeviceEx? _d3dDevice;
        private PresentParameters _presentParams;

        private int _disposed = 0;     // 0 = false, 1 = true
        private int _initialized = 0;  // 0 = false, 1 = true
        private int _isResetting = 0;  // 0 = false, 1 = true

        private static readonly Lazy<D3DManager> _instance = new(() => new D3DManager());

        private readonly object _resetLock = new();

        private IntPtr _hwnd = IntPtr.Zero;
        private int _width = 0;
        private int _height = 0;

        // Loaded 재시도 1회만
        private int _pendingInitOnLoaded = 0; // 0=false, 1=true
        #endregion

        #region Constructors
        public D3DManager() { }
        #endregion

        #region Properties
        public static D3DManager Instance => _instance.Value;

        public DeviceEx? D3DDevice => _d3dDevice;
        public Direct3DEx? D3DContext => _d3dContext;

        public bool IsInitialized => Volatile.Read(ref _initialized) == 1;
        public bool IsResetting => Volatile.Read(ref _isResetting) == 1;
        #endregion

        #region Public Methods
        public void Initialize()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (Volatile.Read(ref _disposed) == 1)
                        return;

                    var mainWindow = Application.Current.MainWindow;
                    if (mainWindow == null)
                        return;

                    _hwnd = new WindowInteropHelper(mainWindow).Handle;

                    // 레이아웃/로드 타이밍에 따라 0이 나올 수 있음 → Loaded에서 1회 재시도
                    int w = Math.Max(0, (int)Math.Round(mainWindow.ActualWidth));
                    int h = Math.Max(0, (int)Math.Round(mainWindow.ActualHeight));

                    if (_hwnd == IntPtr.Zero)
                        return;

                    if (w > 0 && h > 0)
                    {
                        _width = w;
                        _height = h;
                        Initialize(_hwnd, _width, _height);
                        return;
                    }

                    if (Interlocked.CompareExchange(ref _pendingInitOnLoaded, 1, 0) == 0)
                    {
                        void OnContentRendered(object? sender, EventArgs e)
                        {
                            try
                            {
                                mainWindow.ContentRendered -= OnContentRendered;

                                if (Volatile.Read(ref _disposed) == 1 || IsInitialized)
                                    return;

                                _hwnd = new WindowInteropHelper(mainWindow).Handle;

                                int w = Math.Max(1, (int)Math.Round(mainWindow.ActualWidth));
                                int h = Math.Max(1, (int)Math.Round(mainWindow.ActualHeight));

                                _width = w;
                                _height = h;

                                if (_hwnd != IntPtr.Zero)
                                    Initialize(_hwnd, _width, _height);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("[D3DManager] Initialize() ContentRendered retry error: " + ex);
                            }
                            finally
                            {
                                Interlocked.Exchange(ref _pendingInitOnLoaded, 0);
                            }
                        }

                        mainWindow.ContentRendered += OnContentRendered;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[D3DManager] Initialize() error: " + ex);
                }
            }, DispatcherPriority.Normal);
        }

        public void Initialize(IntPtr hwnd, int width, int height)
        {
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
                return;

            if (Volatile.Read(ref _disposed) == 1)
            {
                Volatile.Write(ref _initialized, 0);
                throw new ObjectDisposedException(nameof(D3DManager), "D3DManager has been disposed before initialization.");
            }

            _hwnd = hwnd;
            _width = Math.Max(1, width);
            _height = Math.Max(1, height);

            try
            {
                _d3dContext = new Direct3DEx();

                _presentParams = new PresentParameters
                {
                    Windowed = true,
                    SwapEffect = SwapEffect.Discard,
                    BackBufferFormat = Format.A8R8G8B8,
                    BackBufferWidth = _width,
                    BackBufferHeight = _height,
                    BackBufferCount = 1,
                    EnableAutoDepthStencil = false,
                    PresentationInterval = PresentInterval.One,
                    DeviceWindowHandle = hwnd
                };

                var createFlags = CreateFlags.Multithreaded | CreateFlags.FpuPreserve;

                if (SystemParameters.IsRemoteSession)
                    createFlags |= CreateFlags.SoftwareVertexProcessing;
                else
                    createFlags |= CreateFlags.HardwareVertexProcessing;

                _d3dDevice = new DeviceEx(
                    _d3dContext,
                    0,
                    DeviceType.Hardware,
                    hwnd,
                    createFlags,
                    _presentParams
                );
            }
            catch (SharpDXException ex)
            {
                // partial init 정리 + 롤백
                try { _d3dDevice?.Dispose(); } catch { }
                _d3dDevice = null;

                try { _d3dContext?.Dispose(); } catch { }
                _d3dContext = null;

                Volatile.Write(ref _initialized, 0);
                throw new InvalidOperationException("Failed to initialize Direct3D9Ex device.", ex);
            }
            catch
            {
                try { _d3dDevice?.Dispose(); } catch { }
                _d3dDevice = null;

                try { _d3dContext?.Dispose(); } catch { }
                _d3dContext = null;

                Volatile.Write(ref _initialized, 0);
                throw;
            }
        }

        public void Resize(int newWidth, int newHeight)
        {
            if (!IsInitialized || Volatile.Read(ref _disposed) == 1)
                return;

            lock (_resetLock)
            {
                if (!IsInitialized || Volatile.Read(ref _disposed) == 1 || _d3dDevice == null)
                    return;

                _width = Math.Max(1, newWidth);
                _height = Math.Max(1, newHeight);

                _presentParams.BackBufferWidth = _width;
                _presentParams.BackBufferHeight = _height;

                Volatile.Write(ref _isResetting, 1);
                try
                {
                    _d3dDevice.ResetEx(ref _presentParams, null);
                }
                catch (SharpDXException ex)
                {
                    Debug.WriteLine($"[D3DManager] Resize ResetEx failed: {ex.ResultCode} / {ex.Message}");
                }
                finally
                {
                    Volatile.Write(ref _isResetting, 0);
                    Monitor.PulseAll(_resetLock);
                }
            }
        }

        public void Reset()
        {
            if (!IsInitialized || Volatile.Read(ref _disposed) == 1)
                return;

            lock (_resetLock)
            {
                if (!IsInitialized || Volatile.Read(ref _disposed) == 1 || _d3dDevice == null)
                    return;

                while (Volatile.Read(ref _isResetting) == 1)
                {
                    if (!Monitor.Wait(_resetLock, 1000))
                    {
                        Debug.WriteLine("[D3DManager] Reset wait timeout");
                        return;
                    }

                    if (!IsInitialized || Volatile.Read(ref _disposed) == 1 || _d3dDevice == null)
                        return;
                }

                Volatile.Write(ref _isResetting, 1);

                try
                {
                    _presentParams.BackBufferWidth = Math.Max(1, _width);
                    _presentParams.BackBufferHeight = Math.Max(1, _height);

                    try
                    {
                        _d3dDevice.ResetEx(ref _presentParams, null);
                    }
                    catch (SharpDXException ex)
                    {
                        Debug.WriteLine($"[D3DManager] ResetEx failed: {ex.ResultCode} / {ex.Message}");
                    }
                }
                finally
                {
                    Volatile.Write(ref _isResetting, 0);
                    Monitor.PulseAll(_resetLock);
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            lock (_resetLock)
            {
                try
                {
                    Volatile.Write(ref _isResetting, 1);

                    try { _d3dDevice?.Dispose(); } catch { }
                    _d3dDevice = null;

                    try { _d3dContext?.Dispose(); } catch { }
                    _d3dContext = null;

                    Volatile.Write(ref _initialized, 0);
                }
                finally
                {
                    Volatile.Write(ref _isResetting, 0);
                    Monitor.PulseAll(_resetLock);
                }
            }

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private / Protected Methods
        private void RecoverDevice()
        {
            if (Volatile.Read(ref _disposed) == 1)
                return;

            lock (_resetLock)
            {
                if (Volatile.Read(ref _disposed) == 1)
                    return;

                Volatile.Write(ref _isResetting, 1);
                try
                {
                    try { _d3dDevice?.Dispose(); } catch { }
                    _d3dDevice = null;

                    try { _d3dContext?.Dispose(); } catch { }
                    _d3dContext = null;

                    Volatile.Write(ref _initialized, 0);
                }
                finally
                {
                    Volatile.Write(ref _isResetting, 0);
                    Monitor.PulseAll(_resetLock);
                }
            }

            Thread.Sleep(100);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (Volatile.Read(ref _disposed) == 1)
                        return;

                    if (_hwnd != IntPtr.Zero && _width > 0 && _height > 0)
                        Initialize(_hwnd, _width, _height);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[D3DManager] RecoverDevice Initialize failed: {ex.Message}");
                }
            }), DispatcherPriority.Normal);
        }
        #endregion
    }
}

