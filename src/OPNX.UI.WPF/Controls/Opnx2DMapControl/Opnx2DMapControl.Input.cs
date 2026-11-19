using System.Windows;
using System.Windows.Input;
using SkiaSharp;

namespace OPNX.UI.WPF.Controls;

public sealed partial class Opnx2DMapControl
{
    #region Fields
    private static readonly Lazy<Cursor> RotationCursor = new(() =>
    {
        using var stream = typeof(Opnx2DMapControl).Assembly.GetManifestResourceStream(
            "OPNX.UI.WPF.Controls.Opnx2DMapControl.Resources.rotate.cur")
            ?? throw new InvalidOperationException("The rotation cursor resource is missing.");
        return new Cursor(stream, true);
    });

    private enum Gesture { None, Pan, Move, Resize, Rotate, Click }
    private Gesture _gesture;
    private Point _startPointer, _startDocument;
    private Vector _startOffset;
    private string? _beforeGesture;
    private Map2DItem? _gestureItem;
    private double _startX, _startY, _startWidth, _startHeight, _startRotation;
    private int _handle = -1, _clickCount;
    private bool _dragged;
    private readonly List<Point> _polylinePoints = [];
    private Point? _polylinePointer;

    #endregion

    #region Methods
    /// <summary>Starts a new point-by-point polyline. Completion adds one undoable item.</summary>
    public void BeginPolyline()
    {
        VerifyAccess();
        if (!IsEditMode) throw new InvalidOperationException("Drawing requires edit mode.");
        CancelInteraction(); CancelPolyline();
        SelectedItem = null;
        IsDrawingPolyline = true;
        Cursor = Cursors.Cross;
        DrawingModeChanged?.Invoke(this, EventArgs.Empty);
        Focus(); InvalidateVisual();
    }

    /// <summary>Adds a vertex in document coordinates while drawing.</summary>
    public void AddPolylinePoint(Point documentPoint)
    {
        VerifyAccess();
        if (!IsDrawingPolyline) throw new InvalidOperationException("Polyline drawing is not active.");
        if (!double.IsFinite(documentPoint.X) || !double.IsFinite(documentPoint.Y)) throw new ArgumentOutOfRangeException(nameof(documentPoint));
        if (_polylinePoints.Count >= 4096) throw new InvalidOperationException("Too many polyline points.");
        var point = new Point(Math.Clamp(documentPoint.X, 0, _documentWidth), Math.Clamp(documentPoint.Y, 0, _documentHeight));
        if (_polylinePoints.Count == 0 || (_polylinePoints[^1] - point).Length > .001) _polylinePoints.Add(point);
        _polylinePointer = point;
        InvalidateVisual();
    }

    public Map2DItem? CompletePolyline()
    {
        VerifyAccess();
        if (!IsDrawingPolyline) return null;
        var points = _polylinePoints.ToArray();
        CancelPolyline();
        if (points.Length < 2) return null;
        double x = points.Min(p => p.X), y = points.Min(p => p.Y);
        double width = Math.Max(1, points.Max(p => p.X) - x), height = Math.Max(1, points.Max(p => p.Y) - y);
        var item = new Map2DItem
        {
            Kind = Map2DItemKind.Polyline, X = x, Y = y, Width = width, Height = height,
            Fill = "#00000000", Stroke = "#087FBC", StrokeWidth = 2,
            Points = points.Select(p => new Map2DPoint((p.X - x) / width, (p.Y - y) / height)).ToArray()
        };
        AddItem(item);
        return item;
    }

    public void CancelPolyline()
    {
        VerifyAccess();
        bool drawing = IsDrawingPolyline;
        IsDrawingPolyline = false;
        _polylinePoints.Clear(); _polylinePointer = null;
        if (!drawing) return;
        Cursor = Cursors.Arrow;
        InvalidateVisual();
        DrawingModeChanged?.Invoke(this, EventArgs.Empty);
    }

    private static Point[] GetHandles(Map2DItem item) =>
    [new(0, 0), new(item.Width / 2, 0), new(item.Width, 0), new(item.Width, item.Height / 2),
     new(item.Width, item.Height), new(item.Width / 2, item.Height), new(0, item.Height), new(0, item.Height / 2)];

