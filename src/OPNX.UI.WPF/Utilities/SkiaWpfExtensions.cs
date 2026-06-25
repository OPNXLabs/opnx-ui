using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OPNX.UI.WPF.Utilities
{
    public static class SkiaWpfExtensions
    {
        public static SKBitmap? ToSKBitmap(this BitmapSource? bitmapSource)
        {
            if (bitmapSource == null)
                return null;

            var converted = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, null, 0);
            int width = converted.PixelWidth;
            int height = converted.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            converted.CopyPixels(pixels, stride, 0);

            var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmap.GetPixels(), pixels.Length);
            return bitmap;
        }

        public static SKBitmap? CaptureToSKBitmap(this FrameworkElement element)
        {
            return UIHelper.CaptureElement(element).ToSKBitmap();
        }

        public static WriteableBitmap? ToWriteableBitmap(this SKBitmap? bitmap)
        {
            if (bitmap == null || bitmap.Width <= 0 || bitmap.Height <= 0)
                return null;

            var writeableBitmap = new WriteableBitmap(
                bitmap.Width,
                bitmap.Height,
                96,
                96,
                PixelFormats.Bgra32,
                null);

            writeableBitmap.WritePixels(
                new Int32Rect(0, 0, bitmap.Width, bitmap.Height),
                bitmap.GetPixels(),
                bitmap.RowBytes * bitmap.Height,
                bitmap.RowBytes);

            return writeableBitmap;
        }
    }
}
