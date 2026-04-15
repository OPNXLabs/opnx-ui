using FFmpeg.AutoGen;
using OPNX.Lib.Common.Platform.Windows;
using SharpDX;
using SharpDX.Direct3D9;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OPNX.UI.WPF.Controls.OpnxImage
{
    public class OpnxImage : Image, IDisposable
    {
        #region Fields        
        private bool _isDisposed = false;

        private readonly D3DImage? _d3dImage = null;

        private Surface? _frontSurface = null;
        private Surface? _backSurface = null;
        private Format? _backSurfaceFormat = null;

        private readonly ConcurrentQueue<Int32Rect> _dirtyRectQueue = new();

        private byte[]? _backBufferData = null;
        private WriteableBitmap? _backBufferA;
        private WriteableBitmap? _backBufferB;
        private WriteableBitmap? _writeBuffer;

        private readonly Lock _remoteBufferLock = new();
        private int _remoteStride;   // frame->linesize[0] 저장
        private int _remoteWidth;
        private int _remoteHeight;
        #endregion

        #region Constructors
        public OpnxImage()
        {
            _d3dImage = new D3DImage();

            this.Source = _d3dImage;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }
        #endregion

        #region Properties
        public int PixelWidth => _d3dImage?.PixelWidth ?? 0;
        public int PixelHeight => _d3dImage?.PixelHeight ?? 0;
        #endregion

        #region Events
        public event EventHandler? UpdatedFrontSurface;
        #endregion

        #region Public Methods
        public unsafe void UpdateBackSurface(bool isRemoteSession, DeviceEx d3dDevice, AVFrame* frame)
        {
            if (_isDisposed || frame == null)
                return;

            if (isRemoteSession)
            {
                if (_backSurface != null)
                {
                    SetBackBuffer(null);
                    _backSurface.Dispose();
                    _backSurface = null;
                    _backSurfaceFormat = null;
                }

                if ((AVPixelFormat)frame->format != AVPixelFormat.AV_PIX_FMT_BGR24)
                    return;

                if (_backBufferA == null || _backBufferB == null ||
                    _backBufferA.PixelWidth != frame->width || _backBufferA.PixelHeight != frame->height)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //_backBufferA = new WriteableBitmap(1920, 1080, 96, 96, PixelFormats.Bgr24, null);
                        //_backBufferB = new WriteableBitmap(1920, 1080, 96, 96, PixelFormats.Bgr24, null);
                        _backBufferA = new WriteableBitmap(frame->width, frame->height, 96, 96, PixelFormats.Bgr24, null);
                        _backBufferB = new WriteableBitmap(frame->width, frame->height, 96, 96, PixelFormats.Bgr24, null);
                        _writeBuffer = _backBufferA;
                    });
                }

                int width = frame->width;
                int height = frame->height;
                int stride = frame->linesize[0];
                int bufferSize = height * stride;

                _remoteWidth = width;
                _remoteHeight = height;
                _remoteStride = stride;

                if (_backBufferData == null || _backBufferData.Length != bufferSize)
                    _backBufferData = new byte[bufferSize];

                using (_remoteBufferLock.EnterScope())
                {
                    fixed (byte* pDest = _backBufferData)
                    {
                        Win32.MemCopy((IntPtr)pDest, (IntPtr)frame->data[0], (UIntPtr)bufferSize);
                    }
                }
            }
            else
            {
                if (_backSurface == null)
                    return;

                Format format = Format.A8R8G8B8;
                switch ((AVPixelFormat)frame->format)
                {
                    case AVPixelFormat.AV_PIX_FMT_BGR24:
                        format = Format.X8R8G8B8;
                        break;
                    case AVPixelFormat.AV_PIX_FMT_NV12:
                        format = (Format)842094158;
                        break;
                    case AVPixelFormat.AV_PIX_FMT_YUV420P:
                    case AVPixelFormat.AV_PIX_FMT_YUVJ420P:
                        format = (Format)0x32315659;
                        break;
                }

                if (!CreateBackSurface(d3dDevice, format, frame->width, frame->height))
                    return;

                DataRectangle dataRectangle = _backSurface.LockRectangle(LockFlags.None);
                IntPtr dataPointer = dataRectangle.DataPointer;
                int stride = dataRectangle.Pitch;

                try
                {
                    switch (format)
                    {
                        case Format.X8R8G8B8:
                            DrawRGB24(frame, dataPointer, stride);
                            break;
                        case Format.A8R8G8B8:
                            DrawRGBA(frame, dataPointer, stride);
                            break;
                        case (Format)842094158:
                            DrawNV12(frame, dataPointer, stride);
                            break;
                        default:
                            DrawYUV420(frame, dataPointer, stride);
                            break;
                    }
                }
                finally
                {
                    if (_backSurface?.IsDisposed == false)
                    {
                        try { _backSurface.UnlockRectangle(); }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"UnlockRectangle failed: {ex.GetType().Name} / {ex.Message}");
                        }
                    }
                }
            }
        }


        public void UpdateFrontSurface(bool isRemoteSession, DeviceEx d3dDevice)
        {
            if (_isDisposed)
                return;

            if (isRemoteSession)
            {
                _frontSurface?.Dispose();
                _frontSurface = null;

                if (_writeBuffer == null || _backBufferData == null)
                    return;

                WriteableBitmap? nextBuffer = (_writeBuffer == _backBufferA) ? _backBufferB : _backBufferA;

                if (nextBuffer == null)
                    return;

                nextBuffer.Lock();
                try
                {
                    using (_remoteBufferLock.EnterScope())
                    {
                        nextBuffer.WritePixels(
                            new Int32Rect(0, 0, nextBuffer.PixelWidth, nextBuffer.PixelHeight),
                            _backBufferData,
                            _remoteStride,
                            0);
                    }
                }
                finally
                {
                    nextBuffer.Unlock();
                }

                _writeBuffer = nextBuffer;
                this.Source = _writeBuffer;
            }
            else
            {
                if (_backSurface == null)
                    return;

                bool surfaceSizeChanged = _frontSurface == null ||
                                          _backSurface.Description.Width != _frontSurface.Description.Width ||
                                          _backSurface.Description.Height != _frontSurface.Description.Height;

                if (surfaceSizeChanged)
                {
                    _frontSurface?.Dispose();
                    _frontSurface = Surface.CreateRenderTarget(
                        d3dDevice,
                        (int)_backSurface.Description.Width,
                        (int)_backSurface.Description.Height,
                        Format.X8R8G8B8,
                        MultisampleType.None,
                        0,
                        true);

                    SetBackBuffer(_frontSurface);
                }

                try
                {
                    d3dDevice.StretchRectangle(_backSurface, _frontSurface, TextureFilter.Linear);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"StretchRectangle error: {ex.Message}");
                }
            }

            UpdatedFrontSurface?.Invoke(this, EventArgs.Empty);
        }


        public void Rendering(bool isRemoteSession = false)
        {
            if (_isDisposed || isRemoteSession || _d3dImage == null)
                return;

            if (this.Source != _d3dImage)
                this.Source = _d3dImage;

            if (_d3dImage.PixelWidth > 0 && _d3dImage.PixelHeight > 0)
            {
                try
                {
                    _d3dImage.Lock();
                    _d3dImage.AddDirtyRect(new Int32Rect(0, 0, _d3dImage.PixelWidth, _d3dImage.PixelHeight));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Rendering error: {ex.Message}");
                }
                finally
                {
                    _d3dImage.Unlock();
                }
            }
        }


        public void ClearSurface()
        {
            if (_frontSurface == null)
                return;

            SetBackBuffer(null);

            SurfaceDescription description = _frontSurface.Description;
            DataRectangle dataRectangle = _frontSurface.LockRectangle(LockFlags.Discard);
            try
            {
                IntPtr dataPointer = dataRectangle.DataPointer;
                int pitch = dataRectangle.Pitch;
                int width = description.Width;
                int height = description.Height;
                int blackColor = unchecked((int)0xFF000000);

                unsafe
                {
                    int* p = (int*)dataPointer;
                    for (int y = 0; y < height; y++)
                    {
                        int offset = y * (pitch / sizeof(int));
                        for (int x = 0; x < width; x++)
                            p[offset + x] = blackColor;
                    }
                }
            }
            finally
            {
                _frontSurface.UnlockRectangle();
            }

            Rendering();
            this.InvalidateVisual();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            SetBackBuffer(null);
            _frontSurface?.Dispose();
            _backSurface?.Dispose();
            _backSurfaceFormat = null;

            _backBufferA = null;
            _backBufferB = null;
            _writeBuffer = null;

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private / Protected Methods           

        // UI 스레드에서 실행되는 메서드
        //private void SwitchBuffer(WriteableBitmap renderedBuffer)
        //{
        //    if (renderedBuffer == null)
        //        return;

        //    writePtr = renderedBuffer?.BackBuffer ?? IntPtr.Zero;    

        //    // 렌더링된 이미지 추가
        //    renderedBuffer.Lock();
        //    renderedBuffer.AddDirtyRect(new Int32Rect(0, 0, renderedBuffer.PixelWidth, renderedBuffer.PixelHeight));
        //    renderedBuffer.Unlock();

        //    // 이미지 소스를 업데이트


        //}


        //private unsafe void UpdateBitmapSurface(AVFrame* frame)
        //{
        //    if (frame == null || writeableBitmap == null)
        //        return;

        //    int width = frame->width;
        //    int height = frame->height;
        //    int stride = width * 3; // RGB24일 경우

        //    // UI 스레드에서 Lock 및 Unlock 처리
        //    writeableBitmap.Lock();

        //    try
        //    {
        //        byte* srcPtr = frame->data[0]; // AVFrame에 이미지 데이터 시작
        //        IntPtr destPtr = writeableBitmap.BackBuffer;

        //        for (int y = 0; y < height; y++)
        //        {
        //            Buffer.MemoryCopy(
        //                srcPtr + y * frame->linesize[0],
        //                (void*)(destPtr + y * writeableBitmap.BackBufferStride),
        //                writeableBitmap.BackBufferStride,
        //                stride
        //            );
        //        }

        //        // 변경된 영역을 WPF에 알려주기
        //        writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error updating WriteableBitmap: {ex.Message}");
        //    }
        //    finally
        //    {
        //        // 작업이 끝난 후 반드시 Unlock을 호출하여 잠금을 해제
        //        writeableBitmap.Unlock();
        //    }
        //}

        private bool CreateBackSurface(DeviceEx d3dDevice, Format format, int width, int height)
        {
            if (_isDisposed || d3dDevice == null || d3dDevice.IsDisposed)
                return false;

            try
            {
                if (_backSurface == null ||
                    width != _backSurface.Description.Width ||
                    height != _backSurface.Description.Height ||
                    _backSurfaceFormat != format)
                {
                    _backSurface?.Dispose();
                    _backSurface = Surface.CreateOffscreenPlain(d3dDevice, width, height, format, Pool.Default);
                    _backSurfaceFormat = format;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return _backSurface != null;
        }

        private void SetBackBuffer(Surface? surface) // surface가 null 가능
        {
            if (_d3dImage == null)
                return;

            try
            {
                _d3dImage.Lock();
                _d3dImage.SetBackBuffer(
                    D3DResourceType.IDirect3DSurface9,
                    surface != null ? surface.NativePointer : IntPtr.Zero);
            }
            finally
            {
                _d3dImage.Unlock();
            }
        }

        //private void SetBackBuffer<T>(T target)
        //    where T : class
        //{
        //    if (target is not Surface surface)
        //        return;

        //    try
        //    {
        //        _d3dImage.Lock();
        //        _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
        //    }
        //    finally
        //    {
        //        _d3dImage.Unlock();
        //    }
        //}

        private static unsafe void DrawYUV422(AVFrame* frame, IntPtr dest, int pitch)
        {
            int width = frame->width;
            int height = frame->height;

            byte* pY = frame->data[0];
            byte* pU = frame->data[1];
            byte* pV = frame->data[2];

            int yStride = frame->linesize[0];
            int uStride = frame->linesize[1];
            int vStride = frame->linesize[2];

            byte* dstBase = (byte*)dest;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int Y = pY[y * yStride + x];
                    int U = pU[y * uStride + (x / 2)];
                    int V = pV[y * vStride + (x / 2)];

                    int C = Y - 16;
                    int D = U - 128;
                    int E = V - 128;

                    int R = Clamp((298 * C + 409 * E + 128) >> 8);
                    int G = Clamp((298 * C - 100 * D - 208 * E + 128) >> 8);
                    int B = Clamp((298 * C + 516 * D + 128) >> 8);

                    int offset = y * pitch + x * 4;

                    dstBase[offset + 0] = (byte)B;
                    dstBase[offset + 1] = (byte)G;
                    dstBase[offset + 2] = (byte)R;
                    dstBase[offset + 3] = 255; // alpha
                }
            }
        }

        private static int Clamp(int val)
        {
            return val < 0 ? 0 : (val > 255 ? 255 : val);
        }

        private static unsafe void DrawNV12(AVFrame* frame, IntPtr dest, int pitch)
        {
            // NV12 데이터 크기 계산
            int ySize = frame->width * frame->height;
            //int uvSize = ySize / 2;  // UV 합쳐진 크기

            // 목적지 메모리는 D3D9 NV12 surface 레이아웃(Y plane 다음 interleaved UV plane)에 맞춘다.
            IntPtr py = dest;
            IntPtr puv = IntPtr.Add(py, pitch * frame->height);

            // 프레임의 절반 너비와 높이
            int halfWidth = frame->width / 2;
            int halfHeight = frame->height / 2;

            //int pitchHalf = pitch / 2;

            byte* pY = frame->data[0];
            byte* pUV = frame->data[1];

            byte* pyBase = (byte*)py.ToPointer();
            byte* puvBase = (byte*)puv.ToPointer();

            // Y 평면 복사
            for (int y = 0; y < frame->height; y++)
            {
                //Buffer.MemoryCopy(pY + y * frame->linesize[0], pyBase + y * pitch, frame->width, frame->width);
                //Unsafe.CopyBlock(pyBase + y * pitch, pY + y * frame->linesize[0], (uint)frame->width);
                Win32.MemCopy((IntPtr)(pyBase + y * pitch), (IntPtr)(pY + y * frame->linesize[0]), (UIntPtr)frame->width);
                //Unsafe.CopyBlockUnaligned(pyBase + y * pitch, pY + y * frame->linesize[0], (uint)frame->width);
            }

            // UV 평면 복사
            for (int y = 0; y < halfHeight; y++)
            {
                //Buffer.MemoryCopy(pUV + y * frame->linesize[1], puvBase + y * pitch, frame->width, frame->width);
                //Unsafe.CopyBlock(puvBase + y * pitch, pUV + y * frame->linesize[1], (uint)frame->width);
                Win32.MemCopy((IntPtr)(puvBase + y * pitch), (IntPtr)(pUV + y * frame->linesize[1]), (UIntPtr)frame->width);
                //Unsafe.CopyBlockUnaligned(puvBase + y * pitch, pUV + y * frame->linesize[1], (uint)frame->width);
            }
        }

        private static unsafe void DrawNV12_SIMD(AVFrame* frame, IntPtr dest, int pitch)
        {
            IntPtr py = dest;
            IntPtr puv = IntPtr.Add(py, pitch * frame->height);
            byte* pY = frame->data[0];
            byte* pUV = frame->data[1];
            byte* pyBase = (byte*)py.ToPointer();
            byte* puvBase = (byte*)puv.ToPointer();

            int simdWidth = Vector<byte>.Count;

            // Y 평면 복사 (SIMD) - 병렬화 가능
            Parallel.For(0, frame->height, y =>
            {
                byte* src = pY + y * frame->linesize[0];
                byte* dst = pyBase + y * pitch;
                int x = 0;

                // SIMD 복사 - 메모리 정렬 최적화
                for (; x <= frame->width - simdWidth; x += simdWidth)
                {
                    var vec = Unsafe.ReadUnaligned<Vector<byte>>(src + x);
                    Unsafe.WriteUnaligned(dst + x, vec);
                }

                // 남은 부분 - 8바이트씩 처리 (64비트 시스템 최적화)
                for (; x <= frame->width - 8; x += 8)
                {
                    *(ulong*)(dst + x) = *(ulong*)(src + x);
                }

                // 마지막 남은 바이트들
                for (; x < frame->width; x++)
                {
                    dst[x] = src[x];
                }
            });

            // UV 평면 복사 (SIMD) - NV12는 UV가 인터리브됨
            int halfHeight = frame->height / 2;
            Parallel.For(0, halfHeight, y =>
            {
                byte* src = pUV + y * frame->linesize[1];
                byte* dst = puvBase + y * pitch;
                int x = 0;

                for (; x <= frame->width - simdWidth; x += simdWidth)
                {
                    var vec = Unsafe.ReadUnaligned<Vector<byte>>(src + x);
                    Unsafe.WriteUnaligned(dst + x, vec);
                }

                for (; x <= frame->width - 8; x += 8)
                {
                    *(ulong*)(dst + x) = *(ulong*)(src + x);
                }

                for (; x < frame->width; x++)
                {
                    dst[x] = src[x];
                }
            });
        }

        private static unsafe void DrawYUV420(AVFrame* frame, IntPtr dest, int pitch)
        {
            // YUV420P 데이터 크기 계산
            int width = frame->width;
            int height = frame->height;

            // Y, U, V 데이터 포인터 설정
            byte* pY = frame->data[0];
            byte* pU = frame->data[1];
            byte* pV = frame->data[2];

            // 목적지 메모리는 D3D9 YV12 surface 레이아웃(Y plane, V plane, U plane)을 따른다.
            IntPtr py = dest;
            IntPtr pu = IntPtr.Add(py, pitch * height);
            IntPtr pv = IntPtr.Add(pu, (pitch * height) / 4);

            int w2 = width / 2;
            int pitch2 = pitch / 2;
            int halfHeight = height / 2;

            byte* pyBase = (byte*)py.ToPointer();
            byte* puBase = (byte*)pu.ToPointer();
            byte* pvBase = (byte*)pv.ToPointer();

            // Y 평면 복사
            for (int y = 0; y < height; y++)
            {
                //Buffer.MemoryCopy(pY + y * frame->linesize[0], pyBase + y * pitch, width, width);
                //Unsafe.CopyBlock(pyBase + y * pitch, pY + y * frame->linesize[0], (uint)width);
                Win32.MemCopy(IntPtr.Add(py, y * pitch), (IntPtr)pY + y * frame->linesize[0], (UIntPtr)width);
                //Unsafe.CopyBlockUnaligned((byte*)py + y * pitch, pY + y * frame->linesize[0], (uint)width);

            }

            // D3D9 YV12는 Y, V, U 순서이므로 FFmpeg의 Y, U, V 입력을 목적지에 맞게 교차 저장한다.
            for (int y = 0; y < halfHeight; y++)
            {
                //Buffer.MemoryCopy(pU + y * frame->linesize[1], pvBase + y * pitch2, w2, w2);
                //Buffer.MemoryCopy(pV + y * frame->linesize[2], puBase + y * pitch2, w2, w2);
                //Unsafe.CopyBlock(pvBase + y * pitch2, pU + y * frame->linesize[1], (uint)w2);
                //Unsafe.CopyBlock(puBase + y * pitch2, pV + y * frame->linesize[2], (uint)w2);
                Win32.MemCopy(IntPtr.Add(pu, y * pitch2), (IntPtr)pV + y * frame->linesize[1], (UIntPtr)w2);
                Win32.MemCopy(IntPtr.Add(pv, y * pitch2), (IntPtr)pU + y * frame->linesize[2], (UIntPtr)w2);
                //Unsafe.CopyBlockUnaligned((byte*)pu + y * pitch2, pV + y * frame->linesize[1], (uint)w2);
                //Unsafe.CopyBlockUnaligned((byte*)pv + y * pitch2, pU + y * frame->linesize[2], (uint)w2);
            }
        }

        private static unsafe void DrawYUV420_SIMD(AVFrame* frame, IntPtr dest, int pitch)
        {
            int width = frame->width;
            int height = frame->height;
            int w2 = width / 2;
            int pitch2 = pitch / 2;
            int halfHeight = height / 2;
            int simdWidth = Vector<byte>.Count;

            byte* pY = frame->data[0];
            byte* pU = frame->data[1];
            byte* pV = frame->data[2];
            byte* pyBase = (byte*)dest;
            byte* puBase = pyBase + pitch * height;
            byte* pvBase = puBase + (pitch * height) / 4;

            // Y 평면 복사 (SIMD + 병렬화)
            Parallel.For(0, height, y =>
            {
                byte* src = pY + y * frame->linesize[0];
                byte* dst = pyBase + y * pitch;
                int x = 0;

                // SIMD 복사
                for (; x <= width - simdWidth; x += simdWidth)
                {
                    var vec = Unsafe.ReadUnaligned<Vector<byte>>(src + x);
                    Unsafe.WriteUnaligned(dst + x, vec);
                }

                // 8바이트씩 복사
                for (; x <= width - 8; x += 8)
                {
                    *(ulong*)(dst + x) = *(ulong*)(src + x);
                }

                // 남은 바이트들
                for (; x < width; x++)
                {
                    dst[x] = src[x];
                }
            });

            // U, V 평면 복사 (SIMD + 병렬화)
            Parallel.For(0, halfHeight, y =>
            {
                // U 평면 복사 (pV -> puBase)
                byte* srcU = pV + y * frame->linesize[1];
                byte* dstU = puBase + y * pitch2;
                int x = 0;

                for (; x <= w2 - simdWidth; x += simdWidth)
                {
                    var vec = Unsafe.ReadUnaligned<Vector<byte>>(srcU + x);
                    Unsafe.WriteUnaligned(dstU + x, vec);
                }

                for (; x <= w2 - 8; x += 8)
                {
                    *(ulong*)(dstU + x) = *(ulong*)(srcU + x);
                }

                for (; x < w2; x++)
                {
                    dstU[x] = srcU[x];
                }

                // V 평면 복사 (pU -> pvBase)
                byte* srcV = pU + y * frame->linesize[2];
                byte* dstV = pvBase + y * pitch2;
                x = 0;

                for (; x <= w2 - simdWidth; x += simdWidth)
                {
                    var vec = Unsafe.ReadUnaligned<Vector<byte>>(srcV + x);
                    Unsafe.WriteUnaligned(dstV + x, vec);
                }

                for (; x <= w2 - 8; x += 8)
                {
                    *(ulong*)(dstV + x) = *(ulong*)(srcV + x);
                }

                for (; x < w2; x++)
                {
                    dstV[x] = srcV[x];
                }
            });
        }

        private static unsafe void DrawRGBA(AVFrame* frame, IntPtr dest, int pitch)
        {
            int pixel_w_size = frame->width * 4;
            byte* pSrc = (byte*)frame->data[0];
            byte* pDest = (byte*)dest;

            for (int i = 0; i < frame->height; i++)
            {
                //Buffer.MemoryCopy(pSrc, pDest, pixel_w_size, pixel_w_size);
                //Unsafe.CopyBlock(pDest, pSrc, (uint)pixel_w_size);
                Win32.MemCopy((IntPtr)pDest, (IntPtr)pSrc, (UIntPtr)pixel_w_size);
                //Unsafe.CopyBlockUnaligned(pDest, pSrc, (uint)pixel_w_size);

                pDest += pitch;
                pSrc += frame->linesize[0]; // linesize[0]은 각 행의 바이트 수입니다.
            }
        }

        private static unsafe void DrawRGBA_SIMD(AVFrame* frame, IntPtr dest, int pitch)
        {
            int width = frame->width;
            int height = frame->height;
            int pixel_w_size = width * 4;
            byte* pSrc = (byte*)frame->data[0];
            byte* pDest = (byte*)dest;

            int simdWidth = Vector<byte>.Count;

            Parallel.For(0, height, y =>
            {
                byte* srcRow = pSrc + y * frame->linesize[0];
                byte* dstRow = pDest + y * pitch;
                int x = 0;

                // SIMD 벡터로 복사
                for (; x <= pixel_w_size - simdWidth; x += simdWidth)
                {
                    var vec = Unsafe.ReadUnaligned<Vector<byte>>(srcRow + x);
                    Unsafe.WriteUnaligned(dstRow + x, vec);
                }

                // 8바이트씩 복사
                for (; x <= pixel_w_size - 8; x += 8)
                {
                    *(ulong*)(dstRow + x) = *(ulong*)(srcRow + x);
                }

                // 남은 바이트들
                for (; x < pixel_w_size; x++)
                {
                    dstRow[x] = srcRow[x];
                }
            });
        }

        private static unsafe void DrawRGB24(AVFrame* frame, IntPtr dest, int pitch)
        {
            int width = frame->width;
            int height = frame->height;
            int rgb24Stride = frame->linesize[0];

            byte* pSrc = (byte*)frame->data[0];
            byte* pDest = (byte*)dest;

            for (int y = 0; y < height; y++)
            {
                byte* pSrcRow = pSrc + y * rgb24Stride;
                byte* pDestRow = pDest + y * pitch;

                for (int x = 0; x < width; x++)
                {
                    pDestRow[x * 4 + 0] = pSrcRow[x * 3 + 0]; // Blue
                    pDestRow[x * 4 + 1] = pSrcRow[x * 3 + 1]; // Green
                    pDestRow[x * 4 + 2] = pSrcRow[x * 3 + 2]; // Red
                    pDestRow[x * 4 + 3] = 255; // Alpha (fully opaque)
                }
            }
        }

        private static unsafe void DrawRGB24_SIMD(AVFrame* frame, IntPtr dest, int pitch)
        {
            int width = frame->width;
            int height = frame->height;
            int rgbStride = frame->linesize[0];
            byte* pSrc = (byte*)frame->data[0];
            byte* pDest = (byte*)dest;

            Parallel.For(0, height, y =>
            {
                byte* srcRow = pSrc + y * rgbStride;
                byte* dstRow = pDest + y * pitch;
                int x = 0;

                // 4픽셀씩 ulong으로 처리 (가장 효율적)
                for (; x <= width - 4; x += 4)
                {
                    byte* s = srcRow + x * 3;
                    ulong* d = (ulong*)(dstRow + x * 4);

                    // 4픽셀을 2개의 ulong으로 한 번에 처리
                    ulong pixel01 = ((ulong)s[0] | ((ulong)s[1] << 8) | ((ulong)s[2] << 16) | (255UL << 24) |
                                   ((ulong)s[3] << 32) | ((ulong)s[4] << 40) | ((ulong)s[5] << 48) | (255UL << 56));
                    ulong pixel23 = ((ulong)s[6] | ((ulong)s[7] << 8) | ((ulong)s[8] << 16) | (255UL << 24) |
                                   ((ulong)s[9] << 32) | ((ulong)s[10] << 40) | ((ulong)s[11] << 48) | (255UL << 56));

                    d[0] = pixel01;
                    d[1] = pixel23;
                }

                // 남은 픽셀을 uint으로 처리
                for (; x < width; x++)
                {
                    uint* d = (uint*)(dstRow + x * 4);
                    byte* s = srcRow + x * 3;
                    *d = (uint)(s[0] | (s[1] << 8) | (s[2] << 16) | (255 << 24));
                }
            });
        }

        //private void DrawRGB(int width, int height, byte[] rgbData, IntPtr dest, int pitch)
        //{
        //    // 데이터 복사
        //    int bytesPerPixel = 4; // Assuming 32bpp (8 bits per channel for RGBA)
        //    int widthInBytes = (int)width * bytesPerPixel;

        //    // Use parallel processing to copy image data
        //    Parallel.For(0, (int)height, y =>
        //    {
        //        IntPtr rowPointer = IntPtr.Add(dest, y * pitch);
        //        Marshal.Copy(rgbData, y * widthInBytes, rowPointer, widthInBytes);
        //    });
        //}

        //private void DrawRGB(int width, int height, byte[] rgbData, IntPtr dest, int pitch)
        //{
        //    // 데이터 복사
        //    int bytesPerPixel = 4; // Assuming 32bpp (8 bits per channel for RGBA)
        //    int widthInBytes = (int)width * bytesPerPixel;

        //    // Use parallel processing to copy image data
        //    Parallel.For(0, (int)height, y =>
        //    {
        //        IntPtr rowPointer = IntPtr.Add(dest, y * pitch);
        //        Marshal.Copy(rgbData, y * widthInBytes, rowPointer, widthInBytes);
        //    });
        //} 
        #endregion
    }
}


