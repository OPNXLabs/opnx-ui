using System.Windows;
using System.Windows.Input;

namespace OPNX.UI.WPF.Controls
{
    internal partial class MultiViewPanel
    {
        private const double BoundaryTolerance = 0.000001;
        private const double BoundaryHitDistance = 5;

        private BoundaryResize? _boundaryResize;

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && FindBoundary(e.GetPosition(this)) is BoundaryHit hit)
            {
                if (CaptureMouse())
                {
                    _boundaryResize = CreateBoundaryResize(hit, e.GetPosition(this));
                    Focusable = true;
                    Focus();
                    e.Handled = true;
                }
            }

            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (_boundaryResize is BoundaryResize resize)
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                {
                    CompleteBoundaryResize(false);
                }
                else
                {
                    UpdateBoundaryResize(resize, e.GetPosition(this));
                }
                e.Handled = true;
            }
            base.OnPreviewMouseMove(e);
        }

        private void UpdateBoundaryResize(BoundaryResize resize, Point point)
        {
            double extent = resize.IsVertical ? ActualWidth : ActualHeight;
            if (extent > 0)
            {
                double position = (resize.IsVertical ? point.X : point.Y) * DEFAULT_SIZE / extent;
                double delta = Math.Clamp(position - resize.Start, resize.MinimumDelta, resize.MaximumDelta);
                if (delta == resize.Delta)
                    return;

                resize.Delta = delta;
                foreach (var item in resize.Locations)
                {
                    Rect rect = item.Location;
                    if (resize.IsVertical)
                    {
                        if (item.IsBefore)
                            rect.Width += delta;
                        else
                        {
                            rect.X += delta;
                            rect.Width -= delta;
                        }
                    }
                    else if (item.IsBefore)
                        rect.Height += delta;
                    else
                    {
                        rect.Y += delta;
                        rect.Height -= delta;
                    }
                    SetLocation(item.Cell, rect);
                }

                if (resize.SelectedCells.Count > 0)
                {
                    Rect selection = Rect.Empty;
                    foreach (var cell in resize.SelectedCells)
                        selection.Union(GetLocation(cell));
                    SelectionArea = selection;
                }
                InvalidateMeasure();
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (_boundaryResize != null && e.ChangedButton == MouseButton.Left)
            {
                CompleteBoundaryResize(false);
                e.Handled = true;
            }
            base.OnPreviewMouseUp(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (_boundaryResize != null && e.Key == Key.Escape)
            {
                CompleteBoundaryResize(true);
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            CompleteBoundaryResize(false);
            base.OnLostMouseCapture(e);
        }

        protected override void OnQueryCursor(QueryCursorEventArgs e)
        {
            bool? vertical = _boundaryResize?.IsVertical ?? FindBoundary(Mouse.GetPosition(this))?.IsVertical;
            if (vertical.HasValue)
            {
                e.Cursor = vertical.Value ? Cursors.SizeWE : Cursors.SizeNS;
                e.Handled = true;
            }
            base.OnQueryCursor(e);
        }

        private void CompleteBoundaryResize(bool cancel)
        {
            var resize = _boundaryResize;
            if (resize == null)
                return;

            _boundaryResize = null;
            bool changed = resize.Delta != 0;
            if (cancel)
            {
                foreach (var item in resize.Locations)
                    SetLocation(item.Cell, item.Location);
                SelectionArea = resize.Selection;
                InvalidateMeasure();
            }
            if (IsMouseCaptured)
                ReleaseMouseCapture();
            if (changed && !cancel)
                MultiView?.RaiseLayoutChanged();
            Mouse.UpdateCursor();
        }

        private BoundaryHit? FindBoundary(Point point)
        {
            if (MultiView?.IsBoundaryResizeEnabled != true || ZoomedCell != null || IsBusyForZoom ||
                ActualWidth <= 0 || ActualHeight <= 0)
                return null;

            var cells = new List<Rect>(Children.Count);
            foreach (UIElement child in Children)
            {
                if (child is MultiViewCell cell)
                    cells.Add(GetLocation(cell));
            }
            BoundaryHit? result = null;
            double nearest = BoundaryHitDistance;
            for (int direction = 0; direction < 2; direction++)
            {
                bool vertical = direction == 0;
                double scale = (vertical ? ActualWidth : ActualHeight) / DEFAULT_SIZE;
                double axis = vertical ? point.X : point.Y;
                double cross = (vertical ? point.Y / ActualHeight : point.X / ActualWidth) * DEFAULT_SIZE;
                var positions = new HashSet<double>();
                foreach (var cell in cells)
                {
                    Rect rect = cell;
                    double position = vertical ? rect.Right : rect.Bottom;
                    if (!positions.Add(position))
                        continue;
                    double distance = Math.Abs(axis - position * scale);
                    if (position >= DEFAULT_SIZE - BoundaryTolerance || distance > nearest)
                        continue;

                    // Only shared edges form a boundary. Merge touching intervals along that line.
                    var intervals = new List<(double Start, double End)>();
                    var leadingEdges = new List<Rect>();
                    foreach (Rect location in cells)
                    {
                        if (Math.Abs((vertical ? location.Left : location.Top) - position) <= BoundaryTolerance)
                            leadingEdges.Add(location);
                    }
                    foreach (var left in cells)
                    {
                        Rect a = left;
                        if (Math.Abs((vertical ? a.Right : a.Bottom) - position) > BoundaryTolerance)
                            continue;
                        foreach (var right in leadingEdges)
                        {
                            Rect b = right;
                            double start = Math.Max(vertical ? a.Top : a.Left, vertical ? b.Top : b.Left);
                            double end = Math.Min(vertical ? a.Bottom : a.Right, vertical ? b.Bottom : b.Right);
                            if (end - start > BoundaryTolerance)
                                intervals.Add((start, end));
                        }
                    }
                    intervals.Sort((a, b) => a.Start.CompareTo(b.Start));
                    for (int i = 0; i < intervals.Count; i++)
                    {
                        double start = intervals[i].Start;
                        double end = intervals[i].End;
                        while (i + 1 < intervals.Count && intervals[i + 1].Start <= end + BoundaryTolerance)
                            end = Math.Max(end, intervals[++i].End);
                        if (cross < start || cross > end)
                            continue;

                        result = new BoundaryHit(vertical, position, start, end);
                        nearest = distance;
                        break;
                    }
                }
            }
            return result;
        }

        private BoundaryResize CreateBoundaryResize(BoundaryHit hit, Point point)
        {
            bool vertical = hit.IsVertical;
            double scale = (vertical ? ActualWidth : ActualHeight) / DEFAULT_SIZE;
            var resize = new BoundaryResize
            {
                IsVertical = vertical,
                Start = (vertical ? point.X : point.Y) / scale,
                Selection = SelectionArea,
                SelectedCells = GetSelectedCells()
            };
            double minimumSize = MultiView!.MinimumCellSize / scale;
            foreach (UIElement child in Children)
            {
                if (child is not MultiViewCell cell)
                    continue;

                Rect location = GetLocation(cell);
                double leading = vertical ? location.Left : location.Top;
                double trailing = vertical ? location.Right : location.Bottom;
                double low = vertical ? location.Top : location.Left;
                double high = vertical ? location.Bottom : location.Right;
                if (high <= hit.Start + BoundaryTolerance || low >= hit.End - BoundaryTolerance)
                    continue;
                bool before = Math.Abs(trailing - hit.Position) < BoundaryTolerance;
                bool after = Math.Abs(leading - hit.Position) < BoundaryTolerance;
                if (!before && !after)
                    continue;

                resize.Locations.Add(new BoundaryCell(cell, location, before));
                double size = trailing - leading;
                // Existing smaller tiles may grow, but cannot be made smaller.
                double minimum = Math.Min(size, minimumSize);
                if (before)
                    resize.MinimumDelta = Math.Max(resize.MinimumDelta, minimum - size);
                else
                    resize.MaximumDelta = Math.Min(resize.MaximumDelta, size - minimum);
            }
            return resize;
        }

        private readonly record struct BoundaryHit(bool IsVertical, double Position, double Start, double End);

        private readonly record struct BoundaryCell(MultiViewCell Cell, Rect Location, bool IsBefore);

        private sealed class BoundaryResize
        {
            internal bool IsVertical { get; init; }
            internal double Start { get; init; }
            internal double Delta { get; set; }
            internal double MinimumDelta { get; set; } = double.NegativeInfinity;
            internal double MaximumDelta { get; set; } = double.PositiveInfinity;
            internal Rect Selection { get; init; }
            internal List<MultiViewCell> SelectedCells { get; init; } = [];
            internal List<BoundaryCell> Locations { get; } = [];
        }
    }
}
