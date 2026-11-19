using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace OPNX.UI.WPF.Controls;

public sealed partial class Opnx2DMapControl
{
    #region Private / Protected Methods
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (ActualWidth <= 0 || ActualHeight <= 0) return;
        var dpi = VisualTreeHelper.GetDpi(this);
        int width = Math.Max(1, (int)Math.Ceiling(ActualWidth * dpi.DpiScaleX));
        int height = Math.Max(1, (int)Math.Ceiling(ActualHeight * dpi.DpiScaleY));
        if (_surface == null || _surface.PixelWidth != width || _surface.PixelHeight != height ||
            Math.Abs(_surface.DpiX - dpi.PixelsPerInchX) > .01)
            _surface = new WriteableBitmap(width, height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32, null);
        _surface.Lock();
        try
        {
            using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul),
                _surface.BackBuffer, _surface.BackBufferStride);
            var canvas = surface.Canvas;
            var viewportColor = ViewportBackgroundColor;
            canvas.Clear(new SKColor(viewportColor.R, viewportColor.G, viewportColor.B, viewportColor.A));
            canvas.Scale((float)dpi.DpiScaleX, (float)dpi.DpiScaleY);
            canvas.Translate((float)_offset.X, (float)_offset.Y);
            canvas.Scale((float)_zoom);
            var documentColor = DocumentBackgroundColor;
            using var paper = new SKPaint { Color = new SKColor(documentColor.R, documentColor.G, documentColor.B, documentColor.A) };
            canvas.DrawRect(0, 0, (float)_documentWidth, (float)_documentHeight, paper);
            canvas.Save();
            canvas.ClipRect(new SKRect(0, 0, (float)_documentWidth, (float)_documentHeight));
            if (_backgroundData != null)
            {
                _background ??= Decode(Convert.FromBase64String(_backgroundData));
                canvas.DrawBitmap(_background, 0, 0, new SKSamplingOptions(SKFilterMode.Linear));
            }
            foreach (var item in Items) DrawItem(canvas, item);
            DrawPolylinePreview(canvas);
            canvas.Restore();
            if (IsEditMode && SelectedItem is { } selected) DrawSelection(canvas, selected);
            canvas.Flush();
            _surface.AddDirtyRect(new Int32Rect(0, 0, width, height));
        }
        finally { _surface.Unlock(); }
        drawingContext.DrawImage(_surface, new Rect(0, 0, ActualWidth, ActualHeight));
    }

    private void DrawPolylinePreview(SKCanvas canvas)
    {
        if (!IsDrawingPolyline || _polylinePoints.Count == 0) return;
        using var stroke = new SKPaint { Color = SKColor.Parse("#087FBC"), IsAntialias = true,
            Style = SKPaintStyle.Stroke, StrokeWidth = (float)(2 / _zoom), StrokeCap = SKStrokeCap.Round };
        using var builder = new SKPathBuilder();
        builder.MoveTo((float)_polylinePoints[0].X, (float)_polylinePoints[0].Y);
        foreach (var point in _polylinePoints.Skip(1)) builder.LineTo((float)point.X, (float)point.Y);
        if (_polylinePointer is { } pointer) builder.LineTo((float)pointer.X, (float)pointer.Y);
        using var path = builder.Detach();
        canvas.DrawPath(path, stroke);
        using var fill = new SKPaint { Color = SKColors.White, IsAntialias = true };
        foreach (var point in _polylinePoints)
        {
            canvas.DrawCircle((float)point.X, (float)point.Y, (float)(4 / _zoom), fill);
            canvas.DrawCircle((float)point.X, (float)point.Y, (float)(4 / _zoom), stroke);
        }
    }

    private void DrawItem(SKCanvas canvas, Map2DItem item)
    {
        if (item.Opacity <= 0 || !double.IsFinite(item.Width) || !double.IsFinite(item.Height) || item.Width <= 0 || item.Height <= 0) return;
        canvas.Save();
        try
        {
            canvas.Translate((float)(item.X + item.Width / 2), (float)(item.Y + item.Height / 2));
            canvas.RotateDegrees((float)item.Rotation);
            if (!IsEditMode) canvas.Scale((float)item.DisplayScale);
            canvas.Translate((float)(-item.Width / 2), (float)(-item.Height / 2));
            // Reject clipped items before allocating paints, paths, fonts or decoding images.
            float padding = (float)(item.StrokeWidth * 2 + 2 / _zoom);
            if (canvas.QuickReject(new SKRect(-padding, -padding, (float)item.Width + padding, (float)item.Height + padding))) return;
            float alpha = (float)Math.Clamp(item.Opacity, 0, 1);
            if (item.IsBlinking && Environment.TickCount64 % 1000 < 500) alpha *= .25f;
            SKColor.TryParse(item.Fill, out var fillColor);
            SKColor.TryParse(item.Stroke, out var strokeColor);
            using var fill = new SKPaint { IsAntialias = true, Color = fillColor.WithAlpha((byte)(fillColor.Alpha * alpha)) };
            using var stroke = new SKPaint { IsAntialias = true, Color = strokeColor.WithAlpha((byte)(strokeColor.Alpha * alpha)),
                Style = SKPaintStyle.Stroke, StrokeWidth = (float)Math.Max(0, item.StrokeWidth) };
            var rect = new SKRect(0, 0, (float)item.Width, (float)item.Height);
            switch (item.Kind)
            {
                case Map2DItemKind.Rectangle: canvas.DrawRect(rect, fill); if (item.StrokeWidth > 0) canvas.DrawRect(rect, stroke); break;
                case Map2DItemKind.Ellipse: canvas.DrawOval(rect, fill); if (item.StrokeWidth > 0) canvas.DrawOval(rect, stroke); break;
                case Map2DItemKind.Polyline:
                    if (item.Points is { Length: >= 2 } points && item.StrokeWidth > 0)
                    {
                        using var builder = new SKPathBuilder();
                        builder.MoveTo((float)(points[0].X * item.Width), (float)(points[0].Y * item.Height));
                        for (int i = 1; i < points.Length; i++) builder.LineTo((float)(points[i].X * item.Width), (float)(points[i].Y * item.Height));
                        using var line = builder.Detach();
                        stroke.StrokeCap = SKStrokeCap.Round; stroke.StrokeJoin = SKStrokeJoin.Round;
                        canvas.DrawPath(line, stroke);
                    }
                    break;
                case Map2DItemKind.Line: if (item.StrokeWidth > 0) canvas.DrawLine(0, 0, rect.Right, rect.Bottom, stroke); break;
                case Map2DItemKind.Polygon:
                case Map2DItemKind.Path:
                    using (var path = CreatePath(item))
                    {
                        if (path != null) { canvas.DrawPath(path, fill); if (item.StrokeWidth > 0) canvas.DrawPath(path, stroke); }
                    }
                    break;
                case Map2DItemKind.Image:
                    if (item.ImageData is { } data)
                    {
                        if (!_images.TryGetValue(data, out var image)) _images[data] = image = Decode(Convert.FromBase64String(data));
                        using var paint = new SKPaint { Color = SKColors.White.WithAlpha((byte)(255 * alpha)), IsAntialias = true };
                        canvas.DrawBitmap(image, rect, new SKSamplingOptions(SKFilterMode.Linear), paint);
                        if (item.StrokeWidth > 0) canvas.DrawRect(rect, stroke);
                    }
                    break;
                case Map2DItemKind.Text:
                    canvas.Save(); canvas.ClipRect(rect);
                    using (var typeface = SKTypeface.FromFamilyName(item.FontFamily))
                    using (var font = new SKFont(typeface, (float)item.FontSize))
                    {
                        var metrics = font.Metrics;
                        float y = -metrics.Ascent;
                        foreach (string line in (item.Text ?? "").Replace("\r", "").Split('\n'))
                        {
                            canvas.DrawText(line, 0, y, SKTextAlign.Left, font, fill);
                            if (item.StrokeWidth > 0) canvas.DrawText(line, 0, y, SKTextAlign.Left, font, stroke);
                            y += metrics.Descent - metrics.Ascent + metrics.Leading;
                        }
                    }
                    canvas.Restore(); break;
            }
        }
        finally { canvas.Restore(); }
    }

    private static SKPath? CreatePath(Map2DItem item)
    {
        if (item.Kind == Map2DItemKind.Path)
        {
            var path = SKPath.ParseSvgPathData(item.PathData ?? "");
            if (path == null) return null;
            var bounds = path.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0) { path.Dispose(); return null; }
            path.Transform(SKMatrix.CreateTranslation(-bounds.Left, -bounds.Top));
            path.Transform(SKMatrix.CreateScale((float)item.Width / bounds.Width, (float)item.Height / bounds.Height));
            return path;
        }
        using var polygon = new SKPathBuilder();
        polygon.MoveTo((float)item.Width / 2, 0);
        polygon.LineTo((float)item.Width, (float)item.Height * .38f);
        polygon.LineTo((float)item.Width * .8f, (float)item.Height);
        polygon.LineTo((float)item.Width * .2f, (float)item.Height);
        polygon.LineTo(0, (float)item.Height * .38f);
        polygon.Close();
        return polygon.Detach();
    }

    private void DrawSelection(SKCanvas canvas, Map2DItem item)
    {
        canvas.Save();
        canvas.Translate((float)(item.X + item.Width / 2), (float)(item.Y + item.Height / 2));
        canvas.RotateDegrees((float)item.Rotation);
        canvas.Translate((float)(-item.Width / 2), (float)(-item.Height / 2));
        using var outline = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = SKColor.Parse("#087FBC"), StrokeWidth = (float)(1.5 / _zoom) };
        using var white = new SKPaint { IsAntialias = true, Color = SKColors.White };
        canvas.DrawRect(0, 0, (float)item.Width, (float)item.Height, outline);
        if (!item.IsLocked)
        {
            canvas.DrawLine((float)item.Width / 2, 0, (float)item.Width / 2, (float)(-28 / _zoom), outline);
            canvas.DrawCircle((float)item.Width / 2, (float)(-28 / _zoom), (float)(5 / _zoom), white);
            canvas.DrawCircle((float)item.Width / 2, (float)(-28 / _zoom), (float)(5 / _zoom), outline);
            foreach (var handle in GetHandles(item))
            {
                float radius = (float)(4 / _zoom);
                canvas.DrawCircle((float)handle.X, (float)handle.Y, radius, white);
                canvas.DrawCircle((float)handle.X, (float)handle.Y, radius, outline);
            }
        }
        canvas.Restore();
    }
    #endregion
}
