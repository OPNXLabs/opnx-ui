using OPNX.UI.WPF.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls
{
    internal class MultiViewPanel : Canvas
    {
        #region Constants and Fields

        private const double DEFAULT_SIZE = 100000;

        private MultiViewCell? _controlledCell;

        private Rect _selectionArea = Rect.Empty;

        private MultiViewCell? _zoomedCell;
        private UIElement? _zoomedElement = null;

        public bool IsHoldButtonClicked = false;

        private int _rowCount = 0;
        private int _columnCount = 0;

        private DateTime _zoomCanvasLastClickTime = DateTime.MinValue;
        private const int DoubleClickThreshold = 300;

        private readonly System.Diagnostics.Stopwatch _doubleTapStopwatch = new();
        private Point _lastTapLocation;

        #endregion

        #region Events

        internal event EventHandler<CellAddedArgs>? CellAdded;

        internal event EventHandler<CellRemovedArgs>? CellRemoved;

        internal event EventHandler<ControlledCellChangedEventArgs>? ControlledCellChanged;

        internal event EventHandler<SelectionChangedArgs>? SelectionChanged;

        internal event EventHandler<ZoomedCellChangeEventArgs>? ZoomedCellChanged;

        internal void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, new SelectionChangedArgs(this.GetActualSelectionArea(), this.GetCells(this._selectionArea)));
        }

        internal event EventHandler<ZoomedCellChangeEventArgs>? ZoomedCellChanging;
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty ZoomCanvasProperty =
            DependencyProperty.Register(nameof(ZoomCanvas), typeof(Canvas), typeof(MultiViewPanel),
                                        new PropertyMetadata(null, OnZoomCanvasChanged));
        #endregion

        #region Properties
        public int RowCount => _rowCount;
        public int ColumnCount => _columnCount;


        public Canvas ZoomCanvas
        {
            get => (Canvas)GetValue(ZoomCanvasProperty);
            set => SetValue(ZoomCanvasProperty, value);
        }

        public bool IsBusyForZoom { get; private set; }

        internal MultiViewCell? ControlledCell
        {
            get => this._controlledCell;
            set
            {
                if (this._controlledCell != value)
                {
                    this.SetControlledCell(value);
                }
            }
        }

        internal bool IsSelectionEnabled { get; private set; }

        internal MultiViewCell? ZoomedCell
        {
            get => this._zoomedCell;
            set
            {
                if (ReferenceEquals(this._zoomedCell, value) || this.IsBusyForZoom)
                    return;

                this.SetZoomedCell(value);
            }
        }

        internal UIElement? ZoomedElement => this._zoomedElement;

        private double CellWidthRatio => this.ActualWidth / DEFAULT_SIZE;

        private double CellHeightRatio => this.ActualHeight / DEFAULT_SIZE;

        private Rect SelectionArea
        {
            get => this._selectionArea;
            set
            {
                if (this._selectionArea != value)
                {
                    this._selectionArea = value;
                    OnSelectionChanged();
                }
            }
        }

        #endregion

        #region Public Properties

        public MultiViewCell? MouseDownCell { get; private set; }

        public OpnxMultiView? MultiView { get; set; }

        #endregion // Public Properties

        #region Public Methods

        public void SelectCell(MultiViewCell cell)
        {
            var rect = GetLocation(cell);
            this.SelectionArea = rect;
        }
        #endregion

        #region Methods
        private static void OnZoomCanvasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiViewPanel grid && e.NewValue is Canvas canvas)
            {
                canvas.MouseLeftButtonDown += grid.Canvas_MouseLeftButtonDown;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ZoomCanvas == null)
                return;

            var now = DateTime.Now;
            var interval = (now - _zoomCanvasLastClickTime).TotalMilliseconds;

            if (interval < DoubleClickThreshold)
            {
                if (_zoomedCell == null)
                    return;

                SetZoomedCell(null);
            }

            _zoomCanvasLastClickTime = now;
        }

        internal static Rect GetLocation(MultiViewCell? cell)
        {
            if (cell == null)
            {
                return new Rect(0, 0, 0, 0);
            }

            var result = new Rect
            {
                X = Canvas.GetLeft(cell),
                Y = Canvas.GetTop(cell),
                Width = Canvas.GetRight(cell) - Canvas.GetLeft(cell),
                Height = Canvas.GetBottom(cell) - Canvas.GetTop(cell)
            };

            return result;
        }

        internal static void SetLocation(MultiViewCell cell, Rect rect)
        {
            Canvas.SetLeft(cell, rect.Left);
            Canvas.SetTop(cell, rect.Top);
            Canvas.SetRight(cell, rect.Right);
            Canvas.SetBottom(cell, rect.Bottom);
        }

        internal Rect GetActualRect(MultiViewCell cell)
        {
            if (cell == null)
            {
                return Rect.Empty;
            }

            var rect = GetLocation(cell);

            return new Rect
            {
                X = this.CellWidthRatio * rect.Left,
                Y = this.CellHeightRatio * rect.Top,
                Width = this.CellWidthRatio * rect.Width,
                Height = this.CellHeightRatio * rect.Height
            };
        }

        internal void AddCell(MultiViewCell cell, Rect rect, bool isBack = false)
        {
            SetLocation(cell, rect);

            cell.Background = Brushes.Transparent;

            cell.PreviewMouseDown += this.Cell_PreviewMouseDown;

            cell.MouseEnter += this.Cell_MouseEnter;
            cell.SelectedCell += this.Cell_SelectedCell;

            cell.PreviewTouchDown += this.Cell_PreviewTouchDown;
            cell.TouchEnter += this.Cell_TouchEnter;
            cell.TouchDown += this.Cell_TouchDown;

            if (isBack)
            {
                this.Children.Insert(0, cell);
            }
            else
            {
                this.Children.Add(cell);
            }

            CellAdded?.Invoke(this, new CellAddedArgs([cell]));
        }

        internal void Clear()
        {
            this.ClearInternal();

            this.SelectionArea = Rect.Empty;
            this.IsSelectionEnabled = false;
        }


        internal MultiViewCell? GetCellByPercent(double x, double y)
        {
            if (x < 0.0 || y < 0.0) return null;

            double posX = this.ActualWidth * x / 100.0;
            double posY = this.ActualHeight * y / 100.0;

            for (int i = 0; i < this.Children.Count; i++)
            {
                if (this.Children[i] is MultiViewCell cell)
                {
                    if (GetActualRect(cell).Contains(posX, posY))
                    {
                        return cell;
                    }
                }
            }

            return null;
        }

        internal Rect GetActualSelectionArea()
        {
            var result = new Rect();
            if (!this._selectionArea.IsEmpty && this._selectionArea.Width > 0 && this._selectionArea.Height > 0)
            {
                result.X = this.CellWidthRatio * this._selectionArea.Left;
                result.Y = this.CellHeightRatio * this._selectionArea.Top;
                result.Width = this.CellWidthRatio * this._selectionArea.Width;
                result.Height = this.CellHeightRatio * this._selectionArea.Height;
            }
            else
            {
                result = Rect.Empty;
            }

            return result;
        }

        internal Rect GetActualMaxRect(MultiViewCell cell)
        {
            if (cell == null)
            {
                return Rect.Empty;
            }

            var cellRect = this.GetActualRect(cell);
            return this.GetActualZoomedLocationByRect(cellRect);
        }

        internal List<MultiViewCell> GetAllCells()
        {
            return [.. Children.OfType<MultiViewCell>()];
        }

        internal List<MultiViewCell> GetSelectedCells()
        {
            return this.IsCellSelected() ? this.GetCells(this._selectionArea) : [];
        }

        internal (int _rowCount, int _columnCount) GetSelectionGridCount()
        {
            if (!this.IsCellSelected())
            {
                return (-1, -1);
            }

            List<MultiViewCell> cells = this.GetCells(this._selectionArea);
            int cellCount = cells.Count;

            if (cellCount == 0)
            {
                return (-1, -1);
            }

            var lefts = new HashSet<double>(cellCount);
            var tops = new HashSet<double>(cellCount);

            for (int i = 0; i < cellCount; i++)
            {
                var loc = GetLocation(cells[i]);
                lefts.Add(loc.Left);
                tops.Add(loc.Top);
            }

            return (cellCount == lefts.Count * tops.Count) ? (tops.Count, lefts.Count) : (-1, -1);

        }
        internal int GetSelectionColumnCount() => GetSelectionGridCount()._columnCount;


        internal int GetSelectionRowCount() => GetSelectionGridCount()._rowCount;
        internal MultiViewCell? AddRowAndResizeCells()
        {
            SelectionClear();

            var allCells = GetAllCells();

            double oldCellWidth = DEFAULT_SIZE / _columnCount;
            double oldCellHeight = DEFAULT_SIZE / _rowCount;

            _rowCount += 1;

            double newCellWidth = DEFAULT_SIZE / _columnCount;
            double newCellHeight = DEFAULT_SIZE / _rowCount;

            this.Children.Clear();
            foreach (var cell in allCells)
            {
                Rect cellLocation = GetLocation(cell);

                double cellRowSpan = cellLocation.Height / oldCellHeight;
                cellLocation.Height = newCellHeight * cellRowSpan;


                if (cellLocation.Y > 0)
                    cellLocation.Y = (cellLocation.Y / oldCellHeight) * newCellHeight;

                SetLocation(cell, cellLocation);
                this.Children.Add(cell);
            }

            var newCells = new List<MultiViewCell>(_columnCount);
            double newRowY = (_rowCount - 1) * newCellHeight;

            for (int i = 0; i < _columnCount; i++)
            {
                double newCellX = i * newCellWidth;
                var newCell = this.MakeNewCell(newCellX, newRowY, newCellWidth, newCellHeight);
                newCells.Add(newCell);
            }

            this.CellAdded?.Invoke(this, new CellAddedArgs(newCells));

            return newCells.Count > 0 ? newCells[0] : null;
        }

        internal void Init(int _rowCount, int _columnCount)
        {
            this._rowCount = _rowCount;
            this._columnCount = _columnCount;

            this.ClearInternal();

            var addedList = new List<MultiViewCell>(_rowCount * _columnCount);

            var width = DEFAULT_SIZE / _columnCount;
            var height = DEFAULT_SIZE / _rowCount;

            for (var y = 0; y < _rowCount; ++y)
            {
                for (var x = 0; x < _columnCount; ++x)
                {
                    var left = x * width;
                    var top = y * height;

                    var cell = this.MakeNewCell(left, top, width, height);

                    addedList.Add(cell);
                }
            }

            this.SelectionArea = Rect.Empty;
            this.IsSelectionEnabled = true;

            this.CellAdded?.Invoke(this, new CellAddedArgs(addedList));
        }

        internal bool IsCellSelected()
        {
            return !this._selectionArea.IsEmpty && this._selectionArea.Width > 0 && this._selectionArea.Height > 0;
        }

        internal bool IsSelectionAlignable()
        {
            if (this.IsCellSelected() == false)
            {
                return false;
            }

            List<MultiViewCell> cells = this.GetCells(this._selectionArea);
            var lefts = new HashSet<double>();
            var tops = new HashSet<double>();

            double? widthBasis = null;
            double? heightBasis = null;

            foreach (MultiViewCell item in cells)
            {
                Rect loc = GetLocation(item);

                lefts.Add(loc.Left);
                tops.Add(loc.Top);

                double curWidth = Math.Round(loc.Width, 1, MidpointRounding.AwayFromZero);
                double curHeight = Math.Round(loc.Height, 1, MidpointRounding.AwayFromZero);

                if (!widthBasis.HasValue)
                {
                    widthBasis = curWidth;
                }
                else if (widthBasis.Value != curWidth)
                {
                    return false;
                }

                if (!heightBasis.HasValue)
                {
                    heightBasis = curHeight;
                }
                else if (heightBasis.Value != curHeight)
                {
                    return false;
                }
            }

            int count = (int)Math.Ceiling(Math.Sqrt(cells.Count));

            return lefts.Count == count && tops.Count == count;
        }

        internal List<MultiViewCell> GetCellsWithinSelectionArea()
        {
            return this.GetCells(this._selectionArea);
        }

        internal List<MultiViewCell> GetCellsWithoutSelectionArea()
        {
            if (this._selectionArea.IsEmpty || this._selectionArea.Width <= 0 || this._selectionArea.Height <= 0)
            {
                return [.. this.Children.OfType<MultiViewCell>()];
            }

            var result = new List<MultiViewCell>(this.Children.Count);

            foreach (UIElement item in this.Children)
            {
                if (item is MultiViewCell cell && !RegionHelper.IsRectContains(this._selectionArea, GetLocation(cell), 0))
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        internal void RemoveCells(List<MultiViewCell> cells)
        {
            foreach (MultiViewCell cell in cells)
            {
                this.RemoveCell(cell);
            }
        }

        internal void SelectionAll()
        {
            this.SelectionArea = new Rect(0, 0, DEFAULT_SIZE, DEFAULT_SIZE);
            this.IsSelectionEnabled = true;
        }

        internal void SelectionClear()
        {
            this.SelectionArea = Rect.Empty;
            this.IsSelectionEnabled = true;
        }

        internal MultiViewCell? SelectionMerge(bool isBackground = false)
        {
            if (!this.IsCellSelected())
            {
                return null;
            }

            if (!isBackground)
            {
                this.RemoveSelectedCells();
            }

            var addedCell = this.MakeNewCell(this._selectionArea.Left, this._selectionArea.Top,
                                            this._selectionArea.Width, this._selectionArea.Height,
                                            isBackground);

            CellAdded?.Invoke(this, new CellAddedArgs([addedCell]));

            return addedCell;
        }

        internal List<MultiViewCell> SelectionSplit(int _rowCount, int _columnCount, bool isBackground = false)
        {
            var addedList = new List<MultiViewCell>(_rowCount * _columnCount);

            if (!this.IsCellSelected())
            {
                return addedList;
            }

            if (!isBackground)
                RemoveSelectedCells();

            double width = this._selectionArea.Width / _columnCount;
            double height = this._selectionArea.Height / _rowCount;
            double left = this._selectionArea.Left;
            double top = this._selectionArea.Top;

            for (int y = 0; y < _rowCount; ++y)
            {
                var topOffset = y * height + top;

                for (int x = 0; x < _columnCount; ++x)
                {
                    var leftOffset = x * width + left;

                    addedList.Add(this.MakeNewCell(leftOffset, topOffset, width, height, isBackground));
                }
            }

            CellAdded?.Invoke(this, new CellAddedArgs(addedList));

            return addedList;
        }

        internal void SetActualLocation(MultiViewCell cell, Rect rect)
        {
            var loc = new Rect
            {
                X = rect.Left * DEFAULT_SIZE / this.ActualWidth,
                Y = rect.Top * DEFAULT_SIZE / this.ActualHeight,
                Width = rect.Width * DEFAULT_SIZE / this.ActualWidth,
                Height = rect.Height * DEFAULT_SIZE / this.ActualHeight
            };

            SetLocation(cell, loc);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in this.InternalChildren)
            {
                var arrangeRect = new Rect();

                if (child is MultiViewCell cell)
                {
                    Rect loc = GetLocation(cell);

                    arrangeRect.X = finalSize.Width * loc.Left / DEFAULT_SIZE;
                    arrangeRect.Y = finalSize.Height * loc.Top / DEFAULT_SIZE;
                    arrangeRect.Size = child.DesiredSize;
                }
                else
                {
                    arrangeRect.X = 0;
                    arrangeRect.Y = 0;
                    arrangeRect.Size = child.DesiredSize;
                }

                child.Arrange(arrangeRect);
            }

            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in this.InternalChildren)
            {
                Size childDesiredSize;

                if (child is MultiViewCell cell)
                {
                    Rect loc = GetLocation(cell);

                    cell.Width = double.IsInfinity(availableSize.Width)
                                     ? loc.Width
                                     : availableSize.Width / DEFAULT_SIZE * loc.Width;
                    cell.Height = double.IsInfinity(availableSize.Height)
                                      ? loc.Height
                                      : availableSize.Height / DEFAULT_SIZE * loc.Height;
                    childDesiredSize.Width = cell.Width;
                    childDesiredSize.Height = cell.Height;
                }
                else
                {
                    childDesiredSize.Width = availableSize.Width;
                    childDesiredSize.Height = availableSize.Height;
                }

                child.Measure(childDesiredSize);
            }

            return base.MeasureOverride(availableSize);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (this._zoomedCell == null || _zoomedElement == null)
            {
                return;
            }

            _zoomedElement.SetValue(FrameworkElement.WidthProperty, sizeInfo.NewSize.Width);
            _zoomedElement.SetValue(FrameworkElement.HeightProperty, sizeInfo.NewSize.Height);
        }

        #endregion

        #region Private Methods

        public void SetZoomedCell(MultiViewCell? value)
        {
            if (_rowCount == 1 && _columnCount == 1)
                return;

            if (value == null)
            {
                MultiViewCell? zoomCell = _zoomedCell;
                this.ZoomOutCell();
                if (zoomCell != null)
                    SelectCell(zoomCell);
            }
            else
            {
                this.ZoomInCell(value);
                if (value != null)
                    SelectionClear();
            }
        }

        internal void ZoomInCell(MultiViewCell value, Action? onComplete = null)
        {
            if (_zoomedCell != null)
            {
                return;
            }

            if (value.GetElementCount() < 1)
            {
                return;
            }

            var innerGrid = value.InnerGrid;
            if (innerGrid == null || innerGrid.Children.Count < 1)
            {
                return;
            }

            var element = innerGrid.Children[0];

            _zoomedCell = value;
            _zoomedCell.IsZoomed = true;
            _zoomedElement = element;

            void BeforeAnimation()
            {
                var currentZoomedCell = _zoomedCell;
                var currentZoomedElement = _zoomedElement;

                if (currentZoomedCell == null || currentZoomedElement == null)
                {
                    return;
                }

                IsBusyForZoom = true;
                ZoomedCellChanging?.Invoke(this, new ZoomedCellChangeEventArgs(currentZoomedCell));

                currentZoomedCell.HideBorder();
                innerGrid.Children.Remove(currentZoomedElement);

                // Reparent after layout work has settled; moving the element synchronously can use stale dimensions.
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    if (currentZoomedElement == null)
                    {
                        return;
                    }

                    ZoomCanvas.Children.Add(currentZoomedElement);
                    ZoomCanvas.Visibility = Visibility.Visible;
                    ZoomCanvas.IsHitTestVisible = true;

                    Panel.SetZIndex(ZoomCanvas, 2);

                    double width = ZoomCanvas.ActualWidth;
                    double height = ZoomCanvas.ActualHeight;

                    if (double.IsNaN(width) || width <= 0)
                    {
                        width = ActualWidth;
                    }

                    if (double.IsNaN(height) || height <= 0)
                    {
                        height = ActualHeight;
                    }

                    currentZoomedElement.SetValue(FrameworkElement.WidthProperty, width);
                    currentZoomedElement.SetValue(FrameworkElement.HeightProperty, height);
                }));
            }

            void AfterAnimation()
            {
                var currentZoomedCell = _zoomedCell;
                if (currentZoomedCell != null)
                {
                    ZoomedCellChanged?.Invoke(this, new ZoomedCellChangeEventArgs(currentZoomedCell));
                }

                Zoommed(currentZoomedCell);
                IsBusyForZoom = false;
                onComplete?.Invoke();
            }

            CellZoomAnimation(ZoomCanvas, true, BeforeAnimation, AfterAnimation);
        }

        internal void ZoomOutCell(Action? onComplete = null)
        {
            if (IsHoldButtonClicked)
            {
                return;
            }

            var oldCell = _zoomedCell;
            var oldElement = _zoomedElement;

            if (oldCell == null)
            {
                return;
            }

            oldCell.IsZoomed = false;

            _zoomedCell = null;
            _zoomedElement = null;

            void BeforeAnimation()
            {
                IsBusyForZoom = true;
                ZoomedCellChanging?.Invoke(this, new ZoomedCellChangeEventArgs(null));
            }

            void AfterAnimation()
            {
                Panel.SetZIndex(ZoomCanvas, -1);
                ZoomCanvas.Visibility = Visibility.Collapsed;
                ZoomCanvas.IsHitTestVisible = false;
                ZoomCanvas.Children.Clear();

                if (oldElement != null)
                {
                    oldElement.SetValue(FrameworkElement.WidthProperty, double.NaN);
                    oldElement.SetValue(FrameworkElement.HeightProperty, double.NaN);

                    oldCell.InnerGrid?.Children.Add(oldElement);
                }

                oldCell.ShowBorder();

                ZoomedCellChanged?.Invoke(this, new ZoomedCellChangeEventArgs(null));
                IsBusyForZoom = false;

                Zoommed(oldCell);
                onComplete?.Invoke();
            }

            CellZoomAnimation(ZoomCanvas, false, BeforeAnimation, AfterAnimation);
        }

        private static void Zoommed(object? obj)
        {
            if (obj is MultiViewCell cell)
            {
                cell.Dispatcher.Invoke(() => cell.IsHitTestVisible = true, DispatcherPriority.Render);
            }
        }


        private void SetControlledCell(MultiViewCell? value)
        {
            if (value == null)
            {
                this._controlledCell = null;
            }
            else
            {
                if (value.GetElementCount() < 1)
                {
                    this._controlledCell = null;
                }
                else
                {
                    this._controlledCell = value;
                    this.SelectionArea = GetLocation(value);
                }
            }

            this.ControlledCellChanged?.Invoke(this, new ControlledCellChangedEventArgs(this._controlledCell));
        }


        private void Cell_MouseEnter(object sender, MouseEventArgs e)
        {

            var cell = sender as MultiViewCell;
            Rect loc = GetLocation(cell);

            this.IsSelectionEnabled = this.CheckSelectionEnabled(loc);
        }

        private void Cell_TouchEnter(object? sender, TouchEventArgs e)
        {
            if (sender is MultiViewCell cell)
            {
                Rect loc = GetLocation(cell);

                this.IsSelectionEnabled = this.CheckSelectionEnabled(loc);
            }
        }

        private bool IsDoubleTap(TouchEventArgs e)
        {
            Point currentTapPosition = e.GetTouchPoint(this).Position;
            double offset = Math.Sqrt((currentTapPosition.X - _lastTapLocation.X) * (currentTapPosition.X - _lastTapLocation.X) + (currentTapPosition.Y - _lastTapLocation.Y) * (currentTapPosition.Y - _lastTapLocation.Y));
            bool tapsAreCloseInDistance = offset < 40;
            _lastTapLocation = currentTapPosition;

            TimeSpan elapsed = _doubleTapStopwatch.Elapsed;
            _doubleTapStopwatch.Restart();
            bool tapsAreCloseInTime = (elapsed != TimeSpan.Zero && elapsed < TimeSpan.FromSeconds(0.7));

            return tapsAreCloseInDistance && tapsAreCloseInTime;
        }

        private void Cell_TouchDown(object? sender, TouchEventArgs e)
        {
            if (IsDoubleTap(e))
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
                {
                    this.OnDoubleClick(sender);

                    e.Handled = true;
                }
            }
        }

        private void Cell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.MouseDownCell = sender as MultiViewCell;

            e.Handled = false;


            bool isDoubleClick = e.ClickCount >= 2;
            bool isLeftClickWithoutShift = e.LeftButton == MouseButtonState.Pressed &&
                                            (Keyboard.Modifiers & ModifierKeys.Shift) == 0;

            if (isDoubleClick && isLeftClickWithoutShift)
            {
                this.OnDoubleClick(sender);
                return;
            }

            this.OnCellSelection(sender, e);
        }

        private void Cell_PreviewTouchDown(object? sender, TouchEventArgs e)
        {
            if (sender is MultiViewCell cell)
            {
                this.MouseDownCell = cell;
            }
        }

        void Cell_SelectedCell(object? sender, EventArgs e)
        {
            this.SelectCellForced(sender);
        }

        private static void CellZoomAnimation(Canvas canvas, bool isZoomIn, Action? before = null, Action? completed = null)
        {
            before?.Invoke();

            if (canvas.RenderTransform is not ScaleTransform)
            {
                canvas.RenderTransformOrigin = new Point(0.5, 0.5);
                canvas.RenderTransform = new ScaleTransform();
            }

            var storyboard = new Storyboard();
            var duration = new Duration(TimeSpan.FromMilliseconds(100));

            double fromScale = isZoomIn ? 0.8 : 1.0;
            double toScale = isZoomIn ? 1.0 : 0.8;

            double fromOpacity = isZoomIn ? 0.0 : 1.0;
            double toOpacity = isZoomIn ? 1.0 : 0.0;

            var easing = new QuadraticEase
            {
                EasingMode = isZoomIn ? EasingMode.EaseOut : EasingMode.EaseIn
            };

            var scaleXAnim = new DoubleAnimation(fromScale, toScale, duration)
            {
                EasingFunction = easing
            };
            Storyboard.SetTarget(scaleXAnim, canvas);
            Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            storyboard.Children.Add(scaleXAnim);

            var scaleYAnim = new DoubleAnimation(fromScale, toScale, duration)
            {
                EasingFunction = easing
            };
            Storyboard.SetTarget(scaleYAnim, canvas);
            Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboard.Children.Add(scaleYAnim);

            var opacityAnim = new DoubleAnimation(fromOpacity, toOpacity, duration);
            Storyboard.SetTarget(opacityAnim, canvas);
            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(opacityAnim);

            if (completed != null)
                storyboard.Completed += (s, e) => completed();

            storyboard.Begin();
        }

        private static void StartDoubleAnimation(Animatable animatable, DependencyProperty dp, Duration duration, double from, double to, Action? completed = null)
        {
            var ani = new DoubleAnimation(from, to, duration);

            if (completed != null)
            {
                ani.Completed += (sender, e) => completed();
            }

            animatable.BeginAnimation(dp, ani);
        }


        private bool CheckSelectionEnabled(Rect rect)
        {
            if (RegionHelper.IsRectContains(this._selectionArea, rect, 0))
                return true;

            Rect tmpSelectionArea = Rect.Union(this._selectionArea, rect);

            var containedCells = new HashSet<MultiViewCell>();
            var intersectedCells = new HashSet<MultiViewCell>();

            foreach (MultiViewCell cell in this.Children)
            {
                Rect cellRect = GetLocation(cell);

                if (RegionHelper.IsRectContains(tmpSelectionArea, cellRect, 0))
                    containedCells.Add(cell);

                if (RegionHelper.IsRectIntersectWith(tmpSelectionArea, cellRect, 0))
                    intersectedCells.Add(cell);
            }

            // A valid rectangular selection must not partially intersect any cell.
            return containedCells.SetEquals(intersectedCells);
        }

        private void ClearInternal()
        {
            this._zoomedCell = null;

            var removedList = this.Children.OfType<MultiViewCell>().ToList();

            foreach (var cell in removedList)
                cell.Clear();

            this.Children.Clear();

            foreach (var cell in removedList)
                this.RemoveCell(cell);

            this.CellRemoved?.Invoke(this, new CellRemovedArgs(removedList));
        }


        private Rect GetActualZoomedLocationByRect(Rect cellRect)
        {
            Rect result = new()
            {
                Width = this.ActualWidth,
                Height = this.ActualHeight
            };

            var center = new Point
            {
                X = cellRect.Left + (cellRect.Width / 2),
                Y = cellRect.Top + (cellRect.Height / 2)
            };

            result.X = center.X - (result.Width / 2);
            result.Y = center.Y - (result.Height / 2);

            result.X = Math.Max(0, Math.Min(result.X, this.ActualWidth - result.Width));
            result.Y = Math.Max(0, Math.Min(result.Y, this.ActualHeight - result.Height));

            return result;
        }

        private List<MultiViewCell> GetCells(Rect area)
        {
            if (area.IsEmpty || area.Width <= 0 || area.Height <= 0)
            {
                return [];
            }

            return [..this.Children
                .OfType<MultiViewCell>()
                .Where(item => RegionHelper.IsRectContains(area, GetLocation(item), 0))];
        }

        private MultiViewCell MakeNewCell(double left, double top, double width, double height, bool isBack = false)
        {
            MultiViewCell cell = new()
            {
                Background = Brushes.Yellow
            };

            cell.PreviewMouseDown += this.Cell_PreviewMouseDown;

            cell.MouseEnter += this.Cell_MouseEnter;
            cell.SelectedCell += Cell_SelectedCell;

            cell.PreviewTouchDown += this.Cell_PreviewTouchDown;
            cell.TouchDown += this.Cell_TouchDown;
            cell.TouchEnter += this.Cell_TouchEnter;

            if (isBack)
            {
                this.Children.Insert(0, cell);
            }
            else
            {
                this.Children.Add(cell);
            }

            SetLocation(cell, new Rect(left, top, width, height));


            return cell;
        }

        private DateTime _lastClickTime = DateTime.Now;
        private void OnDoubleClick(object? sender)
        {
            if (sender is not MultiViewCell cell)
                return;

            if ((DateTime.Now - this._lastClickTime).TotalSeconds <= 1.0)
            {
                return;
            }

            this.ZoomedCell = this._zoomedCell == cell ? null : cell;
            this._lastClickTime = DateTime.Now;
        }

        internal bool OnMapControlCellZoomOut(MultiViewCell cell, bool isIconClick = false)
        {
            if (isIconClick && this._zoomedCell != cell)
            {
                return false;
            }

            if ((DateTime.Now - this._lastClickTime).TotalSeconds <= 1.0)
            {
                return true;
            }

            this.ZoomedCell = this._zoomedCell == cell ? null : cell;
            this._lastClickTime = DateTime.Now;

            return this._zoomedCell == cell;
        }

        private void SelectCellForced(object? sender)
        {
            if (sender is MultiViewCell cell)
            {
                SelectionArea = GetLocation(cell);
            }
        }


        private void OnCellSelection(object sender, MouseButtonEventArgs e)
        {
            if (sender is not MultiViewCell cell)
                return;

            var loc = GetLocation(cell);

            if (e.RightButton == MouseButtonState.Pressed && RegionHelper.IsRectEquals(this._selectionArea, loc, 0))
            {
                SelectionClear();
                return;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (this._zoomedCell != null && !GetCells(loc).Contains(this._zoomedCell))
                return;

            if (this.IsBusyForZoom)
                return;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (CheckSelectionEnabled(loc) && !RegionHelper.IsRectContains(this._selectionArea, loc, 0))
                {
                    SelectionArea = Rect.Union(this._selectionArea, loc);
                }
                return;
            }

            SelectionArea = loc;
        }

        private void RemoveCell(MultiViewCell cell)
        {
            cell.PreviewMouseDown -= Cell_PreviewMouseDown;

            cell.MouseEnter -= Cell_MouseEnter;
            cell.SelectedCell -= Cell_SelectedCell;

            cell.PreviewTouchDown -= Cell_PreviewTouchDown;
            cell.TouchDown -= Cell_TouchDown;
            cell.TouchEnter -= Cell_TouchEnter;

            foreach (var control in cell.GetAllElements().OfType<OpnxControl>())
            {
                control.Dispose();
            }

            cell.Dispose();

            Children.Remove(cell);
        }

        private void RemoveSelectedCells()
        {
            if (!IsCellSelected())
                return;

            var removedList = GetCells(this._selectionArea);
            if (removedList.Count == 0)
                return;

            removedList.ForEach(RemoveCell);
            CellRemoved?.Invoke(this, new CellRemovedArgs(removedList));
        }

        internal MultiViewCell? GetNextEmptyCell()
        {
            return GetAllCells()
            .FirstOrDefault(cell => cell.GetAllElements().Count <= 0);
        }

        internal MultiViewCell? GetNextTopLeftEmptyCell()
        {
            return GetAllCells()
                .Where(cell => cell.GetAllElements().Count == 0)
                .OrderBy(cell => GetLocation(cell).Y)
                .ThenBy(cell => GetLocation(cell).X)
                .FirstOrDefault();
        }

        internal MultiViewCell? GetCell(int row, int col)
        {
            var minRect = this.Children.Count > 0 ? GetLocation(this.Children[0] as MultiViewCell) : new Rect();

            foreach (MultiViewCell item in this.Children)
            {
                var rect = GetLocation(item);

                if (rect.Width < minRect.Width)
                {
                    minRect.Width = rect.Width;
                }

                if (rect.Height < minRect.Height)
                {
                    minRect.Height = rect.Height;
                }
            }

            minRect.X = minRect.Width * col;
            minRect.Y = minRect.Height * row;

            MultiViewCell? cell = null;

            if (minRect.IsEmpty || minRect.Width <= 0 || minRect.Height <= 0)
            {
                return cell;
            }

            foreach (MultiViewCell item in this.Children)
            {
                Rect rect = GetLocation(item);
                if (RegionHelper.IsRectContains(minRect, rect, 0, true))
                {
                    cell = item;
                    break;
                }
            }

            return cell;
        }

        internal MultiViewCell? GetCell(UIElement uiElement)
        {
            return GetAllCells()
                 .FirstOrDefault(cell => cell.GetAllElements().Contains(uiElement));
        }

        #endregion
    }
}