    private Point ToLocal(Map2DItem item, Point point, bool useDisplayScale = true)
    {
        Vector delta = point - new Point(item.X + item.Width / 2, item.Y + item.Height / 2);
        if (!IsEditMode && useDisplayScale) delta /= item.DisplayScale;
        double a = -item.Rotation * Math.PI / 180;
        return new Point(delta.X * Math.Cos(a) - delta.Y * Math.Sin(a) + item.Width / 2,
            delta.X * Math.Sin(a) + delta.Y * Math.Cos(a) + item.Height / 2);
    }

    private int HitTestHandle(Map2DItem item, Point documentPoint)
    {
        Point local = ToLocal(item, documentPoint);
        if ((local - new Point(item.Width / 2, -28 / _zoom)).Length <= 9 / _zoom)
            return -2; // Rotation handle.
        var handles = GetHandles(item);
        for (int i = 0; i < handles.Length; i++)
            if ((local - handles[i]).Length <= 8 / _zoom) return i;
        return -1;
    }

    private static Cursor GetResizeCursor(int handle, double rotation)
    {
        double angle = rotation + (handle switch
        {
            0 or 4 => 45,
            1 or 5 => 90,
            2 or 6 => 135,
            _ => 0
        });
        int direction = (int)Math.Floor(((angle % 180 + 180) % 180 + 22.5) / 45) % 4;
        return direction switch
        {
            0 => Cursors.SizeWE,
            1 => Cursors.SizeNWSE,
            2 => Cursors.SizeNS,
            _ => Cursors.SizeNESW
        };
    }

