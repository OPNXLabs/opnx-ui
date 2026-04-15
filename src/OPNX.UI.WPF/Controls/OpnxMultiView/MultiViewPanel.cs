using OPNX.UI.WPF.Controls.Primitives;
using OPNX.UI.WPF.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls.OpnxMultiView
{
    /// <summary>
    /// Grid 컨트롤을 사용하지 않았다.
    /// CoordinatesPercentage를 최대값으로 하는 비율좌표계를 사용한다.
    /// 각 Cell들의 좌표 및 크기는 Canvas.LeftProperty / Canvas.TopProperty / Canvas.RightProperty / Canvas.BottomProperty를 이용한다.
    /// 외부에서 MultiViewPanel.Children 속성에 직접 접근하지 않는 것을 권장한다.
    /// </summary>
    internal class MultiViewPanel : Canvas
    {
        #region Constants and Fields

        /// <summary>
        /// MultiView의 실제 동작에 중추적인 역할을 하는 비율 좌표 단위.
        /// </summary>
        private const double DEFAULT_SIZE = 100000;

        /// <summary>
        /// DoubleClick검출을 위해 대기할 시간.
        /// </summary>
        private MultiViewCell? _controlledCell;

        private Rect _selectionArea = Rect.Empty;

        //private Rect zoomedCellOriginalRect;
        private MultiViewCell? _zoomedCell;
        private UIElement? _zoomedElement = null;

        public bool IsHoldButtonClicked = false;

        private int _rowCount = 0;
        private int _columnCount = 0;

        private DateTime _zoomCanvasLastClickTime = DateTime.MinValue;
        // 더블클릭으로 판단할 시간 (ms)
        private const int DoubleClickThreshold = 300;

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
            //var handler = eSelectionChanged;
            //if (handler != null)
            //    handler(this, new SelectionChangedArgs(this.GetActualSelectionArea(), this.GetCells(this._selectionArea)));
        }

        internal event EventHandler<ZoomedCellChangeEventArgs>? ZoomedCellChanging;

        //internal event EventHandler<ZoomedCellChangeEventArgs> eZoomedCellBeginChange;

        //internal event EventHandler<CellDropCompletedArgs> CellDropCompleted;

        //public event EventHandler<OpnxMultiViewZoomedForStaticEventArgs> eSlideZoomOut;

        //public event EventHandler<OpnxMultiViewZoomedForStaticEventArgs> eFavoriteSlideZoomOut;

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

        /// <summary>
        /// 풀 스크린 줌 처리 중.
        /// </summary>
        public bool IsBusyForZoom { get; private set; }

        /// <summary>
        /// Gets or sets _controlledCell.
        /// 제어모드중인 Cell.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether IsSelectionEnabled.
        /// 선택이 가능한지 (selection 영역의 변경이 가능한지) 여부.
        /// </summary>
        internal bool IsSelectionEnabled { get; private set; }

        /// <summary>
        /// Gets or sets _zoomedCell.
        /// 현재 Zoom된 Cell.
        /// </summary>
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

        /// <summary>
        /// Gets 셀의 실제 넓이를 구할 때 사용하는 비율 값.
        /// </summary>
        private double CellWidthRatio => this.ActualWidth / DEFAULT_SIZE;

        /// <summary>
        /// Gets 셀의 실제 높이를 구할 때 사용하는 비율 값.
        /// </summary>
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

        /// <summary>
        /// MouseDown되었을때 Cell.
        /// </summary>
        public MultiViewCell? MouseDownCell { get; private set; }

        /// <summary>
        /// 부모 OpnxMultiView.
        /// </summary>
        public OpnxMultiView? MultiView { get; set; }

        #endregion // Public Properties

        #region Public Methods

        /// <summary>
        /// 특정 셀 선택하기.
        /// </summary>
        /// <param name="cell">선택해야할 셀.</param>
        public void SelectCell(MultiViewCell cell)
        {
            var rect = GetLocation(cell);
            this.SelectionArea = rect;
        }

        /// <summary>
        /// Layout을 포함하고 있는 Cell 목록을 가져온다 (Selection영역과는 관계없다).
        /// </summary>
        /// <returns>
        /// Cell 목록.
        /// </returns>
        //public List<Cell> GetCellsIncludeLayout()
        //{
        //    var result = new List<Cell>();
        //    foreach (object child in this.Children)
        //    {
        //        var cell = child as Cell;
        //        if (cell == null)
        //        {
        //            continue;
        //        }

        //        if (cell.IsIncludeLayout)
        //        {
        //            result.Add(cell);
        //        }
        //    }

        //    return result;
        //}

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

        /// <summary>
        /// Cell의 비율좌표계(MultiView에서만 사용하는 내부 좌표계)상의 위치를 가져온다.
        /// </summary>
        /// <param name="cell">
        /// The Cell.
        /// </param>
        /// <returns>
        /// The Rect.
        /// </returns>
        internal static Rect GetLocation(MultiViewCell? cell)
        {
            if (cell == null)
            {
                return new Rect(0, 0, 0, 0);  // 기본값을 간단히 초기화
            }

            var result = new Rect
            {
                X = Canvas.GetLeft(cell),
                Y = Canvas.GetTop(cell),
                Width = Canvas.GetRight(cell) - Canvas.GetLeft(cell),  // X와 Right를 사용해 계산
                Height = Canvas.GetBottom(cell) - Canvas.GetTop(cell)  // Y와 Bottom을 사용해 계산
            };

            return result;
        }

        /// <summary>
        /// Cell의 비율좌표계(MultiView에서만 사용하는 내부 좌표계)상의 위치를 설정한다.
        /// </summary>
        /// <param name="cell">
        /// The Cell.
        /// </param>
        /// <param name="rect">
        /// The Rect.
        /// </param>
        internal static void SetLocation(MultiViewCell cell, Rect rect)
        {
            Canvas.SetLeft(cell, rect.Left);
            Canvas.SetTop(cell, rect.Top);
            Canvas.SetRight(cell, rect.Right);
            Canvas.SetBottom(cell, rect.Bottom);
        }

        /// <summary>
        /// Cell의 Rendering된 위치와 크기를 가져온다.
        /// </summary>
        /// <param name="cell">
        /// The Cell.
        /// </param>
        /// <returns>
        /// The Rect.
        /// </returns>
        internal Rect GetActualRect(MultiViewCell cell)
        {
            if (cell == null)
            {
                return Rect.Empty;  // cell이 null이면 바로 Empty Rect 반환
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

        /// <summary>
        /// 새로운 Cell을 추가한다. Children.Add()를 사용하지 말것.
        /// </summary>
        /// <param name="cell">
        /// The Cell.
        /// </param>
        /// <param name="rect">
        /// The Rect.
        /// </param>
        /// <param name="isBack"> true이면 0번째 자식으로 등록, false이면 마지막으로 등록 </param>
        internal void AddCell(MultiViewCell cell, Rect rect, bool isBack = false)
        {
            SetLocation(cell, rect);

            cell.Background = Brushes.Transparent;

            cell.PreviewMouseDown += this.Cell_PreviewMouseDown;

            //cell.MouseLeftButtonDown += this.Cell_MouseLeftButtonDown;
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


            //// TODO : Multigrid DnD
            //var targetDropAdvisor = new DragDrop.CellDropTargetAdvisor();
            //targetDropAdvisor.eDropCompleted += this.TargetDropAdvisor_eDropCompleted;
            //Commons.Utils.DragDropManager.SetDropTargetAdvisor(cell, targetDropAdvisor);

            //if (this.eCellAdded != null)
            //{
            //    var addedList = new List<Cell>(1)
            //                        {
            //                            cell
            //                        };

            //    var localCellAdded = this.eCellAdded;
            //    if (localCellAdded != null)
            //    {
            //        localCellAdded(this, new CellAddedArgs(addedList));
            //    }
            //}
        }

        /// <summary>
        /// 멀티 그리드의 모든 자식을 제거.
        /// </summary>
        internal void Clear()
        {
            this.ClearInternal();

            this._selectionArea = Rect.Empty;
            this.IsSelectionEnabled = false;
        }


        internal MultiViewCell? GetCellByPercent(double x, double y)
        {
            if (x < 0.0 || y < 0.0) return null;

            double posX = this.ActualWidth * x / 100.0;
            double posY = this.ActualHeight * y / 100.0;

            for (int i = 0; i < this.Children.Count; i++)
            {
                if (this.Children[i] is MultiViewCell cell) // 안전한 형 변환
                {
                    if (GetActualRect(cell).Contains(posX, posY))
                    {
                        return cell; // 좌표가 포함된 첫 번째 Cell 반환
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 현재 SelectionArea의 영역을 반환한다.
        /// </summary>
        /// <returns>
        /// Selection영역 (WPF좌표계).
        /// </returns>
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

        /// <summary>
        /// Cell이 ZoomIn되었을 경우의 WPF좌표계 Rect영역.
        /// </summary>
        /// <param name="cell">
        /// The Cell.
        /// </param>
        /// <returns>
        /// The Rect.
        /// </returns>
        internal Rect GetActualMaxRect(MultiViewCell cell)
        {
            if (cell == null)
            {
                return Rect.Empty;  // cell이 null인 경우 바로 Rect.Empty 반환
            }

            //if (!CommonsConfig.Instance.UseCellAutoFullScreen)
            //{
            //    return this.GetCellDoubleClickRectByConfig();
            //}

            //if (cell == null)
            //{
            //    System.Diagnostics.Debug.Assert(cell != null, "셀이 null이 될 수 없을 것 같습니다. 이 부분에 어설트가 걸리면 소스를 살펴볼 것.");
            //    return this.GetMultiViewSize();
            //}

            var cellRect = this.GetActualRect(cell);
            return this.GetActualZoomedLocationByRect(cellRect);
        }

        /// <summary>
        /// 현재 MultiView의 모든 Cell 목록을 가져온다(Selection 영역과는 관계없다).
        /// </summary>
        /// <returns>
        /// Cell 목록.
        /// </returns>
        internal List<MultiViewCell> GetAllCells()
        {
            //var result = new List<Cell>(this.Children.Count);
            //result.AddRange(this.Children.OfType<Cell>());

            //return result;

            return [.. Children.OfType<MultiViewCell>()];
        }

        ///// <summary>
        ///// 현재 MultiGrid의 모든 Layout의 목록을 가져온다(Selection영역과는 관계없다).
        ///// 실제로 Cell의 Layout은 하나만 존재함. 나중을 위해서 List를 반환하는 구조로 감.
        ///// </summary>
        ///// <returns>
        ///// Layout 목록.
        ///// </returns>
        //internal List<LayoutControl> GetAllLayout()
        //{
        //    var result = new List<LayoutControl>();
        //    foreach (object child in this.Children)
        //    {
        //        var cell = child as Cell;
        //        if (cell == null)
        //        {
        //            continue;
        //        }

        //        result.AddRange(cell.GetAllLayout());
        //    }

        //    return result;
        //}

        //internal List<MapControl> GetAllMapControl()
        //{
        //    var result = new List<MapControl>();
        //    foreach (object child in this.Children)
        //    {
        //        var cell = child as Cell;
        //        if (cell == null)
        //        {
        //            continue;
        //        }

        //        result.AddRange(cell.GetAllMapControl());
        //    }

        //    return result;
        //}

        /// <summary>
        /// 현재 MultiView의 선택 영역 Cell 목록을 가져온다.
        /// </summary>
        /// <returns>
        /// Cell 목록.
        /// </returns>
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
                var loc = GetLocation(cells[i]);  // 캐싱하여 중복 호출 방지
                lefts.Add(loc.Left);
                tops.Add(loc.Top);
            }

            return (cellCount == lefts.Count * tops.Count) ? (tops.Count, lefts.Count) : (-1, -1);

        }
        /// <summary>
        /// 현재 Selection된 Cell들의 열 갯수.
        /// </summary>
        /// <returns>
        /// The Int.
        /// </returns>
        internal int GetSelectionColumnCount() => GetSelectionGridCount()._columnCount;


        /// <summary>
        /// 현재 Selection된 Cell들의 행 갯수.
        /// </summary>
        /// <returns>
        /// The Int.
        /// </returns>
        internal int GetSelectionRowCount() => GetSelectionGridCount()._rowCount;


        ///// <summary>
        ///// 현재 Selection영역의 Cell이 Slide Index.
        ///// 여러 Cell이 선택되어 있을 경우 -1을 반환한다.
        ///// </summary>
        ///// <returns>
        ///// The Int.
        ///// </returns>
        //internal int GetSelectionSlideIndex()
        //{
        //    if (this.IsCellSelected() == false)
        //    {
        //        return -1;
        //    }

        //    List<Cell> cells = this.GetCells(this._selectionArea);
        //    if (cells.Count != 1)
        //    {
        //        return -1;
        //    }

        //    return cells[0].SlideIndex;
        //}

        //internal void SetSelectionSlideIndex(int index)
        //{
        //    if (this.IsCellSelected() == false)
        //    {
        //        return;
        //    }

        //    var cells = this.GetCells(this._selectionArea);
        //    if (cells.Count != 1)
        //    {
        //        return;
        //    }

        //    cells[0].SlideIndex = index;
        //}

        /// <summary>
        /// 현재 Selection영역의 Cell이 Slide Interval.
        /// 여러 Cell이 선택되어 있을 경우 -1을 반환한다.
        /// </summary>
        /// <returns>
        /// The Int.
        /// </returns>
        //internal int GetSelectionSlideInterval()
        //{
        //    if (this.IsCellSelected() == false)
        //    {
        //        return -1;
        //    }

        //    List<Cell> cells = this.GetCells(this._selectionArea);
        //    if (cells.Count != 1)
        //    {
        //        return -1;
        //    }

        //    return cells[0].SlideIntervalSeconds;
        //}

        /// <summary>
        /// 현재 Selection영역에 Slide모드인 Cell이 포함되어 있는지 여부.
        /// </summary>
        /// <returns>
        /// The Bool.
        /// </returns>
        //internal bool HasSelectionSlideMode()
        //{
        //    if (this.IsCellSelected() == false)
        //    {
        //        return false;
        //    }

        //    foreach (Cell cell in this.GetCells(this._selectionArea))
        //    {
        //        if (cell.IsSlideMode())
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

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

        /// <summary>
        /// MultiView의 모든 Cell 및 Cell 내부 Element를 삭제하고 Cell을 재배치한다.
        /// </summary>
        /// <param name="_rowCount">
        /// 초기화할 행.
        /// </param>
        /// <param name="_columnCount">
        /// 초기화할 열.
        /// </param>
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
                    //if (this.eCellAdded != null)
                    //{
                    //    addedList.Add(cell);
                    //}
                }
            }

            this._selectionArea = Rect.Empty;
            this.IsSelectionEnabled = true;

            this.CellAdded?.Invoke(this, new CellAddedArgs(addedList));
        }

        //internal void TargetDropAdvisor_eDropCompleted(object sender, DropTargetAdvisorDropCompletedEventArgs e)
        //{
        //    //if (this.eCellDropCompleted != null)
        //    //{
        //    //    this.eCellDropCompleted(this, new CellDropCompletedArgs(this, e.Source, e.TargetCell));
        //    //}
        //}

        /// <summary>
        /// 현재 Selection상태인지(하나의 Cell이라도 Selection되어있는지) 여부.
        /// </summary>
        /// <returns>
        /// The Bool.
        /// </returns>
        internal bool IsCellSelected()
        {
            return !this._selectionArea.IsEmpty && this._selectionArea.Width > 0 && this._selectionArea.Height > 0;
        }

        /// <summary>
        /// 현재 Selection된 Cell들이 N by N 의 배치인지 여부(2x2, 3x3, 4x4,...).
        /// </summary>
        /// <returns>
        /// The Bool.
        /// </returns>
        internal bool IsSelectionAlignable()
        {
            if (this.IsCellSelected() == false)
            {
                return false;
            }

            List<MultiViewCell> cells = this.GetCells(this._selectionArea);
            var lefts = new HashSet<double>();  // 중복을 자동으로 처리
            var tops = new HashSet<double>();   // 중복을 자동으로 처리
            //var lefts = new List<double>();
            //var tops = new List<double>();

            double? widthBasis = null;
            double? heightBasis = null;

            // 1.모든 rect의 left/top값들의 list를 구한다 (중복제외)
            foreach (MultiViewCell item in cells)
            {
                Rect loc = GetLocation(item);

                lefts.Add(loc.Left);  // HashSet은 자동으로 중복을 처리
                tops.Add(loc.Top);    // HashSet은 자동으로 중복을 처리

                double curWidth = Math.Round(loc.Width, 1, MidpointRounding.AwayFromZero);
                double curHeight = Math.Round(loc.Height, 1, MidpointRounding.AwayFromZero);

                if (!widthBasis.HasValue)
                {
                    widthBasis = curWidth;
                }
                else if (widthBasis.Value != curWidth)
                {
                    return false;  // 넓이가 일치하지 않으면 정렬 불가
                }

                if (!heightBasis.HasValue)
                {
                    heightBasis = curHeight;
                }
                else if (heightBasis.Value != curHeight)
                {
                    return false;  // 높이가 일치하지 않으면 정렬 불가
                }
            }

            int count = (int)Math.Ceiling(Math.Sqrt(cells.Count));

            // lefts와 tops의 개수가 count와 같은지 확인
            return lefts.Count == count && tops.Count == count;
        }

        /// <summary>
        /// 현재 Selection영역의 Cell이 Slide모드인지 여부.
        /// 여러 Cell이 선택되어 있을 경우 False를 반환한다.
        /// </summary>
        /// <returns>
        /// The Bool.
        /// </returns>
        //internal bool IsSelectionSlideMode()
        //{
        //    if (this.IsCellSelected() == false)
        //    {
        //        return false;
        //    }

        //    List<Cell> cells = this.GetCells(this._selectionArea);
        //    if (cells.Count != 1)
        //    {
        //        return false;
        //    }

        //    return cells[0].IsSlideMode();
        //}

        /// <summary>
        /// 현재 Selection영역의 모든 Cell들을 반환한다. 즉, 선택된 모든 Cell들을 반환한다.
        /// </summary>
        internal List<MultiViewCell> GetCellsWithinSelectionArea()
        {
            return this.GetCells(this._selectionArea);
        }

        /// <summary>
        /// 현재 Selection영역 외부의 모든 Cell들을 반환한다. 즉, 선택되지 않은 모든 Cell들을 반환한다.
        /// </summary>
        internal List<MultiViewCell> GetCellsWithoutSelectionArea()
        {
            //// SelectionArea가 비어있거나 크기가 0인 경우 전체 셀을 그대로 반환
            //if (this._selectionArea.IsEmpty || this._selectionArea.Width <= 0 || this._selectionArea.Height <= 0)
            //{
            //    return new List<Cell>(this.Children.OfType<Cell>());
            //}

            //// 영역에 포함되지 않는 셀을 리스트에 추가
            //var result = new List<Cell>();

            //foreach (UIElement item in this.Children)
            //{
            //    if (item is Cell cell) // UIElement를 Cell로 캐스팅
            //    {
            //        Rect rect = GetLocation(cell);
            //        if (!RegionHelper.IsRectContains(this._selectionArea, rect, 0))  // 선택 영역에 포함되지 않으면 추가
            //        {
            //            result.Add(cell);
            //        }
            //    }
            //}
            if (this._selectionArea.IsEmpty || this._selectionArea.Width <= 0 || this._selectionArea.Height <= 0)
            {
                return [.. this.Children.OfType<MultiViewCell>()];
            }

            var result = new List<MultiViewCell>(this.Children.Count); // 초기 용량 설정 (최대 100개)

            foreach (UIElement item in this.Children)
            {
                if (item is MultiViewCell cell && !RegionHelper.IsRectContains(this._selectionArea, GetLocation(cell), 0))
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        /// <summary>
        /// 현재 Selection영역의 Cell이 Slide 동작중인지 여부.
        /// 여러 Cell이 선택되어 있을 경우 False를 반환한다.
        /// </summary>
        /// <returns>
        /// The Bool.
        /// </returns>
        //internal bool IsSelectionSlidePlaying()
        //{
        //    if (this.IsCellSelected() == false)
        //    {
        //        return false;
        //    }

        //    List<Cell> cells = this.GetCells(this._selectionArea);
        //    if (cells.Count != 1)
        //    {
        //        return false;
        //    }

        //    return cells[0].IsSlidePlaying();
        //}

        internal void RemoveCells(List<MultiViewCell> cells)
        {
            foreach (MultiViewCell cell in cells)
            {
                this.RemoveCell(cell);
            }
        }

        internal void SelectionAll()
        {
            this._selectionArea = new Rect(0, 0, DEFAULT_SIZE, DEFAULT_SIZE);
            this.IsSelectionEnabled = true;
        }

        internal void SelectionClear()
        {
            this._selectionArea = Rect.Empty;
            this.IsSelectionEnabled = true;
        }

        /// <summary>
        /// 현재 선택된 영역내의 모든 Cell들을 하나의 Cell로 병합한다.
        /// </summary>
        /// <param name="isBackground"> 셀 생성을 기존 셀 뒤에서 진행하는 옵션. true이면 이전 셀을 지우지 않고 새로 생성되는 셀은 기존 셀 뒤에 배치된다.</param>
        /// <returns>
        /// 새로 생성된 Cell.
        /// </returns>
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

        /// <summary>
        /// 현재 선택된 영역내의 모든 Cell들을 제거하고 새로운 배열로 재배치한다.
        /// </summary>
        /// <param name="_rowCount">
        /// 초기화할 행.
        /// </param>
        /// <param name="_columnCount">
        /// 초기화할 열.
        /// </param>
        /// <param name="isBackground">셀 생성을 기존 셀 뒤에서 진행하는 옵션. true이면 이전 셀을 지우지 않고 새로 생성되는 셀은 기존 셀 뒤에 배치된다. </param>
        /// <returns>
        /// MultiView에 재배치된 Cell 목록.
        /// </returns>
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

                    //Cell cell = this.MakeNewCell((x * width) + left, (y * height) + top, width, height, isBackground);

                    //addedList.Add(cell);
                }
            }

            CellAdded?.Invoke(this, new CellAddedArgs(addedList));

            //var cells = this.GetAllCells();
            //foreach (var cell in cells)
            //{
            //    this.MultiGridControl.MustUpdateControlGuids.Add(cell.SyncGuid.ToString());
            //}

            return addedList;
        }

        /// <summary>
        /// Cell의 WPF좌표계상의(Rendering된) 위치를 설정한다.
        /// </summary>
        /// <param name="cell">
        /// The Cell.
        /// </param>
        /// <param name="rect">
        /// The Rect.
        /// </param>
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

        /// <summary>
        /// 더블클릭시 config에 있는 값을 사용하여 스테이지 기준의 셀의 크기와 위치를 반환한다.
        /// </summary>
        /// <param name="actualWith">
        /// The actual With 에는 멀티그리드의 실제 넓이가 들어간다.
        /// </param>
        /// <param name="actualHight">
        /// The actual Hight 에는 멀티 그리드의 실제 높이가 들어간다.
        /// </param>
        /// <param name="cellDoubleClickRatio">
        /// The cell Double Click Ratio.
        /// </param>
        /// <returns>
        /// 변환될 셀의 크기와 위치.
        /// </returns>
        private static Rect GetCellDoubleClickRect(double actualWith, double actualHight, Rect cellDoubleClickRatio)
        {
            System.Diagnostics.Debug.Assert(cellDoubleClickRatio.Left <= 100, "CellManualFullScreenRect 의 위치비율은 100보다 클 수 없습니다.");
            System.Diagnostics.Debug.Assert(cellDoubleClickRatio.Left >= 0, "CellManualFullScreenRect 의 위치비율은 0보다 작을 수 없습니다.");

            System.Diagnostics.Debug.Assert(cellDoubleClickRatio.Top <= 100, "CellManualFullScreenRect 의 위치비율은 100보다 클 수 없습니다.");
            System.Diagnostics.Debug.Assert(cellDoubleClickRatio.Top >= 0, "CellManualFullScreenRect 의 위치비율은 0보다 작을 수 없습니다.");

            System.Diagnostics.Debug.Assert(cellDoubleClickRatio.Width <= 100, "CellManualFullScreenRect 의 넓이비율은 100보다 클 수 없습니다.");
            System.Diagnostics.Debug.Assert(cellDoubleClickRatio.Width >= 0, "CellManualFullScreenRect 의 넓이비율은 0보다 작을 수 없습니다.");

            System.Diagnostics.Debug.Assert(cellDoubleClickRatio.Height <= 100, "CellManualFullScreenRect 의 높이비율은 100보다 클 수 없습니다.");
            System.Diagnostics.Debug.Assert(cellDoubleClickRatio.Height >= 0, "CellManualFullScreenRect 의 높이비율은 0보다 작을 수 없습니다.");

            System.Diagnostics.Debug.Assert(actualWith >= 0, "actualWith 값은 0보다 작을 수 없습니다.");
            System.Diagnostics.Debug.Assert(actualHight >= 0, "actualHight 값은 0보다 작을 수 없습니다.");

            return new Rect
            {
                X = actualWith / 100 * cellDoubleClickRatio.Left,
                Y = actualHight / 100 * cellDoubleClickRatio.Top,
                Width = actualWith / 100 * cellDoubleClickRatio.Width,
                Height = actualHight / 100 * cellDoubleClickRatio.Height
            };
        }

        /// <summary>
        /// 내부적으로 비율값을 이용하는 임의의 좌표계를 사용하기 때문에 Cell들의 화면상의 배치를 위해 ArrangeOverride를 Override한다.
        /// </summary>
        /// <param name="finalSize">
        /// final Size.
        /// </param>
        /// <returns>
        /// return Size.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in this.InternalChildren)
            {
                var arrangeRect = new Rect();

                if (child is MultiViewCell cell)
                {
                    // Cell의 위치를 finalSize에 맞춰 비율로 계산
                    Rect loc = GetLocation(cell);

                    arrangeRect.X = finalSize.Width * loc.Left / DEFAULT_SIZE;
                    arrangeRect.Y = finalSize.Height * loc.Top / DEFAULT_SIZE;
                    arrangeRect.Size = child.DesiredSize;
                }
                else
                {
                    // Cell이 아닌 경우 기본적으로 DesiredSize에 맞게 배치
                    arrangeRect.X = 0;
                    arrangeRect.Y = 0;
                    arrangeRect.Size = child.DesiredSize;
                }

                // 최종적으로 자식 요소를 배치
                child.Arrange(arrangeRect);
            }

            return finalSize;
        }

        /// <summary>
        /// 내부적으로 비율값을 이용하는 임의의 좌표계를 사용하기 때문에 Cell들의 화면상의 크기를 측정하기 위해 MeasureOverride를 Override한다.
        /// </summary>
        /// <param name="availableSize">
        /// available Size.
        /// </param>
        /// <returns>
        /// return Size.
        /// </returns>
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

        /// <summary>
        /// Cell Zoom 기능에 사용된 Transform 중 외부에서 MultiView 크기가 변경될 때 위치가 어긋나는 경우가 있어서
        /// (동시에 사용된 ScaleTransform은 별 문제 없는것으로 보임)
        /// TranslateTransform좌표를 재계산하기 위해 OnRenderSizeChanged를 override한다.
        /// </summary>
        /// <param name="sizeInfo">
        /// The size info.
        /// </param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (this._zoomedCell == null || _zoomedElement == null)
            {
                return;
            }

            _zoomedElement.SetValue(FrameworkElement.WidthProperty, sizeInfo.NewSize.Width);
            _zoomedElement.SetValue(FrameworkElement.HeightProperty, sizeInfo.NewSize.Height);

            //var grp = this._zoomedCell.RenderTransform as TransformGroup;
            //if (grp == null || grp.Children.Count < 2)
            //{
            //    return;
            //}

            //// var scaleTrans = grp.Children[0] as ScaleTransform;
            //var translateTrans = grp.Children[1] as TranslateTransform;
            //if (translateTrans == null)
            //{
            //    return;
            //}

            //Rect toRect = this.GetActualMaxRect(this._zoomedCell);
            //Rect itemRect = this.GetActualRect(this._zoomedCell);

            //double aniX = toRect.Left - itemRect.Left;
            //double aniy = toRect.Top - itemRect.Top;

            //// Size변경에 따른 Transform좌표 어긋남을 해결하기 위해 재산정한 좌표 설정.
            //DoubleAni(translateTrans, TranslateTransform.XProperty, new Duration(TimeSpan.Zero), aniX, aniX, null);
            //DoubleAni(translateTrans, TranslateTransform.YProperty, new Duration(TimeSpan.Zero), aniy, aniy, null);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 선택된 셀을 확대시킨다. 
        /// 이미 선택되어 확대된 셀은 원상 복귀시킨다.
        /// </summary>
        /// <param name="value">
        /// 더블 클릭된 셀.
        /// </param>
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
            //if (value == null)
            //{
            //    if (Public.GetProgramType() == ProgramType.iDisplay && CommonsConfig.Instance.UseSynchronousAssign)
            //    {
            //        this.ZoomOutCell_Sync();
            //    }
            //    else
            //    {
            //        this.ZoomOutCell(null);
            //    }
            //}
            //else
            //{
            //    if (Public.GetProgramType() == ProgramType.iDisplay && CommonsConfig.Instance.UseSynchronousAssign)
            //    {
            //        this.ZoomInCell_Sync(value);
            //    }
            //    else
            //    {
            //        this.ZoomInCell(value);
            //    }
            //}
        }

        /// <summary>
        /// 선택된 셀을 확대합니다.
        /// </summary>
        /// <param name="value">
        /// 더블 클릭된 셀.
        /// </param>
        //private void ZoomInCell(Cell value)
        //{
        //    if (this._zoomedCell != null)
        //    {
        //        Debug.WriteLine("The zoomed cell already exists!!");
        //        return;
        //    }

        //    this._zoomedCell = value;

        //    if (this._zoomedCell == null)
        //    {
        //        return;
        //    }

        //    if (value.GetElementCount() < 1)
        //    {
        //        this._zoomedCell = null;
        //        return;
        //    }

        //    this._zoomedCell.IsZoomed = true;

        //    this._selectionArea = GetLocation(this._zoomedCell);

        //    SetZIndex(this._zoomedCell, 2);

        //    var fromRect = this.Children.Count == 1
        //                       ? this.GetActualCenterRect()
        //                       : this.GetActualRect(this._zoomedCell);

        //    var toRect = this.GetActualMaxRect(this._zoomedCell);

        //    this.CellAnimation(
        //        this._zoomedCell,
        //        fromRect,
        //        toRect,
        //        new Action(
        //            () =>
        //            {
        //                // 확대 중인 셀은 클릭되지 않도록 함.
        //                this._zoomedCell.IsHitTestVisible = false;

        //                this.IsBusyForZoom = true;

        //                this.eZoomedCellChanging?.Invoke(this, new ZoomedCellChangeEventArgs(this._zoomedCell));                        

        //                //var cameraControls = this._zoomedCell.GetAllCameraControl();
        //                //if (cameraControls != null)
        //                //{
        //                //    if (cameraControls.Count == 1)
        //                //    {
        //                //        cameraControls[0].IsZoomInCell = true;
        //                //    }
        //                //}

        //                this._zoomedCell.HideBorder();
        //            }),
        //        new Action(
        //            () =>
        //            {
        //                if (this._zoomedCell != null)
        //                {
        //                    // UseSynchronousAssign옵션 사용 중일 경우 뒤에 가려지는 카메라를 멈추지 않는다.
        //                    //if (CommonsConfig.Instance.UseSynchronousAssign == false)
        //                    //if (false)
        //                    //{
        //                    //    // Camera High를 계산하기 위해서 뒤에 가려지는 Cell의 Visible 속성을 Hidden으로 변경함
        //                    //    foreach (Cell cell in this.Children)
        //                    //    {
        //                    //        if (Equals(cell, this._zoomedCell))
        //                    //        {
        //                    //            continue;
        //                    //        }

        //                    //        Rect cellRect = this.GetActualRect(cell);

        //                    //        // Zoom이 된 Cell의 좌표가 정확히 정수로 떨어지지 않기 때문에 gap을 둬서 계산을 함
        //                    //        const double gap = 1;

        //                    //        // Zoom된 Cell의 Rect에 나머지 Cell의 Rect가 포함되는지 Check..
        //                    //        if (toRect.X - gap <= cellRect.X && toRect.Y - gap <= cellRect.Y &&
        //                    //            toRect.X + toRect.Width + gap >= cellRect.X + cellRect.Width &&
        //                    //            toRect.Y + toRect.Height + gap >= cellRect.Y + cellRect.Height)
        //                    //        {
        //                    //            cell.Visibility = Visibility.Collapsed;                                  
        //                    //        }
        //                    //    }
        //                    //}

        //                    this.eZoomedCellChanged?.Invoke(this, new ZoomedCellChangeEventArgs(this._zoomedCell));

        //                }

        //                this.IsBusyForZoom = false;  

        //                this.Zoommed(this._zoomedCell);

        //            }));
        //}
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
                    Zoommed(currentZoomedCell);
                }

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

        //public void ZoomOutCell(int? stageIndex)
        //{
        //    if (IsHoldButtonClicked)
        //        return;
        //    //if (_zoomedCell != null)
        //    //{
        //    //if (_zoomedCell.SlideIndex == -1)
        //    //{
        //    //    eFavoriteSlideZoomOut?.Invoke(this, new MultiGridCellZoomedForStaticEventArgs(_zoomedCell, stageIndex));
        //    //}
        //    //else
        //    //{
        //    //    eSlideZoomOut?.Invoke(this, new MultiGridCellZoomedForStaticEventArgs(_zoomedCell, stageIndex));
        //    //}

        //    //eSlideZoomOut?.Invoke(this, new MultiGridCellZoomedForStaticEventArgs(_zoomedCell, stageIndex));
        //    //}

        //    if (this._zoomedCell == null)
        //    {
        //        //System.Diagnostics.Debug.Assert(this._zoomedCell != null, "ZoomOutCell() 함수가 실행될 때에는 zoomedCell에 널값이 들어 올 수 없습니다.");
        //        return;
        //    }

        //    this._zoomedCell.IsZoomed = false;

        //    var oldCell = this._zoomedCell;
        //    this._zoomedCell = null;

        //    SetZIndex(oldCell, 1);

        //    var fromRect = this.GetActualMaxRect(oldCell);

        //    //by blackRoot : Cell이 하나 있을때 가운데로 작아졌다가 다시 커지는 효과를 주기 위해서 Cell이 하나 있을 경우를 체크함 !!
        //    //그랬을 경우 Camera의 고화질 요청을 하지 못하는 문제가 발생함 !! 일단 Cell이 하나 있을 경우의 처리를 주석처리함 !!
        //    // Cell이 하나밖에 없는 상태에서 줌할 경우는 가운데로 축소 되는 느낌으로...
        //    //var toRect = this.Children.Count == 1
        //    //                 ? new Rect(this.ActualWidth / 2, this.ActualHeight / 2, 0, 0)
        //    //                 : this.GetActualRect(oldCell);
        //    var toRect = this.GetActualRect(oldCell);

        //    this.CellAnimation(
        //        oldCell,
        //        fromRect,
        //        toRect,
        //        new Action(
        //            () =>
        //            {
        //                // 확대 중인 셀은 클릭되지 않도록 함.
        //                oldCell.IsHitTestVisible = false;

        //                this.IsBusyForZoom = true;

        //                this.eZoomedCellChanging?.Invoke(this, new ZoomedCellChangeEventArgs(null));
        //            }),
        //        new Action(
        //            () =>
        //            {
        //                //this.StopVideoChildrenCamera();

        //                foreach (Cell cell in this.Children)
        //                {
        //                    //if (CommonsConfig.Instance.UseSynchronousAssign == false)
        //                    //{
        //                    //    cell.Visibility = Visibility.Visible;
        //                    //}
        //                    cell.Visibility = Visibility.Visible;
        //                }

        //                if (this.Children.Count == 1)
        //                {
        //                    oldCell.RenderTransform = null;
        //                }

        //                oldCell.ShowBorder();

        //                SetZIndex(oldCell, 0);

        //                this.eZoomedCellChanged?.Invoke(this, new ZoomedCellChangeEventArgs(null));                        

        //                //ZoomPanSmoothManager.End();
        //                this.IsBusyForZoom = false;


        //                this.Zoommed(oldCell);
        //            }));
        //}

        #region display full screen for sync

        /// <summary>
        /// 디스플레이 싱크를 위한 풀스크린 적용 함수입니다. (디스플레이에서만 사용하십시오)
        /// 기존 셀을 확대하는 방식이 아닌 새로운 셀을 만들어 풀스크린으로 표출하는 방식입니다.
        /// </summary>
        /// <param name="value"></param>
        //private void ZoomInCell_Sync(Cell value)
        //{
        //    if (this._zoomedCell != null)
        //    {
        //        //Debug.WriteLine("The zoomed cell already exists!");
        //        return;
        //    }

        //    if (value == null)
        //    {
        //        return;
        //    }

        //    if (value.GetElementCount() < 1)
        //    {
        //        this._zoomedCell = null;
        //        return;
        //    }

        //    var toRect = this.GetActualMaxRect(value);
        //    //var info = value.GetCamera(0);

        //    this.IsBusyForZoom = true;

        //    this._zoomedCell = this.MakeNewCell(toRect.Left / this.CellWidthRatio, toRect.Top / this.CellWidthRatio,
        //                                       toRect.Width / this.CellWidthRatio,
        //                                       toRect.Height / this.CellHeightRatio, true);

        //    // 확대 중인 셀은 클릭되지 않도록 함.
        //    this._zoomedCell.IsHitTestVisible = false;

        //    //this._zoomedCell.AddCamera(info);

        //    this._zoomedCell.IsZoomed = true;

        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        this.ZoomedCellChanging?.Invoke(this, new ZoomedCellChangeEventArgs(this._zoomedCell));                

        //        //var cameraControls = this._zoomedCell.GetAllCameraControl();
        //        //if (cameraControls != null)
        //        //{
        //        //    if (cameraControls.Count == 1)
        //        //    {
        //        //        cameraControls[0].IsZoomInCell = true;
        //        //    }
        //        //}

        //        //this._zoomedCell.PlaySingleCamera();

        //        this.IsBusyForZoom = false;
        //    }));
        //}

        /// <summary>
        /// 준비된 풀스크린 셀을 화면에 표출합니다.
        /// </summary>
        public void ShowPreparedFullScreen()
        {
            if (this._zoomedCell != null)
            {
                this.ZoomedCellChanged?.Invoke(this, new ZoomedCellChangeEventArgs(this._zoomedCell));

                //// 카메라와 RDS 제어 UI 정리
                //var controls = this._zoomedCell.GetAllElements();
                //if (controls != null)
                //{
                //    foreach (var control in controls)
                //    {
                //        if (control is RdsViewerControls.RdsViewerControl)
                //        {
                //            (control as RdsViewerControls.RdsViewerControl).RefreshControlUISize();
                //        }
                //    }
                //}

                SetZIndex(this._zoomedCell, 2);
            }

            this.IsBusyForZoom = false;

            // 확대 완료된 셀을 다시 클릭되도록 함.(1초를 주는 것은 사용자의 무리한 입력을 방지하기 위한것)
            //var thread = new System.Threading.Thread(this.Zoommed);
            //thread.Start(this._zoomedCell);

            //Task.Run(() =>
            //{
            //    this.Zoommed(this._zoomedCell);
            //});

            Zoommed(this._zoomedCell);
        }

        /// <summary>
        /// 디스플레이 싱크를 위한 풀스크린 해제 함수입니다. (디스플레이에서만 사용하십시오)
        /// </summary>
        //private void ZoomOutCell_Sync()
        //{
        //    if (this._zoomedCell == null)
        //    {
        //        //System.Diagnostics.Debug.Assert(this._zoomedCell != null, "ZoomOutCell() 함수가 실행될 때에는 zoomedCell에 널값이 들어 올 수 없습니다.");
        //        return;
        //    }

        //    this.IsBusyForZoom = true;

        //    this._zoomedCell.IsZoomed = false;

        //    var oldCell = this._zoomedCell;
        //    this._zoomedCell = null;

        //    this.RemoveCell(oldCell);

        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        this.ZoomedCellChanging?.Invoke(this, new ZoomedCellChangeEventArgs(null));

        //        this.ZoomedCellChanged?.Invoke(this, new ZoomedCellChangeEventArgs(null));                

        //        //ZoomPanSmoothManager.End();
        //        this.IsBusyForZoom = false;
        //    }));
        //}

        #endregion // display full screen for sync

        private static void Zoommed(object? obj)
        {
            if (obj is MultiViewCell cell)
            {
                cell.Dispatcher.Invoke(() => cell.IsHitTestVisible = true, DispatcherPriority.Render);
            }
        }

        /// <summary>
        /// 스테이지 중심점을 Rect값으로 반환한다.
        /// </summary>
        /// <returns>
        /// The Rect.
        /// </returns>
        private Rect GetActualCenterRect()
        {
            return new Rect(ActualWidth / 2, ActualHeight / 2, 0, 0);
        }

        //internal void StopVideoChildrenCamera()
        //{
        //    var cells = this.GetAllCells();
        //    foreach (var cell in cells)
        //    {
        //        var children = cell.GetAllElements();
        //        foreach (var child in children)
        //        {
        //            if (child is CameraControl)
        //            {
        //                var cameraControl = child as CameraControl;
        //                cameraControl.StopVideo();
        //            }
        //        }
        //    }
        //}

        private void SetControlledCell(MultiViewCell? value)
        {
            if (value == null)
            {
                this._controlledCell = null;
            }
            else
            {
                // Control될 Cell이 Child를 갖고 있지 않으면 Control세팅하지 않는다.
                if (value.GetElementCount() < 1)
                {
                    this._controlledCell = null;
                }
                else
                {
                    this._controlledCell = value;
                    this._selectionArea = GetLocation(value);
                }
            }

            this.ControlledCellChanged?.Invoke(this, new ControlledCellChangedEventArgs(this._controlledCell));
        }

        //private static void DoAfter(Duration duration, Delegate completed)
        //{
        //    if (completed != null)
        //    {   
        //        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        //        timer.Interval = duration.TimeSpan.Add(TimeSpan.FromMilliseconds(500));
        //        timer.Tick += (sender, e) =>
        //        {
        //            completed.DynamicInvoke();
        //            timer.Stop();
        //        };
        //        timer.Start();
        //    }
        //}

        private static bool IsSameElementList(IList<MultiViewCell> list1, IList<MultiViewCell> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (!list1[i].Equals(list2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private void Cell_MouseEnter(object sender, MouseEventArgs e)
        {
            // 셀 자동 제어 모드를 위해 주석 처리
            //if (this._controlledCell != null)
            //{
            //    return;
            //}

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

        /// <summary>
        /// DoubleClick을 체크하기 위해 꼼수 사용.
        /// 일정시간동안에 2번이상 클릭이 들어오면 DoubleClick, 한번만 들어오고 말면 SingleClick으로 간주한다.
        /// .
        /// UseShiftKeyOnCellSelection옵션 추가됨.
        /// </summary>
        /// <param name="sender">
        /// The Sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        //private void Cell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    //// Player/Editor에서 실행시 Zoom In/Out 방지 하기 위해 Handled = true로 세팅해줌.
        //    //e.Handled = true;

        //    //// 셀 자동 제어 모드를 위해 주석 처리
        //    ////if (this._controlledCell != null)
        //    ////{
        //    ////    return;
        //    ////}

        //    //if (e.ClickCount >= 2)
        //    //{
        //    //    if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
        //    //    {
        //    //        this.OnDoubleClick(sender);
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    this.OnSingleClick_UseKeyboard(sender);
        //    //}
        //}

        #region 터치 더블 클릭

        private readonly System.Diagnostics.Stopwatch _doubleTapStopwatch = new();
        private Point _lastTapLocation;

        //public event EventHandler DoubleTouchDown;

        //protected virtual void OnDoubleTouchDown()
        //{
        //    if (DoubleTouchDown != null)
        //        DoubleTouchDown(this, EventArgs.Empty);
        //}

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

        #endregion // 터치 더블 클릭

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
            //else
            //{
            //    this.OnSingleClick_UseKeyboard(sender);
            //}
        }

        private void Cell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (_rowCount == 1 && _columnCount == 1)
            //    return;

            this.MouseDownCell = sender as MultiViewCell;

            // Player/Editor에서 실행시 Zoom In/Out 방지 하기 위해 Handled = true로 세팅해줌.
            e.Handled = false;

            // 셀 자동 제어 모드를 위해 주석 처리
            //if (this._controlledCell != null)
            //{
            //    return;
            //}

            // 더블 클릭 처리
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

        // Cell을 강제로 선택하게 함
        void Cell_SelectedCell(object? sender, EventArgs e)
        {
            this.SelectCellForced(sender);
        }

        /// <summary>
        /// Cell Zoom In/Out시 에니메이션 효과 적용.
        /// </summary>
        /// <param name="cell">
        /// The Cell.
        /// </param>
        /// <param name="from">
        /// The From.
        /// </param>
        /// <param name="to">
        /// The To.
        /// </param>
        /// <param name="before">
        /// 에니메이션 시작전에 실행할 delegate.
        /// </param>
        /// <param name="completed">
        /// 에니메이션 완료후에 실행할 delegate.
        /// </param>
        //private void CellAnimation(Cell cell, Rect from, Rect to, Delegate before, Delegate completed)
        //{
        //    var grp = new TransformGroup();
        //    var scaleTrans = new ScaleTransform();
        //    var translateTrans = new TranslateTransform();

        //    grp.Children.Add(scaleTrans);
        //    grp.Children.Add(translateTrans);

        //    cell.RenderTransform = grp;

        //    //var duration = new Duration(TimeSpan.FromSeconds(CommonsConfig.Instance.CellFullScreenAnimationSpeed));
        //    var duration = new Duration(TimeSpan.FromSeconds(0.1));

        //    var itemRect = this.GetActualRect(cell);

        //    if (double.IsNaN(from.Width / itemRect.Width) ||
        //        double.IsNaN(to.Width / itemRect.Width) ||
        //        double.IsNaN(from.Height / itemRect.Height) ||
        //        double.IsNaN(to.Height / itemRect.Height))
        //    {
        //        return;
        //    }

        //    if (before != null)
        //    {
        //        before.DynamicInvoke();
        //    }

        //    // duration이 0인경우 고화질 Switching 안되는 문제가 있어서 animation을 타도록 수정함 !!
        //    // Animation Completed에서 UpdateViewArea()를 직접 호출함 !!
        //    DoubleAni(
        //        scaleTrans,
        //        ScaleTransform.ScaleXProperty,
        //        duration,
        //        from.Width / itemRect.Width,
        //        to.Width / itemRect.Width,
        //        null);

        //    DoubleAni(
        //        scaleTrans,
        //        ScaleTransform.ScaleYProperty,
        //        duration,
        //        from.Height / itemRect.Height,
        //        to.Height / itemRect.Height,
        //        null);

        //    DoubleAni(
        //        translateTrans,
        //        TranslateTransform.XProperty,
        //        duration,
        //        from.Left - itemRect.Left,
        //        to.Left - itemRect.Left,
        //        null);

        //    DoubleAni(
        //        translateTrans,
        //        TranslateTransform.YProperty,
        //        duration,
        //        from.Top - itemRect.Top,
        //        to.Top - itemRect.Top,
        //        null);

        //    if (completed != null)
        //    {
        //        completed.DynamicInvoke();                
        //    }

        //    //DoAfter(duration, completed);
        //}
        private static void CellZoomAnimation(Canvas canvas, bool isZoomIn, Action? before = null, Action? completed = null)
        {
            before?.Invoke();

            // 공통 transform 설정
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

            // Scale X
            var scaleXAnim = new DoubleAnimation(fromScale, toScale, duration)
            {
                EasingFunction = easing
            };
            Storyboard.SetTarget(scaleXAnim, canvas);
            Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            storyboard.Children.Add(scaleXAnim);

            // Scale Y
            var scaleYAnim = new DoubleAnimation(fromScale, toScale, duration)
            {
                EasingFunction = easing
            };
            Storyboard.SetTarget(scaleYAnim, canvas);
            Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboard.Children.Add(scaleYAnim);

            // Opacity
            var opacityAnim = new DoubleAnimation(fromOpacity, toOpacity, duration);
            Storyboard.SetTarget(opacityAnim, canvas);
            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(opacityAnim);

            if (completed != null)
                storyboard.Completed += (s, e) => completed();

            storyboard.Begin();
        }

        /// <summary>
        /// Transform 애니메이션을 적용하는 메서드 (중복 제거)
        /// </summary>
        private static void ApplyAnimation(Animatable target, DependencyProperty property, Duration duration, double from, double to)
        {
            if (!double.IsNaN(from) && !double.IsNaN(to))
            {
                StartDoubleAnimation(target, property, duration, from, to, null);
            }
        }

        private static void AddAnimationToStoryboard(Storyboard storyboard, DependencyObject target, DependencyProperty property, double from, double to, Duration duration)
        {
            if (!double.IsNaN(from) && !double.IsNaN(to))
            {
                var animation = new DoubleAnimation
                {
                    From = from,
                    To = to,
                    Duration = duration,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                Storyboard.SetTarget(animation, target);
                Storyboard.SetTargetProperty(animation, new PropertyPath(property));
                storyboard.Children.Add(animation);
            }
        }

        private static void ApplySizeAnimation(DependencyObject target, DependencyProperty property, Duration duration, double from, double to)
        {
            if (!double.IsNaN(from) && !double.IsNaN(to))
            {
                var animation = new DoubleAnimation
                {
                    From = from,
                    To = to,
                    Duration = duration,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };
                Storyboard.SetTarget(animation, target);
                Storyboard.SetTargetProperty(animation, new PropertyPath(property));
                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }
        }

        //private static void DoubleAni(Animatable animatable, DependencyProperty dp, Duration duration, double from, double to, Delegate completed)
        //{
        //    var ani = new DoubleAnimation { Duration = duration, From = from, To = to };

        //    if (completed != null)
        //    {
        //        ani.Completed += (sender, e) => completed.DynamicInvoke();
        //    }
        //    //ani.Changed += (sender, e) => AsyncWorker.Instance.UpdateMultiGridNotify();
        //    animatable.BeginAnimation(dp, ani);
        //}

        private static void StartDoubleAnimation(Animatable animatable, DependencyProperty dp, Duration duration, double from, double to, Action? completed = null)
        {
            var ani = new DoubleAnimation(from, to, duration);

            if (completed != null)
            {
                ani.Completed += (sender, e) => completed();
            }

            animatable.BeginAnimation(dp, ani);
        }


        /// <summary>
        /// 인자로 넘어온 Rect값이 Selection가능한지 여부.
        /// Cell의 배치를 고려해 판별한다.
        /// </summary>
        /// <param name="rect">
        /// Selection 여부를 측정할 Rect.
        /// </param>
        /// <returns>
        /// The Bool.
        /// </returns>
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

            return containedCells.SetEquals(intersectedCells);
        }

        /// <summary>
        /// 멀티 그리드의 모든 자식을 제거함. Children.Clear()를 직접 사용하지 말고 이 함수를 사용하기 바람.
        /// </summary>
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

        /// <summary>
        /// 더블클릭시 config에 있는 값을 사용하여 스테이지 기준의 셀의 크기와 위치를 반환한다.
        /// </summary>
        /// <returns>
        /// 변환될 셀의 크기와 위치.
        /// </returns>
        //private Rect GetCellDoubleClickRectByConfig()
        //{
        //    return GetCellDoubleClickRect(
        //        this.ActualWidth,
        //        this.ActualHeight,
        //        CommonsConfig.Instance.CellDoubleClickRatio);
        //}

        /// <summary>
        /// MultiView의 크기를 반환한다.
        /// </summary>
        /// <returns>
        /// the Rect.
        /// </returns>
        private Rect GetMultiViewSize()
        {
            return new Rect(0, 0, this.ActualWidth, this.ActualHeight);
        }

        private Rect GetActualZoomedLocationByRect(Rect cellRect)
        {
            Rect result = new()
            {
                Width = this.ActualWidth,
                Height = this.ActualHeight
            };

            //var cellRatio = cellRect.Width / cellRect.Height;
            //var thisRatio = this.ActualWidth / this.ActualHeight;

            // 0001654: [CM] 스테이지 분할 후 더블클릭 시 확대 안됨 
            // 특정 그리드 나눴을 때 CM에서 확대가 안되는 문제 때문에 주석처리
            //if (cellRatio > thisRatio)
            //{
            //    result.Width = this.ActualWidth;
            //    result.Height = cellRect.Height * (this.ActualWidth / cellRect.Width);
            //}
            //else
            //{
            //    result.Width = cellRect.Width * (this.ActualHeight / cellRect.Height);
            //    result.Height = this.ActualHeight;
            //}

            var center = new Point
            {
                X = cellRect.Left + (cellRect.Width / 2),
                Y = cellRect.Top + (cellRect.Height / 2)
            };

            result.X = center.X - (result.Width / 2);
            result.Y = center.Y - (result.Height / 2);

            result.X = Math.Max(0, Math.Min(result.X, this.ActualWidth - result.Width));
            result.Y = Math.Max(0, Math.Min(result.Y, this.ActualHeight - result.Height));

            //if (result.Left < 0)
            //{
            //    result.X = 0;
            //}

            //if (result.Right > this.ActualWidth)
            //{
            //    result.X = this.ActualWidth - result.Width;
            //}

            //if (result.Top < 0)
            //{
            //    result.Y = 0;
            //}

            //if (result.Bottom > this.ActualHeight)
            //{
            //    result.Y = this.ActualHeight - result.Height;
            //}

            return result;
        }

        /// <summary>
        /// 특정영역에 포함된 Cell 목록을 반환한다.
        /// </summary>
        /// <param name="area">
        /// Rect 영역.
        /// </param>
        /// <returns>
        /// Cell 목록.
        /// </returns>
        private List<MultiViewCell> GetCells(Rect area)
        {
            if (area.IsEmpty || area.Width <= 0 || area.Height <= 0)
            {
                return [];
            }

            return [..this.Children
                .OfType<MultiViewCell>()
                .Where(item => RegionHelper.IsRectContains(area, GetLocation(item), 0))];

            //var result = new List<Cell>();

            //if (area.IsEmpty || area.Width <= 0 || area.Height <= 0)
            //{
            //    return result;
            //}

            //foreach (Cell item in this.Children)
            //{
            //    Rect rect = GetLocation(item);
            //    if (RegionHelper.IsRectContains(area, rect, 0))
            //    {
            //        result.Add(item);
            //    }
            //}

            //return result;
        }

        private MultiViewCell MakeNewCell(double left, double top, double width, double height, bool isBack = false)
        {
            MultiViewCell cell = new()
            {
                Background = Brushes.Yellow
            };

            cell.PreviewMouseDown += this.Cell_PreviewMouseDown;

            //cell.MouseLeftButtonDown += this.Cell_MouseLeftButtonDown;
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

            // 더블클릭을 빠르게 하는 것을 방지
            if ((DateTime.Now - this._lastClickTime).TotalSeconds <= 1.0)
            {
                return;
            }

            this._zoomedCell = this._zoomedCell == cell ? null : cell;
            this._lastClickTime = DateTime.Now;
        }

        /// <summary>
        /// 맵에서 Cell Zoom 요청
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="isIconClick"></param>
        /// <returns></returns>
        internal bool OnMapControlCellZoomOut(MultiViewCell cell, bool isIconClick = false)
        {
            if (isIconClick && this._zoomedCell != cell)
            {
                return false;
            }

            // 더블클릭을 빠르게 하는 것을 방지
            if ((DateTime.Now - this._lastClickTime).TotalSeconds <= 1.0)
            {
                return true;
            }

            this._zoomedCell = this._zoomedCell == cell ? null : cell;
            this._lastClickTime = DateTime.Now;

            return this._zoomedCell == cell;
        }

        private void SelectCellForced(object? sender)
        {
            if (sender is MultiViewCell cell)
            {
                _selectionArea = GetLocation(cell);
            }

            //var cell = sender as Cell;
            //Rect loc = GetLocation(cell);

            //// 단일 selection 모드
            //this._selectionArea = loc;
        }

        /*
        private void OnSingleClick(object sender)
        {
            var cell = sender as Cell;
            Rect loc = GetLocation(cell);
            if (InnoRegionUtil.IsRectContains(this._selectionArea, loc, 0))
            {
                this._selectionArea = Rect.Empty;
                this.IsSelectionEnabled = true;
            }
            else
            {
                this.IsSelectionEnabled = this.CheckSelectionEnabled(loc);
                if (this.IsSelectionEnabled)
                {
                    Rect tmpSelectionArea = Rect.Union(this._selectionArea, loc);
                    this._selectionArea = tmpSelectionArea;
                }
            }
        }
        */

        /// <summary>
        /// Shift키를 사용한 MultiSelect기능을 위한 함수.
        /// </summary>
        /// <param name="sender">
        /// The Sender.
        /// </param>
        private void OnCellSelection(object sender, MouseButtonEventArgs e)
        {
            if (sender is not MultiViewCell cell)
                return;

            var loc = GetLocation(cell);

            // 우클릭으로 현재 선택 영역과 동일한 위치를 클릭하면 선택 해제
            if (e.RightButton == MouseButtonState.Pressed && RegionHelper.IsRectEquals(this._selectionArea, loc, 0))
            {
                SelectionClear();
                return;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            // 확대된 셀이 존재하면 클릭된 셀이 확대된 셀인지 확인
            if (this._zoomedCell != null && !GetCells(loc).Contains(this._zoomedCell))
                return;

            // 확대 작업 중이면 선택 방지
            if (this.IsBusyForZoom)
                return;

            // Shift 키가 눌린 경우: 선택 확장
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (CheckSelectionEnabled(loc) && !RegionHelper.IsRectContains(this._selectionArea, loc, 0))
                {
                    _selectionArea = Rect.Union(this._selectionArea, loc);
                }
                return;
            }

            // 단일 선택 모드
            _selectionArea = loc;

            //// Shift 키가 눌려 있는 경우
            //if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            //{
            //    // Zoom된 Cell이 있을 경우 선택 방지
            //    if (this._zoomedCell != null)
            //        return;

            //    // 선택 확장 모드
            //    if (!RegionHelper.IsRectContains(this._selectionArea, loc, 0))
            //    {
            //        if (CheckSelectionElabled(loc))
            //        {
            //            _selectionArea = Rect.Union(this._selectionArea, loc);
            //        }
            //    }
            //    return;
            //}

            //// Zoom된 Cell이 있을 경우
            //if (this._zoomedCell != null)
            //{
            //    var selectedCells = GetCells(loc);
            //    if (!selectedCells.Contains(this._zoomedCell))
            //        return;
            //}

            //// Zoom 동작 중이면 선택 방지
            //if (this.IsBusyForZoom)
            //    return;

            //var cell = sender as Cell;
            //Rect loc = GetLocation(cell);

            //if (RegionHelper.IsRectEquals(this._selectionArea, loc, 0))
            //{
            //    this.SelectionClear();
            //}
            //else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            //{
            //    //Zoom된 Cell이 있을 경우 Cell 선택안되게 막음 !!
            //    if (this._zoomedCell != null)
            //        return;

            //    // selction 확장 모드
            //    if (RegionHelper.IsRectContains(this._selectionArea, loc, 0))
            //    {
            //        // 무시
            //    }
            //    else
            //    {
            //        this.IsSelectionEnabled = this.CheckSelectionElabled(loc);
            //        if (this.IsSelectionEnabled)
            //        {
            //            Rect tmpSelectionArea = Rect.Union(this._selectionArea, loc);
            //            this._selectionArea = tmpSelectionArea;
            //        }
            //    }
            //}
            //else
            //{
            //    //Zoom된 Cell이 존재할경우 Zoom된 Cell만 선택이 되게 함 !!
            //    //if (this._zoomedCell == null)
            //    //    return;
            //    if (this._zoomedCell != null)
            //    {
            //        bool isSelectZoomedCell = false;
            //        List<Cell> selectedCellList = this.GetCells(loc);
            //        foreach (Cell selectedCell in selectedCellList)
            //        {
            //            if (selectedCell == this._zoomedCell)
            //            {
            //                isSelectZoomedCell = true;
            //                break;
            //            }
            //        }

            //        if (!isSelectZoomedCell)
            //            return;
            //    }

            //    if (this.IsBusyForZoom)
            //    {
            //        return;
            //    }

            //    // 단일 selection 모드
            //    this._selectionArea = loc;
            //}
        }

        private void RemoveCell(MultiViewCell cell)
        {
            //cell.Clear();

            cell.PreviewMouseDown -= Cell_PreviewMouseDown;

            //cell.MouseLeftButtonDown -= Cell_MouseLeftButtonDown;
            cell.MouseEnter -= Cell_MouseEnter;
            cell.SelectedCell -= Cell_SelectedCell;

            cell.PreviewTouchDown -= Cell_PreviewTouchDown;
            cell.TouchDown -= Cell_TouchDown;
            cell.TouchEnter -= Cell_TouchEnter;

            // Dispose all the controls within the cell
            //foreach (var element in cell.GetAllElements())
            //{
            //    if (element is not OpnxControl control) continue;

            //    control.Dispose();
            //}
            foreach (var control in cell.GetAllElements().OfType<OpnxControl>())
            {
                control.Dispose();
            }

            // TODO : Multigrid 
            //if (DragDropManager.GetDropTargetAdvisor(cell) is DragDrop.CellDropTargetAdvisor targetDropAdvisor)
            //    targetDropAdvisor.eDropCompleted -= TargetDropAdvisor_eDropCompleted;

            //DragDropManager.SetDropTargetAdvisor(cell, null);

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

            //if (!IsCellSelected())
            //{
            //    return;
            //}

            //List<Cell> removedList = GetCells(this._selectionArea);
            //if (removedList.Count < 1)
            //{
            //    return;
            //}

            //foreach (Cell cell in removedList)
            //{
            //    RemoveCell(cell);
            //}
            //eCellRemoved?.Invoke(this, new CellRemovedArgs(removedList));
        }

        /// <summary>
        /// 비어있는 다음 Cell반환
        /// </summary>
        /// <returns></returns>
        internal MultiViewCell? GetNextEmptyCell()
        {
            return GetAllCells()
            .FirstOrDefault(cell => cell.GetAllElements().Count <= 0);
        }

        /// <summary>
        /// 비어있는 Cell 중에서 왼쪽 최상단 Cell반환
        /// </summary>
        /// <returns></returns>
        internal MultiViewCell? GetNextTopLeftEmptyCell()
        {
            return GetAllCells()
                .Where(cell => cell.GetAllElements().Count == 0) // 비어있는 셀 필터링
                .OrderBy(cell => GetLocation(cell).Y) // Y 좌표 기준 정렬 (위쪽부터)
                .ThenBy(cell => GetLocation(cell).X) // X 좌표 기준 정렬 (왼쪽부터)
                .FirstOrDefault(); // 최상단 왼쪽 셀 반환
        }

        /// <summary>
        /// 작은 셀 Row Col 기준으로 하여 해당되는 Cell 반환
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
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

            //var targetCells = this.GetCells(minRect);

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