    private void UpdateHoverCursor(Point viewPoint)
    {
        Cursor = Cursors.Arrow;
        if (IsDrawingPolyline) { Cursor = Cursors.Cross; return; }
        if (!IsEditMode || SelectedItem is not { IsLocked: false } item) return;
        int handle = HitTestHandle(item, ViewToDocument(viewPoint));
        Cursor = handle == -2 ? RotationCursor.Value : handle >= 0 ? GetResizeCursor(handle, item.Rotation) : Cursors.Arrow;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_gesture == Gesture.None) Cursor = Cursors.Arrow;
    }

    public Map2DItem? HitTestItem(Point viewPoint)
    {
        VerifyAccess();
        var point = ViewToDocument(viewPoint);
        if (point.X < 0 || point.Y < 0 || point.X > _documentWidth || point.Y > _documentHeight) return null;
        for (int index = Items.Count - 1; index >= 0; index--)
        {
            var item = Items[index];
            if (item.Opacity <= 0) continue;
            double tolerance = Math.Max(5 / _zoom, item.StrokeWidth / 2);
            if (HitTestLocal(item, ToLocal(item, point), tolerance) ||
                (!IsEditMode && item.DisplayScale != 1 && HitTestLocal(item, ToLocal(item, point, false), tolerance))) return item;
        }
        return null;
    }

    private static bool HitTestLocal(Map2DItem item, Point p, double tolerance)
    {
        if (item.Kind == Map2DItemKind.Polyline && item.Points is { } points)
        {
            for (int i = 1; i < points.Length; i++)
            {
                var a = new Point(points[i - 1].X * item.Width, points[i - 1].Y * item.Height);
                var b = new Point(points[i].X * item.Width, points[i].Y * item.Height);
                Vector segment = b - a;
                double t = segment.LengthSquared == 0 ? 0 : Math.Clamp(Vector.Multiply(p - a, segment) / segment.LengthSquared, 0, 1);
                if ((p - (a + segment * t)).Length <= tolerance) return true;
            }
        }
        else if (item.Kind == Map2DItemKind.Line)
        {
            double length = item.Width * item.Width + item.Height * item.Height;
            double t = Math.Clamp((p.X * item.Width + p.Y * item.Height) / length, 0, 1);
            if ((p - new Point(item.Width * t, item.Height * t)).Length <= tolerance) return true;
        }
        else if (item.Kind == Map2DItemKind.Ellipse)
        {
            double x = (p.X - item.Width / 2) / (item.Width / 2 + tolerance);
            double y = (p.Y - item.Height / 2) / (item.Height / 2 + tolerance);
            if (x * x + y * y <= 1) return true;
        }
        else if (item.Kind is Map2DItemKind.Path or Map2DItemKind.Polygon)
        {
            using var path = CreatePath(item);
            if (path?.Contains((float)p.X, (float)p.Y) == true) return true;
        }
        else if (p.X >= -tolerance && p.Y >= -tolerance && p.X <= item.Width + tolerance && p.Y <= item.Height + tolerance) return true;
        return false;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_gesture == Gesture.None) ZoomAt(Math.Pow(1.15, e.Delta / 120.0), e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (IsDrawingPolyline && e.ChangedButton == MouseButton.Right) { CancelPolyline(); e.Handled = true; return; }
        if (IsDrawingPolyline && e.ChangedButton == MouseButton.Left && !Keyboard.IsKeyDown(Key.Space))
        {
            if (e.ClickCount >= 2 || _polylinePoints.Count >= 4096) CompletePolyline();
            else AddPolylinePoint(ViewToDocument(e.GetPosition(this)));
            e.Handled = true;
            return;
        }
        if (_gesture != Gesture.None || e.ChangedButton is not (MouseButton.Left or MouseButton.Middle)) return;
        Focus();
        _startPointer = e.GetPosition(this); _startDocument = ViewToDocument(_startPointer);
        _startOffset = _offset; _dragged = false; _clickCount = e.ClickCount; _handle = -1;
        _gestureItem = null;
        if (e.ChangedButton == MouseButton.Middle || Keyboard.IsKeyDown(Key.Space)) _gesture = Gesture.Pan;
        else
        {
            if (IsEditMode && SelectedItem is { IsLocked: false } selected)
            {
                _handle = HitTestHandle(selected, _startDocument);
                if (_handle == -2) _gesture = Gesture.Rotate;
                else if (_handle >= 0) _gesture = Gesture.Resize;
                if (_gesture is Gesture.Rotate or Gesture.Resize) _gestureItem = selected;
            }
            if (_gestureItem == null)
            {
                _gestureItem = HitTestItem(_startPointer);
                SelectedItem = _gestureItem;
                _gesture = _gestureItem == null ? Gesture.Pan : IsEditMode && !_gestureItem.IsLocked ? Gesture.Move : Gesture.Click;
            }
        }
        if (_gestureItem is { } item)
        {
            _beforeGesture = _gesture is Gesture.Move or Gesture.Resize or Gesture.Rotate ? SaveDocument() : null;
            _startX = item.X; _startY = item.Y; _startWidth = item.Width; _startHeight = item.Height; _startRotation = item.Rotation;
        }
        CaptureMouse(); e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        Point view = e.GetPosition(this);
        if (IsDrawingPolyline && _gesture == Gesture.None)
        {
            _polylinePointer = ViewToDocument(view); Cursor = Cursors.Cross;
            InvalidateVisual(); e.Handled = true; return;
        }
        if (_gesture == Gesture.None)
        {
            ToolTip = HitTestItem(view)?.ToolTip;
            UpdateHoverCursor(view);
            return;
        }
        Vector viewDelta = view - _startPointer;
        if (!_dragged && viewDelta.Length < 3) return;
        _dragged = true;
        if (_gesture == Gesture.Pan) { _offset = _startOffset + viewDelta; Cursor = Cursors.ScrollAll; NotifyView(); }
        else if (_gestureItem is { } item)
        {
            Point point = ViewToDocument(view);
            if (_gesture == Gesture.Move)
            {
                Vector delta = point - _startDocument; item.X = _startX + delta.X; item.Y = _startY + delta.Y; Cursor = Cursors.SizeAll;
            }
            else if (_gesture == Gesture.Rotate)
            {
                var center = new Point(_startX + _startWidth / 2, _startY + _startHeight / 2);
                Vector start = _startDocument - center, current = point - center;
                double angle = _startRotation + (Math.Atan2(current.Y, current.X) - Math.Atan2(start.Y, start.X)) * 180 / Math.PI;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) angle = Math.Round(angle / 15) * 15;
                item.Rotation = (angle % 360 + 360) % 360; Cursor = RotationCursor.Value;
            }
            else if (_gesture == Gesture.Resize) ResizeItem(item, point, Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
        }
        e.Handled = true;
    }

    private void ResizeItem(Map2DItem item, Point point, bool keepRatio)
    {
        // Work in the original rotated frame; the opposite edge/corner stays fixed.
        double a = _startRotation * Math.PI / 180, c = Math.Cos(a), s = Math.Sin(a);
        Vector delta = point - _startDocument;
        double dx = delta.X * c + delta.Y * s, dy = -delta.X * s + delta.Y * c;
        bool left = _handle is 0 or 6 or 7, right = _handle is 2 or 3 or 4;
        bool top = _handle is 0 or 1 or 2, bottom = _handle is 4 or 5 or 6;
        double width = Math.Max(2, _startWidth + (left ? -dx : right ? dx : 0));
        double height = Math.Max(2, _startHeight + (top ? -dy : bottom ? dy : 0));
        if (keepRatio && (left || right) && (top || bottom))
        {
            double scale = Math.Max(width / _startWidth, height / _startHeight);
            width = _startWidth * scale; height = _startHeight * scale;
        }
        double shiftX = (left ? -1 : right ? 1 : 0) * (width - _startWidth) / 2;
        double shiftY = (top ? -1 : bottom ? 1 : 0) * (height - _startHeight) / 2;
        double centerX = _startX + _startWidth / 2 + shiftX * c - shiftY * s;
        double centerY = _startY + _startHeight / 2 + shiftX * s + shiftY * c;
        item.X = centerX - width / 2; item.Y = centerY - height / 2; item.Width = width; item.Height = height;
        Cursor = GetResizeCursor(_handle, _startRotation);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (_gesture == Gesture.None || (e.ChangedButton != MouseButton.Left && e.ChangedButton != MouseButton.Middle)) return;
        var item = _gestureItem; bool dragged = _dragged; string? before = _beforeGesture;
        _gesture = Gesture.None; _gestureItem = null; _beforeGesture = null;
        ReleaseMouseCapture(); UpdateHoverCursor(e.GetPosition(this));
        if (dragged && before != null) Commit(before);
        if (!dragged && item != null)
        {
            var args = new Map2DItemEventArgs(item, ViewToDocument(e.GetPosition(this)), _clickCount);
            ItemClick?.Invoke(this, args);
            if (_clickCount == 2) ItemDoubleClick?.Invoke(this, args);
        }
        e.Handled = true;
    }

    protected override void OnLostMouseCapture(MouseEventArgs e) { base.OnLostMouseCapture(e); CancelInteraction(); }

    public void CancelInteraction()
    {
        VerifyAccess();
        string? before = _beforeGesture;
        bool restore = _dragged && before != null;
        if (_gesture == Gesture.Pan) { _offset = _startOffset; NotifyView(); }
        _gesture = Gesture.None; _gestureItem = null; _beforeGesture = null; _dragged = false;
        if (IsMouseCaptured) ReleaseMouseCapture();
        if (restore) Restore(before!, false);
        Cursor = Cursors.Arrow;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (IsDrawingPolyline && e.Key is Key.Enter or Key.Escape or Key.Back)
        {
            if (e.Key == Key.Enter) CompletePolyline();
            else if (e.Key == Key.Escape) CancelPolyline();
            else if (_polylinePoints.Count > 0) { _polylinePoints.RemoveAt(_polylinePoints.Count - 1); InvalidateVisual(); }
            e.Handled = true; return;
        }
        bool control = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        if (e.Key == Key.Escape) { CancelInteraction(); e.Handled = true; }
        else if (IsEditMode && control && e.Key == Key.Z) { Undo(); e.Handled = true; }
        else if (IsEditMode && control && e.Key == Key.Y) { Redo(); e.Handled = true; }
        else if (IsEditMode && e.Key == Key.Delete) { DeleteSelected(); e.Handled = true; }
        else if (IsEditMode && SelectedItem is { IsLocked: false } && e.Key is Key.Left or Key.Right or Key.Up or Key.Down)
        {
            double step = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? 10 : 1;
            UpdateSelected(item => { item.X += e.Key == Key.Left ? -step : e.Key == Key.Right ? step : 0;
                item.Y += e.Key == Key.Up ? -step : e.Key == Key.Down ? step : 0; });
            e.Handled = true;
        }
    }
    #endregion
}
