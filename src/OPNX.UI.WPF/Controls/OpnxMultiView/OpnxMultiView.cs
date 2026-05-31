using OPNX.UI.WPF.Interactivity.DragDrop;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls
{
    /// <summary>
    /// OpnxMultiView class.
    /// </summary>
    [TemplatePart(Name = "xMultiViewPanel", Type = typeof(MultiViewPanel))]
    [TemplatePart(Name = "xZoomControl", Type = typeof(Canvas))]
    //[TemplatePart(Name = "xZoomEndButton", Type = typeof(Button))]
    //[TemplatePart(Name = "xSelectionRect", Type = typeof(System.Windows.Shapes.Rectangle))]
    public class OpnxMultiView : OpnxControl
    {
        #region Constants and Fields

        public static readonly DependencyProperty HighlightBorderStyleProperty =
            DependencyProperty.Register(
                nameof(HighlightBorderStyle),
                typeof(BorderStyle),
                typeof(OpnxMultiView),
                new FrameworkPropertyMetadata(BorderStyle.None));

        internal static readonly DependencyProperty HighlightMarginProperty =
            DependencyProperty.Register(
                nameof(HighlightMargin),
                typeof(Thickness),
                typeof(OpnxMultiView),
                new FrameworkPropertyMetadata(new Thickness()));

        internal static readonly DependencyProperty HighlightVisibilityProperty =
            DependencyProperty.Register(
                nameof(HighlightVisibility),
                typeof(Visibility),
                typeof(OpnxMultiView),
                new FrameworkPropertyMetadata(Visibility.Visible));

        internal static readonly DependencyProperty SelectionMarginProperty =
            DependencyProperty.Register(
                nameof(SelectionMargin),
                typeof(Thickness),
                typeof(OpnxMultiView),
                new FrameworkPropertyMetadata(new Thickness()));

        internal static readonly DependencyProperty SelectionVisibilityProperty =
            DependencyProperty.Register(
                nameof(SelectionVisibility),
                typeof(Visibility),
                typeof(OpnxMultiView),
                new FrameworkPropertyMetadata(Visibility.Visible));

        //internal static readonly DependencyProperty ZoomEndButtonMarginProperty =
        //    DependencyProperty.Register(
        //        nameof(ZoomEndButtonMargin),
        //        typeof(Thickness),
        //        typeof(MultiGridControl),
        //        new FrameworkPropertyMetadata(new Thickness()));

        //internal static readonly DependencyProperty ZoomEndButtonVisibilityProperty =
        //    DependencyProperty.Register(
        //        nameof(ZoomEndButtonVisibility),
        //        typeof(Visibility),
        //        typeof(MultiGridControl),
        //        new FrameworkPropertyMetadata(Visibility.Visible));

        private MultiViewCell? _highlightCell;

        //private bool isApplyTemplate;

        private MultiViewPanel? _multiViewPanel;

        private Canvas? _zoomControl;

        //private MultiGridControlInitInfo multiGridControlInitInfo;

        //private Button zoomEndButton;

        //선택시 나오는 빨간색 테두리 (PlayMode인 경우 흰색으로 변경됨)
        //private System.Windows.Shapes.Rectangle selectionRectangle;

        //private FavoriteAsyncWorker favoriteAsyncWorker = new FavoriteAsyncWorker(100, 1000);

        //private bool isPreviewMode = false;

        //private bool prevLeftButtonDownHandled = false;

        //private Point dragStartPoint;
        //private bool isDragging;

        private readonly int _edgeThickness = 1;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="OpnxMultiView"/> class.
        /// </summary>
        static OpnxMultiView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxMultiView), new FrameworkPropertyMetadata(typeof(OpnxMultiView)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpnxMultiView"/> class.
        /// </summary>
        public OpnxMultiView()
        {
            // by blackRoot : Transparent로 하면 Contents Mode의 Layout이 비춰지는 문제가 발생함 !!
            this.Background = Brushes.Black;
            // this.Background = Brushes.Transparent;

            //this.favoriteAsyncWorker.eDeserializeFavoriteData += this.favoriteAsyncWorker_eDeserializeFavoriteData;
        }

        #endregion

        #region Events

        /// <summary>
        /// The e cell drop completed.
        /// </summary>
        public event EventHandler<CellElementDropCompletedEventArgs>? CellDropCompleted;

        /// <summary>
        /// The e cell element added.
        /// </summary>
        public event EventHandler<CellElementChangedEventArgs>? CellElementAdded;

        /// <summary>
        /// The e cell element removed.
        /// </summary>
        public event EventHandler<CellElementChangedEventArgs>? CellElementRemoved;

        /// <summary>
        /// Cell FullScreen Sync.
        /// </summary>
        public event EventHandler<FullScreenChangedEventArgs>? CellFullScreenChanged;

        /// <summary>
        /// 셀 선택 변경 이벤트.
        /// </summary>
        public event EventHandler<OpnxMultiViewSelectionChangedEventArgs>? CellSelectionChanged;

        /// <summary>
        /// 셀에 마우스가 눌려짐.
        /// </summary>
        public event EventHandler<EventArgs>? CellClicked;

        /// <summary>
        /// OpnxMultiView layout changed.
        /// </summary>
        public event EventHandler<OpnxMultiViewLayoutChangedEventArgs>? LayoutChanged;

        /// <summary>
        /// GridControl Sync.
        /// </summary>
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler? FavoriteLoaded;

        #endregion

        #region Enums

        internal enum BorderStyle
        {
            /// <summary>
            /// none BorderStyle.
            /// </summary>
            None,

            /// <summary>
            /// selection enabled.
            /// </summary>
            SelectionEnabled,

            /// <summary>
            /// selection disabled.
            /// </summary>
            SelectionDisabled,
        }

        #endregion

        #region Properties
        public bool IsEnabledDragDrop { get; set; } = true;
        //public bool IsDragging => isDragging;
        /// <summary>
        /// Gets a value indicating whether IsCellSelected.
        /// </summary>
        public bool IsCellSelected
        {
            get => this._multiViewPanel?.IsCellSelected() ?? false;
        }

        /// <summary>
        /// Gets a value indicating whether IsSelectionAlignable.
        /// </summary>
        public bool IsSelectionAlignable
        {
            get => this._multiViewPanel?.IsSelectionAlignable() ?? false;
        }

        /// <summary>
        /// Gets a value indicating whether IsZoomed.
        /// </summary>
        public bool IsZoomed => this._multiViewPanel?.ZoomedCell is not null;


        public MultiViewCell? ZoomedCell
        {
            get => this._multiViewPanel?.ZoomedCell;
            set
            {
                this._multiViewPanel?.ZoomedCell = value;
            }
        }

        /// <summary>
        /// Gets SelectionArea.
        /// </summary>
        public Rect SelectionArea
        {
            get => this._multiViewPanel?.GetActualSelectionArea() ?? Rect.Empty;
        }

        internal BorderStyle HighlightBorderStyle
        {
            get => (BorderStyle)this.GetValue(HighlightBorderStyleProperty);
            set => this.SetValue(HighlightBorderStyleProperty, value);
        }

        internal Thickness HighlightMargin
        {
            get => (Thickness)this.GetValue(HighlightMarginProperty);
            set => this.SetValue(HighlightMarginProperty, value);
        }

        internal Visibility HighlightVisibility
        {
            get => (Visibility)this.GetValue(HighlightVisibilityProperty);
            set => this.SetValue(HighlightVisibilityProperty, value);
        }

        internal Thickness SelectionMargin
        {
            get => (Thickness)this.GetValue(SelectionMarginProperty);
            set => this.SetValue(SelectionMarginProperty, value);
        }

        internal Visibility SelectionVisibility
        {
            get => (Visibility)this.GetValue(SelectionVisibilityProperty);
            set => this.SetValue(SelectionVisibilityProperty, value);
        }

        //internal Thickness ZoomEndButtonMargin
        //{
        //    get
        //    {
        //        return (Thickness)this.GetValue(ZoomEndButtonMarginProperty);
        //    }

        //    set
        //    {
        //        this.SetValue(ZoomEndButtonMarginProperty, value);
        //    }
        //}

        //internal Visibility ZoomEndButtonVisibility
        //{
        //    get
        //    {
        //        return (Visibility)this.GetValue(ZoomEndButtonVisibilityProperty);
        //    }

        //    set
        //    {
        //        this.SetValue(ZoomEndButtonVisibilityProperty, value);
        //    }
        //}

        /// <summary>
        /// Gets or sets a value indicating whether PlayOnControlMode.
        /// 컨트롤 모드인 카메라만 영상을 표출하는 옵션.
        /// </summary>
        public bool PlayOnControlMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsReceivedEventForZoom.
        /// 싱크로부터 받은 명령 처리 후 다시 싱크를 전달하지 않기 위해서 사용됨.
        /// </summary>
        public bool IsReceivedEventForZoom { get; set; }

        #endregion

        #region Public Methods
        public void SelectCellForIncludeElement(UIElement elementToFind)
        {
            var panel = _multiViewPanel;
            if (panel == null)
            {
                return;
            }

            foreach (var cell in panel.GetAllCells())
            {
                foreach (var element in cell.GetAllElements())
                {
                    if (ReferenceEquals(element, elementToFind))
                    {
                        panel.SelectCell(cell);
                        return;
                    }
                }
            }
        }

        public void Show(bool isShow)
        {
            this.Visibility = isShow ? Visibility.Visible : Visibility.Hidden;

            ////Cell에 포함된 Layout들의 Visible 변경함 !!
            //foreach (Cell cell in this.multiGrid.GetAllCells())
            //{
            //    foreach (UIElement item in cell.GetAllElements())
            //    {
            //        if (item is LayoutControl)
            //        {
            //            if (isShow)
            //                (item as LayoutControl).Visibility = System.Windows.Visibility.Visible;
            //            else
            //                (item as LayoutControl).Visibility = System.Windows.Visibility.Hidden;
            //        }
            //    }
            //}
        }

        public void SwitchCellElement(MultiViewCell fromCell, MultiViewCell toCell)
        {
            if (fromCell == toCell)
                return;

            List<UIElement> fromCellElements = fromCell.GetAllElements();

            if (fromCellElements.Count <= 0)
                return;

            List<UIElement> toCellElements = toCell.GetAllElements();

            toCell.ClearElements();
            fromCell.ClearElements();

            foreach (var fromCellElement in fromCellElements)
            {
                toCell.AddElement(fromCellElement);
            }

            foreach (var toCellElement in toCellElements)
            {
                fromCell.AddElement(toCellElement);
            }

            _multiViewPanel?.SelectCell(toCell);
        }

        //public void PreviewMode()
        //{
        //    this.HighlightVisibility = System.Windows.Visibility.Collapsed;
        //    this.ZoomEndButtonVisibility = System.Windows.Visibility.Collapsed;
        //    this.SelectionVisibility = System.Windows.Visibility.Collapsed;

        //    this.selectionRectangle.Visibility = System.Windows.Visibility.Collapsed;
        //    this.zoomEndButton.Visibility = System.Windows.Visibility.Collapsed;

        //    //this.isPreviewMode = true;
        //    //this.h.Visibility = System.Windows.Visibility.Collapsed;
        //}

        /// <summary>
        /// 선택된 MultiView의 Row Count 가져오기
        /// </summary>
        /// <returns>RowCount</returns>
        public int GetSelectedRowCount()
        {
            return this._multiViewPanel?.GetSelectionRowCount() ?? 0;
        }

        /// <summary>
        /// 선택된 MultiView의 Column Count 가져오기
        /// </summary>
        /// <returns>ColumnCount</returns>
        public int GetSelectedColumnCount()
        {
            return this._multiViewPanel?.GetSelectionColumnCount() ?? 0;
        }

        /// <summary>
        /// Change FullScreen (ZoomMode).
        /// </summary>
        /// <param name="cellSyncGuid">
        /// The cell sync guid.
        /// </param>
        public void ChangeFullScreen(Guid cellSyncId)
        {
            if (this._multiViewPanel == null)
            {
                return;
            }

            this.IsReceivedEventForZoom = true;

            // FullScreen 해제 !!
            if (cellSyncId == Guid.Empty)
            {
                if (this._multiViewPanel.ZoomedCell == null)
                {
                    this.IsReceivedEventForZoom = false;
                }
                else
                {
                    this._multiViewPanel.ZoomedCell = null;
                }

                return;
            }

            // FullScreen 설정 !!
            this._multiViewPanel.ZoomedCell = this._multiViewPanel.GetAllCells().FirstOrDefault(cell => cell.SyncId == cellSyncId);
            //foreach (Cell cell in this.multiGrid.GetAllCells())
            //{
            //    if (cell.SyncGuid == cellSyncGuid)
            //    {
            //        this.multiGrid.ZoomedCell = cell;
            //        break;
            //    }
            //}

            //if (constset.ClearSelectionAfterAction)
            //{
            //    this.multiGrid.SelectionClear();
            //}
        }

        //private Rect beforeFullScreenSelectionArea = Rect.Empty;

        /// <summary>
        /// Selection Border (빨간 테두리)를 FullScreen 영역 크기로 확대함.
        /// Stage Title쪽 Grid Template Window가 뜰때 선택영역을 전체로 바꿔줌
        /// </summary>
        //public void FullScreenSelectionBorder()
        //{
        //    if (this.multiGrid == null)
        //        return;

        //    //this.beforeFullScreenSelectionArea = this.multiGrid.SelectionArea;
        //    this.SelectionAll();
        //    this.DrawSelectionRect();

        //    this.selectionRectangle.Stroke = Brushes.Red;
        //    this.selectionRectangle.StrokeThickness = 2;
        //    //this.selectionRectangle.Visibility = System.Windows.Visibility.Visible;
        //    this.SelectionVisibility = Visibility.Visible;
        //}

        /// <summary>
        /// FullScreen 영역으로 확대된 Selection Border (빨간 테두리)를 원래 크기로 만듬.
        /// </summary>
        //public void RestoreFullScreenSelectionBorder()
        //{
        //    if (this.multiGrid == null)
        //        return;

        //    //this.multiGrid.SelectionArea = this.beforeFullScreenSelectionArea;
        //    //this.beforeFullScreenSelectionArea = Rect.Empty;
        //}

        /// <summary>
        /// Grid를 Clear함.
        /// </summary>
        public void Clear()
        {
            ClearAllCellElement();

            this._multiViewPanel?.Clear();
        }

        /// <summary>
        /// Clear Cell Element.
        /// </summary>
        public void ClearAllCellElement()
        {
            if (this._multiViewPanel == null)
                return;

            this.DeleteCellElements(this._multiViewPanel.GetAllCells());

            //if (constset.ClearSelectionAfterAction)
            //{
            //    this.multiGrid.SelectionClear();
            //}

            // Sync Data를 보냄 !!
            this.SendSyncData();

            // TODO : RedrawOpnxControl 삭제 작업 (by jhlee)
            // this.RedrawOpnxControlAsync();
        }

        //private void CreateVideoForPlayback(Cell cell, CameraControlPlayback cameraControl)
        //{
        //    Rect cellRect = MultiGrid.GetLocation(cell);
        //    cameraControl.CreateVideo(new Size(MultiGrid.DEFAULT_SIZE, MultiGrid.DEFAULT_SIZE), cellRect, false);
        //}

        //private void CreateVideoForPreview(Cell cell, CameraControlPlayback cameraControl)
        //{
        //    Rect cellRect = MultiGrid.GetLocation(cell);
        //    cameraControl.CreateVideo(new Size(MultiGrid.DEFAULT_SIZE, MultiGrid.DEFAULT_SIZE), cellRect, true);
        //}

        /// <summary>
        /// 특정 Cell에 요소 설정하기.
        /// </summary>
        /// <param name="cell">요소를 설정할 셀.</param>
        /// <param name="element">설정할 요소.</param>
        public void SetCellElement(MultiViewCell cell, UIElement element)
        {
            if (cell == null || element == null)
                return;

            cell.SyncId = Guid.NewGuid();
            cell.Clear();
            cell.Add(element);

            if (this.UseMultiViewSync)
                this.SendSyncData();
        }

        /// <summary>
        /// MultiView Cell 동기화
        /// </summary>
        public void SendSyncData()
        {
            this.RaiseLayoutChanged();
        }

        /// <summary>
        /// The deserialize for favorite.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="newElementInfos">
        /// The new element infos.
        /// </param>
        //public void DeserializeForFavorite(MultiGridDataForFavorite data, List<NewMultiGridElementInfo> newElementInfos)
        //{
        //    if (this.multiGrid == null)
        //    {
        //        return;
        //    }

        //    //Favorite이 한꺼번에 들어올경우를 대비해서 FavoriteAsyncWorker에서 1초동안 다음 Favorite이 들어오는지 Check를 함 !!
        //    //this.favoriteAsyncWorker.AddFavoriteData(data, newElementInfos);
        //}

        public MultiViewCell? AddRowAndResizeCells()
        {
            return this._multiViewPanel?.AddRowAndResizeCells();
        }

        //FavoriteAsyncWorker에서 1초동안 다음 Favorite이 들어오지 않은 경우 event를 발생시킴 !! 실제 Favorite을 적용함 !!
        //void favoriteAsyncWorker_eDeserializeFavoriteData(object sender, DeserializeFavoriteDataEventArgs e)
        //{
        //    DeserializeForFavorite_Internal(e.FavoriteData, e.FavoriteNewElementInfoList);
        //}

        //private void DeserializeForFavorite_Internal(MultiGridDataForFavorite data, List<NewMultiGridElementInfo> newElementInfos)
        //{
        //    if (this.multiGrid == null)
        //    {
        //        return;
        //    }

        //    this.Clear();

        //    foreach (MultiGridCellDataForFavorite cellData in data.MultiGridCellDataForFavoriteList)
        //    {
        //        //var cell = new Cell { SyncGuid = cellData.CellSyncGuid };
        //        var cell = new Cell { SyncGuid = Guid.NewGuid() };

        //        //mutlGrid에 Cell을 먼저 추가해야 event 순서가 정상적으로 옴 !!
        //        //Add를 하면서 Cell의 Size가 결정됨
        //        this.multiGrid.AddCell(cell, cellData.RectForCanvas);

        //        foreach (NewMultiGridElementInfo info in newElementInfos)
        //        {
        //            if (info.CellGuid.Equals(cellData.CellSyncGuid))
        //            {
        //                cell.Add(info.NewElement);
        //            }
        //        }

        //        // 처음 요소를 표시
        //        if (cellData.ElementTypeList.Count > 0 && cell.GetElementCount() > 0)
        //        {
        //            cell.GetElement(0).Visibility = Visibility.Visible;
        //        }

        //        //// Slide인경우 Play 시작함 !!
        //        //if (cellData.ElementTypeList.Count > 1 && cell.GetElementCount() > 1)
        //        //{
        //        //    cell.SlideIntervalSeconds = cellData.SlideInterval;
        //        //    cell.PlaySlide();
        //        //}

        //        ////여기서 Cell을 추가하면 event 순서가 잘못 오기 때문에 Timeline쪽에 Camera 정보 표시가 안됨 !! 위에서 추가해줘야 함 !!
        //        ////this.multiGrid.AddCell(cell, cellData.RectForCanvas);

        //        //Cell 내부 실제 Camera를 생성함 !! 위 Cell Size가 결정된 후에 Video를 생성해야 함 !!
        //        //List<UIElement> cellElementList = cell.GetAllElements();
        //        //for (int i = 0; i < cellElementList.Count; i++)
        //        //{
        //            //CameraControlPlayback cameraControlPlayback = cellElementList[i] as CameraControlPlayback;
        //            //if (cameraControlPlayback != null)
        //            //{
        //            //    if (this.isPreviewMode)
        //            //        this.CreateVideoForPreview(cell, cameraControlPlayback);
        //            //    else
        //            //        this.CreateVideoForPlayback(cell, cameraControlPlayback);

        //            //}
        //        //}

        //    }

        //    //foreach (var cell in this.multiGrid.GetAllCells())
        //    //{
        //    //    cell.TimerRestart();
        //    //}

        //    // Sync를 보냄 !! (Favorite 복원은 Console에서 발생함 !! Player쪽에 Sync Message를 보내야 하기 때문에 event를 발생시킴 !!)
        //    //var localGridChanged = this.eGridChanged;
        //    //if (localGridChanged != null && data.MultiGridCellDataForFavoriteList.Count > 0)
        //    //{
        //    //    localGridChanged(this, new OpnxMultiViewLayoutChangedEventArgs(this.SyncGUID));
        //    //}
        //    if (data.MultiGridCellDataForFavoriteList.Count > 0)
        //    {
        //        this.SendSyncData();
        //    }

        //    this.OnEFavoriteLoaded();

        //    // TODO : RedrawOpnxControl 삭제 작업 (by jhlee)
        //    // this.RedrawOpnxControlAsync();

        //    // 모든 Layout의 Mouse동작을 막음 !! 
        //    // Layout 내부에 XamlViewer가 있는 경우 제어모드가 아닌데도 MouseOver 동작을 함 !!
        //    //this.ChangeIsHitTestVisibleForAllLayoutsInCells(false);
        //}

        //public void DeserializeTotalElementForSync(MultiGridTotalElementSync data, List<NewMultiGridElementInfo> newElementInfos)
        //{
        //    if (this.multiGrid == null || data.MultiGridCellTotalElementSyncList.Count < 1)
        //    {        
        //        return;
        //    }

        //    // multiGrid 자식 중 Cell 타입 컨트롤 모두 가져옴. (삭제할 Cell들)
        //    var mustRemoveCells = this.multiGrid.GetAllCells();

        //    // FillCellElement(data, mustRemoveCells, newElementInfos);
        //    MultiGridRule.CallFillCellElement(new Action(() => this.FillCellTotalElement(data, mustRemoveCells, newElementInfos)));

        //    // 재사용 하지 않는 Cell들과 Element들은 모두 삭제);
        //    foreach (var cell in mustRemoveCells)
        //    {
        //        foreach (var element in cell.GetAllElements())
        //        {
        //            if (element is IDisposable)
        //            {
        //                (element as IDisposable).Dispose();
        //            }
        //        }
        //    }

        //    this.multiGrid.RemoveCells(mustRemoveCells);
        //    mustRemoveCells.Clear();
        //}

        //private void FillCellTotalElement(MultiGridTotalElementSync data, List<Cell> mustRemoveCells, List<NewMultiGridElementInfo> newElementInfos)
        //{
        //    List<Cell> newCellList = new List<Cell>();
        //    List<Rect> newCellRectList = new List<Rect>();

        //    foreach (var sync in data.MultiGridCellTotalElementSyncList)
        //    {
        //        Cell cell = null;
        //        for (var k = 0; k < mustRemoveCells.Count; k++)
        //        {
        //            // 이전 Cell과 같은 내용을 보냈는지 확인
        //            if (mustRemoveCells[k].SyncGuid.Equals(sync.CellSyncGuid))
        //            {
        //                cell = mustRemoveCells[k];
        //                break;
        //            }
        //        }

        //        // 기존 Cell 재사용. 
        //        // 기존 Cell을 재사용하는 경우 내부 Child의 변화가 없다고 가정??
        //        // Cell에 Camera를 올리거나 Slide로 만들경우 무조건 새로운 Cell이 생성됨 !!
        //        if (cell != null)
        //        {
        //            MultiGrid.SetLocation(cell, InnoConvertUtil.ToRect(sync.RectForCanvas));
        //            mustRemoveCells.Remove(cell);

        //            var oldElements = cell.GetAllElements();

        //            // Cell 삭제 시 iCommand에서 보낸 내용 (newElementInfo = 0)일치 여부 확인 변수
        //            // newElementInfo = 0 이면 Cell 삭제 메시지
        //            bool nullElement = false;

        //            foreach (var info in newElementInfos)
        //            {
        //                if (info.CellGuid.CompareTo(cell.SyncGuid) == 0)
        //                {
        //                    nullElement = true;
        //                }
        //            }

        //            if (!nullElement)
        //            {
        //                foreach (UIElement oldElement in oldElements)
        //                {
        //                    if (oldElement is IDisposable)
        //                    {
        //                        (oldElement as IDisposable).Dispose();
        //                    }

        //                    cell.Remove(oldElement);
        //                }
        //            }
        //        }

        //        // 새로 만들어지는 Cell
        //        else
        //        {
        //            var newCell = new Cell { SyncGuid = sync.CellSyncGuid };

        //            // 여기서 Cell을 추가하게 되면 foreach 문이 꼬여버림 !! List로 보관했다 foreach문이 끝난후 새로생성된 Cell을 Add해줌 !!
        //            // this.multiGrid.AddCell(newCell, InnoConvertUtil.ToRect(sync.RectForCanvas));
        //            newCellList.Add(newCell);
        //            newCellRectList.Add(InnoConvertUtil.ToRect(sync.RectForCanvas));

        //            // IsVisible이 true인 element가 UI상 가장 위에 오게 배치함 !!
        //            UIElement visibleElement = null;
        //            foreach (var info in newElementInfos)
        //            {
        //                if (info.CellGuid.Equals(newCell.SyncGuid))
        //                {
        //                    if (info.IsVisible && visibleElement == null)
        //                    {
        //                        visibleElement = info.NewElement;
        //                    }
        //                    else
        //                    {
        //                        newCell.AddElementWithoutBinding(info.NewElement);
        //                    }
        //                }
        //            }

        //            if (visibleElement == null)
        //            {
        //                if (newCell.GetAllElements().Count < 1)
        //                {
        //                    continue;
        //                }

        //                visibleElement = newCell.GetAllElements()[0];
        //                newCell.BindingElementSize(visibleElement);
        //            }
        //            else
        //            {
        //                newCell.InsertNoBinding(0, visibleElement);
        //                newCell.BindingElementSize(visibleElement);
        //            }

        //            var elementList = newCell.GetAllElements();
        //            if (elementList.Count < 1)
        //            {
        //                continue;
        //            }

        //            ////Slide인 경우 전체 Camera들의 Visibility속성이 Visible로 들어옴 !! 
        //            ////iDisplay쪽에서 처음 한번 검게 나오는 증상을 막기 위해서 Visibility속성을 Visible로 Fix해서 생성함 !!
        //            ////화면에 보이지 않는 Cell의 Element들이 고화질 Switching에 참여를 하게 되는 문제가 발생함 !!
        //            ////화면에 보이지 않는 Cell Element들의 Width, Height를 1로 해서 고화질 Switching이 안되게 막음 !!
        //            ////SlideChanged Event가 왔을때 정상적인 값으로 세팅을 함 !!
        //            //if (newCell.IsSlideMode())
        //            //{
        //            //    for (int i = 1; i < elementList.Count; i++)
        //            //    {
        //            //        var cameraControl = elementList[i] as CameraControl;
        //            //        if (cameraControl != null && cameraControl != visibleElement)
        //            //        {
        //            //            cameraControl.Width = 1;
        //            //            cameraControl.Height = 1;
        //            //        }
        //            //    }
        //            //}
        //        }
        //    }

        //    for (int i = 0; i < newCellList.Count; i++)
        //    {
        //        var newCell = newCellList[i];
        //        var newCellRect = newCellRectList[i];
        //        this.multiGrid.AddCell(newCell, newCellRect);
        //    }

        //    newCellList.Clear();
        //    newCellRectList.Clear();

        //    this.SelectionClear();
        //}

        /// <summary>
        /// The get cell elements by guid.
        /// </summary>
        /// <param name="guid">
        /// The guid.
        /// </param>
        /// <returns>
        /// UIElement List.
        /// </returns>
        public List<UIElement> GetCellElementsByGuid(Guid guid)
        {
            return this._multiViewPanel?.GetAllCells().FirstOrDefault(cell => cell.SyncId == guid)?.GetAllElements() ?? [];

            //if (this.multiGrid == null)
            //{
            //    return new List<UIElement>();
            //}

            //foreach (Cell cell in this.multiGrid.GetAllCells())
            //{
            //    if (cell.SyncGuid == guid)
            //    {
            //        return cell.GetAllElements();
            //    }
            //}

            //return new List<UIElement>();
        }

        public static List<UIElement> GetCellElements(MultiViewCell cell)
        {
            return cell?.GetAllElements() ?? [];
            //if (cell == null)
            //    return new List<UIElement>();
            //return cell.GetAllElements();
        }

        /// <summary>
        /// 현재 선택된 모든 Cell들을 반환한다
        /// </summary>
        public List<MultiViewCell>? GetCellsWithinSelectionArea()
        {
            return this._multiViewPanel?.GetCellsWithinSelectionArea();
        }

        /// <summary>
        /// 현재 선택되지 않은 모든 Cell들을 반환한다
        /// </summary> 
        public List<MultiViewCell>? GetCellsWithoutSelectionArea()
        {
            return this._multiViewPanel?.GetCellsWithoutSelectionArea();
        }

        /// <summary>
        /// UIElement가 포함된 셀 반환
        /// </summary>
        /// <param name="uiElement"></param>
        /// <returns></returns>
        public MultiViewCell? GetCell(UIElement uiElement)
        {
            return this._multiViewPanel?.GetCell(uiElement);
        }

        /// <summary>
        /// 비어있는 마지막 쎌 반환
        /// </summary>
        /// <returns></returns>

        public MultiViewCell? GetNextEmptyCell()
        {
            return this._multiViewPanel?.GetNextEmptyCell();
        }

        /// <summary>
        /// 비어있는 왼쪽 최상단 쎌 반환
        /// </summary>
        /// <returns></returns>
        public MultiViewCell? GetNextTopLeftEmptyCell()
        {
            return this._multiViewPanel?.GetNextTopLeftEmptyCell();
        }

        /// <summary>
        /// 모든 Cell 반환
        /// </summary>
        /// <returns></returns>
        public List<MultiViewCell>? GetAllCells()
        {
            return this._multiViewPanel?.GetAllCells();
        }

        /// <summary>
        /// Get SelectionCellCount.
        /// </summary>
        /// <returns>
        /// return SelectionCellCount.
        /// </returns>
        public int GetSelectionCellCount()
        {
            return this._multiViewPanel?.GetSelectedCells().Count ?? 0;
            //if (this.multiGrid == null)
            //{
            //    return 0;
            //}

            //return this.multiGrid.GetSelectedCells().Count;
        }

        /// <summary>
        /// Get Selection Cell
        /// </summary>
        /// <returns>
        /// return Selection Cell
        /// </returns>
        public List<MultiViewCell>? GetSelectionCell()
        {
            return this._multiViewPanel?.GetSelectedCells() ?? null;
            //if (this.multiGrid == null)
            //{
            //    return null;
            //}

            //return this.multiGrid.GetSelectedCells();
        }

        /// <summary>
        /// Get SelectionElements.
        /// </summary>
        /// <returns>
        /// return list of UIElement.
        /// </returns>
        public List<UIElement> GetSelectionElements()
        {
            return this._multiViewPanel?.GetSelectedCells().SelectMany(cell => cell.GetAllElements()).ToList() ?? [];
            //var result = new List<UIElement>();

            //if (this.multiGrid == null)
            //{
            //    return result;
            //}

            //foreach (Cell cell in this.multiGrid.GetSelectedCells())
            //{
            //    result.AddRange(cell.GetAllElements());
            //}

            //return result;
        }

        public List<UIElement> GetAllElements()
        {
            var result = this._multiViewPanel?.GetAllCells().SelectMany(cell => cell.GetAllElements()).ToList() ?? [];
            var zoomElements = GetZoomedElements();
            if (zoomElements.Count > 0)
                result.AddRange(zoomElements);

            return result;

            //var result = new List<UIElement>();

            //if (this.multiGrid == null)
            //{
            //    return result;
            //}

            //foreach (Cell cell in this.multiGrid.GetAllCells())
            //{
            //    result.AddRange(cell.GetAllElements());
            //}

            //return result;
        }

        public List<UIElement> GetZoomedElements()
        {
            return this._multiViewPanel?.ZoomCanvas?.Children.Cast<UIElement>().ToList() ?? [];

            //var result = new List<UIElement>();

            //result.AddRange(this.multiGrid.ZoomedCell.GetAllElements());

            //return result;
        }

        //public List<CameraControlPlayback> GetAllCameraControls()
        //{
        //    List<UIElement> elementList = this.GetAllElements();

        //    var result = new List<CameraControlPlayback>();

        //    for (int i = 0; i < elementList.Count; i++)
        //    {
        //        if (elementList[i] is CameraControlPlayback)
        //            result.Add(elementList[i] as CameraControlPlayback);
        //    }

        //    return result;
        //}

        //public CameraControlPlayback GetSelectedCameraControl()
        //{
        //    List<UIElement> elementList = this.GetAllElements();
        //    foreach (CameraControlPlayback cameraControl in elementList)
        //    {
        //        if (cameraControl == null)
        //            continue;

        //        if (cameraControl.IsTitleSelected)
        //        {
        //            return cameraControl;
        //        }
        //    }

        //    return null;
        //}

        /// <summary>
        /// Init MultiView.
        /// </summary>
        /// <param name="rowCount">
        /// The row count.
        /// </param>
        /// <param name="columnCount">
        /// The column count.
        /// </param>
        /// <param name="isSelectionAll">
        /// The is selection all.
        /// </param>
        /// <param name="isLocked">
        /// The is lock all.
        /// </param>

        public void Init(int rowCount, int columnCount, bool isSelectionAll, bool isLocked, FrameworkElement initElement)
        {
            if (this._multiViewPanel == null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    this.Init(rowCount, columnCount, isSelectionAll, isLocked, initElement);
                }));
                return;
            }

            // 실제 초기화 처리
            this.Clear();
            this.Background = new SolidColorBrush(Colors.Transparent);
            this._multiViewPanel.Init(rowCount, columnCount);

            if (isSelectionAll)
            {
                this._multiViewPanel.SelectionAll();
            }

            if (initElement != null)
            {
                SetCellElementSilenceMode(this._multiViewPanel.GetSelectedCells()[0], initElement);
            }

            if (this.UseMultiViewSync)
            {
                this.SendSyncData();
            }
        }

        /// <summary>
        /// 최초 1,1 셀을 선택해줌.
        /// </summary>
        //public void FirstCellSelecetion()
        //{
        //    this.multiGrid.FirstCellSelecetion();
        //}

        /// <summary>
        /// override OnApplyTemplate.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._multiViewPanel = this.Template.FindName("xMultiViewPanel", this) as MultiViewPanel;
            if (this._multiViewPanel != null)
            {
                this._multiViewPanel.MultiView = this;

                //this.multiGrid.CellDropCompleted += this.MultiGrid_CellDropCompleted;
                this._multiViewPanel.CellAdded += this.MultiViewPanel_CellAdded;
                this._multiViewPanel.CellRemoved += this.MultiViewPanel_CellRemoved;
                this._multiViewPanel.SelectionChanged += this.MultiViewPanel_SelectionChanged;
                //this.multiGrid.eCellClicked += this.MultiGrid_eCellClicked;
                this._multiViewPanel.ZoomedCellChanging += this.MultiViewPanel_ZoomedCellChanging;
                this._multiViewPanel.ZoomedCellChanged += this.MultiViewPanel_ZoomedCellChanged;
                //this.multiGrid.eControlledCellChanged += this.MultiGrid_eControledCellChanged;

                this._multiViewPanel.SizeChanged += this.MultiViewPanel_SizeChanged;

                // DONE -blackRoot : OnApplyTemplate()함수가 Init()보다 늦게 들어오는 문제가 발생함 !!
                // Init()함수에서 멤버로 저장하고 있다가 OnApplyTemplate()에서 한번더 실행해줌 !!
                //if (this.multiGridControlInitInfo != null)
                //{
                //    this.Init_Internal(
                //        this.multiGridControlInitInfo.InitRow,
                //        this.multiGridControlInitInfo.InitColumn,
                //        this.multiGridControlInitInfo.InitIsSelectionAll,
                //        this.multiGridControlInitInfo.InitElement);
                //}

                // IsControlMode도 위와 마찬가지로 동작한다.
                //this.IsControlMode = this.preControlMode;
            }
            this._zoomControl = this.Template.FindName("xZoomControl", this) as Canvas;
            if (this._zoomControl != null)
            {
                Panel.SetZIndex(this._zoomControl, -1); // 기본은 뒤로 보내두고

                this._zoomControl.Children.Clear();

                // 필요하면 초기 스타일 설정
                this._zoomControl.Visibility = Visibility.Collapsed;
                this._zoomControl.IsHitTestVisible = false;
                this._zoomControl.Background = Brushes.Transparent;
                this._zoomControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                this._zoomControl.VerticalAlignment = VerticalAlignment.Stretch;
            }


            //this.zoomEndButton = this.Template.FindName("xZoomEndButton", this) as Button;
            //if (this.zoomEndButton != null)
            //{
            //    this.zoomEndButton.Click += this.ZoomEndButton_Click;
            //}

            //this.selectionRectangle = this.Template.FindName("xSelectionRect", this) as System.Windows.Shapes.Rectangle;

            this.SelectionVisibility = Visibility.Collapsed;

            //this.isApplyTemplate = true;
        }

        /// <summary>
        /// Update View Area
        /// </summary>
        public override void UpdateViewArea(Rect parentCanvasScreenRegion, List<Rect>? parentsScreenRegion)
        {
            //Camera갱신을 알림 !!
            //AsyncWorker.Instance.UpdateNotify();
            //this.UpdateViewArea_Internal();
        }

        /// <summary>
        /// UpdateViewArea Internal
        /// </summary>
        //private void UpdateViewArea_Internal()
        //{
        //if (this.multiGrid == null)
        //{
        //    return;
        //}

        //// 부모와 연결되어 있지 않으면 처리하지 않음.
        //if (PresentationSource.FromVisual(this) == null)
        //{
        //    return;
        //}

        //// 자신을 포함한 모든 부모의 요소를 가져옴.
        ////var ancestorList = InnoControlUtil.GetAllParents(this, false);
        //List<FrameworkElement> ancestorList = InnoControlUtil.GetAllParents(this, false);
        //if (ancestorList.Any(parent => parent.Visibility != Visibility.Visible))
        //{
        //    ancestorList = null;
        //}

        //// 자신을 포함한 모든 부모의 화면 영역 계산.
        //var ancestorScreenRegionList = new List<Rect>();
        //if (ancestorList != null)
        //{
        //    foreach (FrameworkElement element in ancestorList)
        //    {
        //        //if (element.IsVisible == true)
        //        if (element.Visibility == Visibility.Visible)
        //        {
        //            var elementScreenRegion = new Rect(
        //                element.PointToScreen(new Point(0, 0)),
        //                element.PointToScreen(new Point(element.ActualWidth, element.ActualHeight)));

        //            ancestorScreenRegionList.Add(elementScreenRegion);
        //        }
        //    }
        //}

        //// Visible이 False 이더라도 아래쪽 UpdateViewArea 해야함. (주석 풀지마세요)
        ////if (this.IsVisible == false)
        ////    return;

        //// 자기 자신 화면 영역 계산
        //var parentRect = new Rect(
        //    this.PointToScreen(new Point(0, 0)),
        //    this.PointToScreen(new Point(this.ActualWidth, this.ActualHeight)));

        //var cells = this.multiGrid.GetAllCells();

        ////카메라 및 RDS 영역 Update..
        //foreach (Cell cell in cells)
        //{
        //    foreach (UIElement item in cell.GetAllElements())
        //    {
        //        //Camera와 RDS의 영역 재계산 !!
        //        //RDS는 Camera를 상속받았기 때문에 CameraControl만 처리해줌 !!
        //        if (item is CameraControlPlayback)
        //        {
        //            (item as CameraControlPlayback).UpdateViewArea(parentRect, ancestorScreenRegionList);
        //        }
        //    }
        //}
        //}

        /// <summary>
        /// Remove CellElements.
        /// </summary>
        /// <param name="elements">
        /// The elements.
        /// </param>
        public void RemoveCellElements(IList<UIElement> elements)
        {
            if (this._multiViewPanel == null || !this._multiViewPanel.IsCellSelected() || elements == null || elements.Count <= 0)
            {
                return;
            }

            var selectedCellList = this._multiViewPanel.GetSelectedCells();
            if (selectedCellList.Count <= 0) return;

            // element 삭제 !!
            foreach (UIElement element in elements)
            {
                foreach (MultiViewCell selectedCell in selectedCellList)
                {
                    selectedCell.Remove(element);
                }
            }

            // Grid 재정렬 !!
            var existElementList = selectedCellList.SelectMany(cell => cell.GetAllElements()).ToList();

            foreach (var selectedCell in selectedCellList)
            {
                selectedCell.Clear();
            }
            //var existElementList = new List<UIElement>();
            //foreach (Cell selectedCell in selectedCellList)
            //{
            //    existElementList.AddRange(selectedCell.GetAllElements());
            //    selectedCell.Clear();
            //}

            if (existElementList.Count > 0)
            {
                // 실제 Cell 분할 및 Element추가는 SetCellElements함수로 위임한다.
                this.SetCellElements(existElementList);
            }
            else
            {
                this._multiViewPanel.SelectionMerge();
            }
        }

        public void RemoveCellElement(UIElement element)
        {
            var cell = _multiViewPanel?.GetCell(element);
            if (cell != null)
            {
                DeleteCellElements(cell);
            }
        }

        /// <summary>
        /// Selection All.
        /// </summary>
        public void SelectionAll()
        {
            this._multiViewPanel?.SelectionAll();
            //if (this.multiGrid != null)
            //{
            //    this.multiGrid.SelectionAll();            
            //}
        }

        /// <summary>
        /// Selection Clear.
        /// </summary>
        public void SelectionClear()
        {
            this._multiViewPanel?.SelectionClear();

            //if (this.multiGrid != null)
            //{
            //    this.multiGrid.SelectionClear();
            //}
        }

        /// <summary>
        /// Selection Merge.
        /// </summary>
        public MultiViewCell? SelectionMerge()
        {
            if (this._multiViewPanel == null)
            {
                return null;
            }

            //Cell에 Element 추가시 FullScreen Cell을 먼저 해제함 !!
            if (this._multiViewPanel.ZoomedCell != null)
                this._multiViewPanel.ZoomedCell = null;

            this.DeleteCellElements(this._multiViewPanel.GetSelectedCells());

            var result = this._multiViewPanel.SelectionMerge();

            //Cell cell = this.multiGrid.SelectionMerge();

            // Sync Data를 보냄 !!
            //var localGridChanged = this.eGridChanged;
            //if (localGridChanged != null && cell != null)
            //{
            //    localGridChanged(this, new OpnxMultiViewLayoutChangedEventArgs(this.SyncGUID));
            //}
            this.SendSyncData();

            return result;

            //if (constset.ClearSelectionAfterAction)
            //{
            //    this.multiGrid.SelectionClear();
            //}
        }

        /// <summary>
        /// Selection Split.
        /// </summary>
        /// <param name="rowCount">
        /// The row count.
        /// </param>
        /// <param name="columnCount">
        /// The column count.
        /// </param>
        /// <exception cref="CellSplitException">
        /// </exception>
        public void SelectionSplit(int rowCount, int columnCount)
        {
            if (this._multiViewPanel == null) return;

            if (this.GetSelectionCellCount() <= 0) return;

            if (this.CheckSplitEnable(rowCount, columnCount) == false)
            {
                throw new CellSplitException("Cell is Too Small!");
            }

            this.DeleteCellElements(this._multiViewPanel.GetSelectedCells());

            List<MultiViewCell> cellList = this._multiViewPanel.SelectionSplit(rowCount, columnCount);
            if (cellList.Count > 0)
            {
                this.SendSyncData();
            }

            // Sync Data를 보냄 !!
            //var localGridChanaged = this.eGridChanged;
            //if (localGridChanaged != null && cellList.Count > 0)
            //{
            //    localGridChanaged(this, new OpnxMultiViewLayoutChangedEventArgs(this.SyncGUID));
            //}


            //if (constset.ClearSelectionAfterAction)
            //{
            //    this.multiGrid.SelectionClear();
            //}
        }

        //public MultiGridTotalElementSync SerializeTotalElementForSync()
        //{
        //    var data = new MultiGridTotalElementSync();

        //    if (data.MultiGridCellTotalElementSyncList == null)
        //    {
        //        data.MultiGridCellTotalElementSyncList = new MultiGridCellTotalElementSyncList();
        //    }

        //    if (this.multiGrid == null)
        //    {
        //        return data;
        //    }

        //    data.MultiViewSyncGuid = this.SyncGUID;

        //    foreach (Cell cell in this.multiGrid.GetAllCells())
        //    {
        //        Rect cellRect = MultiGrid.GetLocation(cell);
        //        Rectangle cellRectangle = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

        //        // Cell 정보를 저장함 !!
        //        var cellData = new MultiGridCellTotalElementSync(this.SyncGUID, cell.SyncGuid, cellRectangle);

        //        foreach (UIElement item in cell.GetAllElements())
        //        {
        //            bool isVisible = false;
        //            if (item.Visibility == System.Windows.Visibility.Visible)
        //                isVisible = true;

        //            if (item is CameraControlPlayback)
        //            {
        //                var camera = item as CameraControlPlayback;
        //                cellData.AddCameraData(camera.SyncGUID, camera.ID, isVisible);
        //            }
        //        }

        //        data.AddCellData(cellData);
        //    }

        //    return data;
        //}

        /// <summary>
        /// Grid 정보를 xml로 변환해서 return 함 !! (Sync Data를 보내기 위함).
        /// Grid 정보 + Cell들의 모든 정보(rect, guid, element list...).
        /// </summary>
        /// <returns>
        /// return xmlData.
        /// </returns>
        //public string SerializeDataForFavorite()
        //{
        //    MultiGridDataForFavorite data = this.SerializeForFavorite();
        //    string xmlData = data.SaveDataToXML();

        //    return xmlData;
        //}

        public void SelectionFullScreen()
        {
            var panel = _multiViewPanel;
            if (panel == null)
                return;

            if (IsZoomed)
            {
                panel.SetZoomedCell(null);
                return;
            }

            var cells = GetSelectionCell();
            if (cells != null && cells.Count > 0)
            {
                panel.SetZoomedCell(cells[0]);
            }
        }

        public void ZoomInCell(MultiViewCell cell, Action? onComplete = null)
        {
            _multiViewPanel?.ZoomInCell(cell, onComplete);
        }


        public void ZoomOutCell(Action? onComplete = null)
        {
            _multiViewPanel?.ZoomOutCell(onComplete);
        }

        public List<MultiViewCellLayout> GetLayout()
        {
            var result = new List<MultiViewCellLayout>();
            var panel = _multiViewPanel;

            if (panel == null)
            {
                return result;
            }

            foreach (MultiViewCell cell in panel.GetAllCells())
            {
                var cellLayout = new MultiViewCellLayout
                {
                    SyncId = cell.SyncId,
                    RectForCanvas = cell.RectForCanvas,
                };

                result.Add(cellLayout);
            }

            return result;
        }

        public MultiViewLayout SaveLayout()
        {
            var newLayout = new MultiViewLayout();

            if (this._multiViewPanel == null)
            {
                return newLayout;
            }

            newLayout.CellLayouts.AddRange(GetLayout());

            return newLayout;
        }

        public void LoadLayout(IMultiViewLayout newLayout)
        {
            LoadLayout(newLayout.CellLayouts);
        }

        public void LoadLayout(IEnumerable<IMultiViewCellLayout> cellLayouts)
        {
            if (this._multiViewPanel == null)
            {
                return;
            }

            this.Clear();

            foreach (var cellLayout in cellLayouts)
            {
                var cell = new MultiViewCell { SyncId = cellLayout.SyncId };
                this._multiViewPanel.AddCell(cell, cellLayout.RectForCanvas);
            }
        }



        /// <summary>
        /// Serialize For Favorite.
        /// </summary>
        /// <returns>
        /// return MultiGridDataForFavorite.
        /// </returns>
        //public MultiGridDataForFavorite SerializeForFavorite()
        //{
        //    var data = new MultiGridDataForFavorite();

        //    if (this.multiGrid == null)
        //    {
        //        return data;
        //    }

        //    // Favorite 복원은 GridControl의 SyncGuid와 상관없이 진행돼야 함 !!
        //    // GridControl 내부만 복원을 함 !!
        //    data.MultiViewSyncGuid = Guid.Empty;

        //    foreach (Cell cell in this.multiGrid.GetAllCells())
        //    {
        //        Rect cellRect = MultiGrid.GetLocation(cell);

        //        // Cell 정보를 저장함 !!
        //        //var cellData = new MultiGridCellDataForFavorite(cell.SyncGuid, cellRect);

        //        //foreach (UIElement item in cell.GetAllElements())
        //        //{
        //        //    if (item is CameraControlPlayback)
        //        //    {
        //        //        var camera = item as CameraControlPlayback;
        //        //        cellData.AddCameraData(camera.SyncGUID, camera.ID);
        //        //    }
        //        //}

        //        //data.AddCellData(cellData);
        //    }

        //    return data;
        //}

        /// <summary>
        /// 선택된 Cell의 전체 Element를 지우고 새로 갱신함 !!.
        /// </summary>
        /// <param name="elementList">
        /// The element list.
        /// </param>
        /// <exception cref="CellSplitException">
        /// </exception>
        public void SetCellElements(IList<UIElement> elementList)
        {
            //if (this.multiGrid == null || !this.multiGrid.IsCellSelected() || elementList.Count < 1)
            //{
            //    return;
            //}

            if (this._multiViewPanel == null || elementList.Count < 1)
            {
                return;
            }

            //Cell에 Element 추가시 FullScreen Cell을 먼저 해제함 !!
            if (this._multiViewPanel.ZoomedCell != null)
                this._multiViewPanel.ZoomedCell = null;

            int rowCount, columnCount;
            rowCount = columnCount = (int)Math.Ceiling(Math.Sqrt((double)elementList.Count));
            if (this.CheckSplitEnable(rowCount, columnCount) == false)
            {
                throw new CellSplitException("Cell is Too Small!");
            }

            this.DeleteCellElements(this._multiViewPanel.GetSelectedCells());

            List<MultiViewCell> cellList = this._multiViewPanel.SelectionSplit(rowCount, columnCount);
            for (int i = 0; (i < cellList.Count) && (i < elementList.Count); ++i)
            {
                cellList[i].Clear();
                cellList[i].Add(elementList[i]);
            }

            // Sync Data를 보냄 !!
            //if (cellList.Count > 0)
            //{
            //    this.SendSyncData();
            //}            

            this._multiViewPanel.SelectionClear();

            this.RaiseLayoutChanged();
        }

        public void SetCellElements(MultiViewCell cell, IList<UIElement> elementList)
        {
            if (this._multiViewPanel == null)
            {
                return;
            }

            if (elementList == null)
            {
                return;
            }

            this._multiViewPanel.SelectCell(cell);
            this.SetCellElements(elementList);
        }

        /// <summary>
        /// Cell의 전체 Element를 지우고 새로 갱신함 !!
        /// 조용히 element만 바꿈 !! 현재 ChangeLayout Action 발생했을때 호출해줌 !!
        /// </summary>
        public void SetCellElementSilenceMode(MultiViewCell cell, UIElement element)
        {
            if (this._multiViewPanel == null || cell == null || element == null)
            {
                return;
            }

            OpnxMultiView.DeleteCellElements(cell);

            cell.Clear();
            cell.Add(element);

            //if (element is CameraControlPlayback)
            //{
            //    if (this.isPreviewMode)
            //        this.CreateVideoForPreview(cell, element as CameraControlPlayback);
            //    else
            //        this.CreateVideoForPlayback(cell, element as CameraControlPlayback);

            //}

            //by blackRoot : OpenLayout, ChangeLayout인 경우 MultiGrid Message를 보내지 않고 기존 OpenLayout, ChangeLayout Message를 보내서 처리함 !!
            //여기 함수를 호출하기 전 이미 OpenLayout, ChangeLayout Message를 서버에 보냈음 !!
            /*
            // Sync Data를 보냄 !!
            if (this.eCellVisibleElementChanged != null)
            {
                this.eCellVisibleElementChanged(this, new SlideChangedEventArgs(this.SyncGUID, cell.SyncGuid, element, 0));
            }
            */
        }

        #endregion

        #region Private / Protected Methods 

        /// <summary>
        /// 속성 변경 알림 이벤트 핸들러.
        /// </summary>
        /// <param name="e">Dependency Property Changed Event Args.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            var name = e.Property.Name;

            if (name == nameof(Left) ||
                name == nameof(Top) ||
                name == nameof(ActualWidth) ||
                name == nameof(ActualHeight) ||
                name == nameof(IsVisible))
            {
                UpdateViewArea(Rect.Empty, null);
            }
        }

        /// <summary>
        /// override DoDispose.
        /// </summary>
        /// <param name="isManage">
        /// The is manage.
        /// </param>
        protected override void DoDispose(bool isManage)
        {
            if (isManage)
            {
                if (this._multiViewPanel != null)
                {
                    this._multiViewPanel.CellAdded -= this.MultiViewPanel_CellAdded;
                    this._multiViewPanel.CellRemoved -= this.MultiViewPanel_CellRemoved;
                    this._multiViewPanel.SelectionChanged -= this.MultiViewPanel_SelectionChanged;
                    this._multiViewPanel.SelectionChanged -= this.MultiViewPanel_CellClicked;
                    this._multiViewPanel.ZoomedCellChanging -= this.MultiViewPanel_ZoomedCellChanging;
                    this._multiViewPanel.ZoomedCellChanged -= this.MultiViewPanel_ZoomedCellChanged;
                    //this.multiGrid.eControlledCellChanged -= this.MultiGrid_eControledCellChanged;
                    this._multiViewPanel.SizeChanged -= this.MultiViewPanel_SizeChanged;

                    this.DeleteCellElements(this._multiViewPanel.GetAllCells());

                    this._multiViewPanel.Clear();
                    this._multiViewPanel = null;
                }

                //if (this.zoomEndButton != null)
                //{
                //    this.zoomEndButton.Click -= this.ZoomEndButton_Click;
                //    this.zoomEndButton = null;
                //}
            }

            base.DoDispose(isManage);
        }

        /// <summary>
        /// The on render size changed.
        /// </summary>
        /// <param name="sizeInfo">
        /// The size info.
        /// </param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            this.DrawHighlightRect();
            this.DrawSelectionRect();
            //this.DrawControlModeRect();
        }

        private void Cell_ItemAdded(object? sender, CellElementChangedEventArgs e)
        {
            this.CellElementAdded?.Invoke(this, e);
        }

        private void Cell_ItemRemoved(object? sender, CellElementChangedEventArgs e)
        {
            this.CellElementRemoved?.Invoke(this, e);
        }

        private void Cell_ViewAreaUpdated(object? sender, EventArgs e)
        {
            this.UpdateViewArea(Rect.Empty, null);
        }

        private void Item_VideoElementCreated(object sender, EventArgs e)
        {
            this.CellClicked?.Invoke(this, new EventArgs());
        }

        private void Item_DropCompleted(object? sender, DropTargetAdvisorDropCompletedEventArgs e)
        {
            if (e.TargetUIElement is not MultiViewCell targetCell)
                return;

            CellDropCompleted?.Invoke(this, new CellElementDropCompletedEventArgs(this, e.Source, targetCell));
        }

        private bool CheckSplitEnable(int rowCount, int columnCount)
        {
            var panel = _multiViewPanel;
            if (panel == null || rowCount <= 0 || columnCount <= 0)
            {
                return false;
            }

            double eachWidth = SelectionArea.Width / columnCount;
            double eachHeight = SelectionArea.Height / rowCount;

            double panelWidth = panel.ActualWidth;
            double panelHeight = panel.ActualHeight;

            if (panelWidth <= 0 || panelHeight <= 0)
            {
                return false;
            }

            double eachWidthRatio = eachWidth / panelWidth * 100;
            double eachHeightRatio = eachHeight / panelHeight * 100;

            return eachWidthRatio >= 2 && eachHeightRatio >= 2;
        }

        private static void DeleteCellElements(MultiViewCell cell)
        {
            foreach (var element in cell.GetAllElements())
            {
                (element as IDisposable)?.Dispose();
            }

            cell.Clear();
        }

        private void DeleteCellElements(IEnumerable<MultiViewCell> cells)
        {
            var panel = _multiViewPanel;

            if (panel == null)
            {
                return;
            }

            if (panel.ZoomedCell != null)
            {
                panel.ZoomOutCell(() =>
                {
                    foreach (var item in cells)
                    {
                        DeleteCellElements(item);
                    }
                });
            }
            else
            {
                foreach (var item in cells)
                {
                    DeleteCellElements(item);
                }
            }
        }

        ////Stage가 Play Mode로 변경되거나 해제됨
        //public void ChangePlayMode(bool isStagePlayMode)
        //{
        //    //Play Mode로 변경된 경우 빨간색 테두리를 Title이 선택된 Camera의 영역으로 변경해줌 !!
        //    if (isStagePlayMode)
        //    {
        //        //1.Title이 선택된 Camera가 있는 경우 Cell 선택
        //        //2.Title이 선택된 Camera가 없는 경우 Camera가 존재하는 첫번째 Cell 선택
        //        //3.Title이 선택된 Camera가 없고 Camera가 존재하는 Cell도 없는 경우 첫번째 빈 Cell 선택

        //        Cell firstCell = null;
        //        Cell firstCameraCell = null;
        //        Cell titleSelectedCell = null;

        //        foreach (Cell cell in this.multiGrid.GetSelectedCells())
        //        {
        //            //선택된 영역에 Camera가 없는 경우 첫번째 Cell을 보여주기 위해 저장해둠 !!
        //            if (firstCell == null)
        //                firstCell = cell;

        //            //foreach (CameraControlPlayback cameraControl in cell.GetAllCameraControl())
        //            //{
        //            //    if (cameraControl == null)
        //            //        continue;

        //            //    if (firstCameraCell == null)
        //            //        firstCameraCell = cell;

        //            //    if (cameraControl.IsTitleSelected)
        //            //    {
        //            //        titleSelectedCell = cell;
        //            //        break;
        //            //    }
        //            //}

        //            if (titleSelectedCell != null)
        //                break;
        //        }

        //        if (titleSelectedCell != null)
        //            this.multiGrid.SelectCell(titleSelectedCell);
        //        else if (firstCameraCell != null)
        //            this.multiGrid.SelectCell(firstCameraCell);
        //        else if (firstCell != null)
        //            this.multiGrid.SelectCell(firstCell);
        //        else
        //            this.multiGrid.SelectionClear();
        //    }

        //    this.DrawSelectionRect();
        //}

        private void DrawSelectionRect()
        {
            if (this._multiViewPanel == null)
                return;

            //if (multiGridControlInitInfo.InitColumn == 1 && multiGridControlInitInfo.InitRow == 1)
            //    return;

            try
            {
                if (this._multiViewPanel.IsCellSelected())
                {
                    this.SelectionVisibility = Visibility.Visible;

                    if (this._multiViewPanel.ZoomedCell != null)
                    {
                        // Zoom된 Cell이 있을 경우 ControlMode의 Rect를 사용
                        MultiViewCell zoomCell = this._multiViewPanel.ZoomedCell;
                        Rect rect = this._multiViewPanel.GetActualMaxRect(zoomCell);

                        this.SelectionMargin = new Thickness
                        {
                            Left = rect.X,
                            Top = rect.Y,
                            Right = this.ActualWidth - rect.Right,
                            Bottom = this.ActualHeight - rect.Bottom
                        };
                    }
                    else if (!this.SelectionArea.IsEmpty)
                    {
                        // Zoom된 Cell이 없고, SelectionArea가 유효할 때만 설정
                        this.SelectionMargin = new Thickness
                        {
                            Left = this.SelectionArea.Left,
                            Top = this.SelectionArea.Top,
                            Right = this.ActualWidth - this.SelectionArea.Right,
                            Bottom = this.ActualHeight - this.SelectionArea.Bottom
                        };
                    }
                    else
                    {
                        this.SelectionMargin = new Thickness();
                    }
                }
                else
                {
                    this.SelectionVisibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                // 예외 처리 시 로깅 또는 필요한 예외 처리 로직 추가
                Debug.WriteLine($"Error in DrawSelectionRect: {ex.Message}");
            }
            //try
            //{
            //    //by blackRoot : Cell이 FullScreen일 경우 제어가 가능하도록 기능이 수정됨 !!
            //    //따라서, this.multiGrid.ZoomedCell == null 조건을 삭제함 !! this.multiGrid.ZoomedCell이 null이 아닌경우에도 Cell 선택이 되어야 함 !!
            //    //if (CommonsConfig.Instance.IsSelectionVisibility && this.multiGrid.IsCellSelected() &&
            //    //    this.multiGrid.ZoomedCell == null && !this.IsControlMode && !this.IsLocked)
            //    if (true && this.multiGrid.IsCellSelected()) // if (constset.IsSelectionVisibility && this.multiGrid.IsCellSelected())
            //    {
            //        ////Stage가 Play Mode인 경우 빨간색 테두리를 보여주지 않음 !!
            //        //if (true)  //if (StatusManager.Instance.IsStagePlayMode)
            //        //{
            //        //    this.selectionRectangle.Stroke = Brushes.White;
            //        //    this.selectionRectangle.StrokeThickness = 1;
            //        //}
            //        //else
            //        //{
            //        //    this.selectionRectangle.Stroke = Brushes.Red;
            //        //    this.selectionRectangle.StrokeThickness = 2;
            //        //}

            //        this.SelectionVisibility = Visibility.Visible;

            //        //Zoom된 Cell이 있을 경우 ControlMode의 Rect를 사용함 (녹색테두리와 같아짐) ==> DrawControlModeRect()와 같은 수식을 적용함 !!
            //        if (this.multiGrid.ZoomedCell != null)
            //        {
            //            Cell zoomCell = this.multiGrid.ZoomedCell;
            //            Rect rect = this.multiGrid.GetActualMaxRect(zoomCell);

            //            this.SelectionMargin = new Thickness
            //            {
            //                Left = rect.X,
            //                Top = rect.Y,
            //                Right = this.ActualWidth - rect.Right,
            //                Bottom = this.ActualHeight - rect.Bottom
            //            };
            //        }
            //        //Zoom된 Cell이 없는 경우 원래 소스를 사용함 !!
            //        else
            //        {
            //            if (this.SelectionArea.IsEmpty)
            //            {
            //                this.SelectionMargin = new Thickness();
            //            }
            //            else
            //            {
            //                this.SelectionMargin = new Thickness
            //                {
            //                    Left = this.SelectionArea.Left,
            //                    Top = this.SelectionArea.Top,
            //                    Right = this.ActualWidth - this.SelectionArea.Right,
            //                    Bottom = this.ActualHeight - this.SelectionArea.Bottom
            //                };
            //            }
            //        }
            //    }
            //    else
            //    {
            //        this.SelectionVisibility = Visibility.Hidden;
            //    }
            //}
            //catch
            //{
            //}
        }

        //빨간색 테두리와 노란색 테두리가 몇 Pixel 차이가 나는지 설정        
        public void DrawHighlightRect()
        {
            var panel = _multiViewPanel;

            if (_highlightCell == null || panel == null || panel.ZoomedCell != null)
            {
                HighlightVisibility = Visibility.Hidden;
                return;
            }

            HighlightVisibility = Visibility.Visible;

            HighlightBorderStyle = panel.IsSelectionEnabled || !Keyboard.IsKeyDown(Key.LeftShift)
                ? BorderStyle.SelectionEnabled
                : BorderStyle.SelectionDisabled;

            Rect loc = panel.GetActualRect(_highlightCell);
            int gap = _edgeThickness;

            HighlightMargin = new Thickness(
                loc.Left + gap,
                loc.Top + gap,
                ActualWidth - loc.Right + gap,
                ActualHeight - loc.Bottom + gap
            );

            //if (this._highlightCell != null && this.multiGrid.ZoomedCell == null && true)  //if (this._highlightCell != null && this.multiGrid.ZoomedCell == null && constset.IsHighlightVisibility)
            //{
            //    //Stage가 Play Mode인 경우 노란색 테두리를 보여주지 않음 !!
            //    //if (StatusManager.Instance.IsStagePlayMode || isPreviewMode == true)
            //    //{
            //    //    this.HighlightVisibility = Visibility.Hidden;
            //    //    return;
            //    //}

            //    this.HighlightVisibility = Visibility.Visible;

            //    if (this.multiGrid.IsSelectionEnabled)
            //    {
            //        this.HighlightBorderStyle = BorderStyle.SelectionEnabled;
            //    }
            //    else
            //    {
            //        if (Keyboard.IsKeyDown(Key.LeftShift))
            //        {
            //            this.HighlightBorderStyle = BorderStyle.SelectionDisabled;
            //        }
            //        else
            //        {
            //            this.HighlightBorderStyle = BorderStyle.SelectionEnabled;
            //        }
            //    }


            //    Rect loc = this.multiGrid.GetActualRect(this._highlightCell);
            //    //const int GAP = 5;
            //    int GAP = this._edgeThickness;
            //    this.HighlightMargin = new Thickness
            //    {
            //        Left = loc.Left + GAP,
            //        Top = loc.Top + GAP,
            //        Right = this.ActualWidth - loc.Right + GAP,
            //        Bottom = this.ActualHeight - loc.Bottom + GAP
            //    };
            //}
            //else
            //{
            //    this.HighlightVisibility = Visibility.Hidden;
            //}
        }

        //private void DrawZoomEndButton()
        //{
        //    if (this.IsZoomed)
        //    {
        //        this.ZoomEndButtonVisibility = Visibility.Visible;

        //        Rect rect = this.multiGrid.GetActualMaxRect(this.multiGrid.ZoomedCell);

        //        //VerticalAlignment를 Bottom으로 했을 경우 Bottom값을 다시 지정함 !!
        //        this.ZoomEndButtonMargin = new Thickness
        //        {
        //            //Bottom = this.ActualHeight - rect.Bottom + 5,
        //            Top = rect.Top + 30,
        //            Right = this.ActualWidth - rect.Right + 10
        //        };

        //        //VerticalAlignment를 Top으로 했을 경우 Top값을 다시 지정함 !!
        //        //this.ZoomEndButtonMargin = new Thickness
        //        //    {
        //        //        Top = rect.Top + 5,
        //        //        Right = this.ActualWidth - rect.Right + 5
        //        //    };
        //    }
        //    else
        //    {
        //        this.ZoomEndButtonVisibility = Visibility.Hidden;
        //    }
        //}

        private MultiViewCell? GetSelectedFirstCell()
        {
            var panel = _multiViewPanel;
            if (panel == null)
            {
                return null;
            }

            var selectedCellList = panel.GetSelectedCells();

            if (selectedCellList.Count == 0)
            {
                return null;
            }

            return selectedCellList[0];
        }

        //private void Init_Internal(int rowCount, int columnCount, bool isSelectionAll, FrameworkElement initElement)
        //{
        //    this.Clear();

        //    if (this.multiGrid != null)
        //    {
        //        this.Background = new SolidColorBrush(Colors.Transparent);
        //        //this.multiGrid.isPreviewMode = this.isPreviewMode;

        //        this.multiGrid.Init(rowCount, columnCount);

        //        if (isSelectionAll)
        //        {
        //            this.multiGrid.SelectionAll();
        //        }

        //        if (initElement != null)
        //        {
        //            SetCellElementSilenceMode(this.multiGrid.GetSelectedCells()[0], initElement);
        //        }

        //        if (!this.UseMultiViewSync)
        //        {
        //            return;
        //        }

        //        // Sync Data를 보냄 !!
        //        //var localGridChanged = this.eGridChanged;
        //        //if (localGridChanged != null)
        //        //{
        //        //    localGridChanged(this, new OpnxMultiViewLayoutChangedEventArgs(this.SyncGUID));
        //        //}
        //        this.SendSyncData();

        //        // TODO : RedrawOpnxControl 삭제 작업 (by jhlee)
        //        // this.RedrawOpnxControlAsync();
        //    }
        //}


        //Camera 전체에 대한 동작은 Surface쪽에서 처리함 !!
        //public void Seek(string cameraID, double unixTimeMiliSeconds)
        //{
        //    List<UIElement> selectedElementList = this.GetSelectionElements();
        //    foreach (CameraControlPlayback cameraControl in selectedElementList)
        //    {
        //        if (cameraControl == null)
        //            continue;

        //        if (cameraControl.ID.ToUpper() == cameraID.ToUpper())
        //        {
        //            //Seek는 초단위로 설정함 (1000으로 나눠줌)
        //            cameraControl.Seek(Math.Truncate(unixTimeMiliSeconds / 1000));
        //        }
        //    }
        //}

        //public void SeekAllCamera(double unixTimeMiliSeconds)
        //{
        //    List<UIElement> allElementList = this.GetAllElements();
        //    foreach (CameraControlPlayback cameraControl in allElementList)
        //    {
        //        if (cameraControl == null)
        //            continue;

        //        //Seek는 초단위로 설정함 (1000으로 나눠줌)
        //        cameraControl.Seek(Math.Truncate(unixTimeMiliSeconds / 1000));
        //    }
        //}

        //public void PlayAllCamera(PlayRecordType playRecordType = PlayRecordType.Normal, double speed = 1.0)
        //{
        //    List<UIElement> allElementList = this.GetAllElements();
        //    foreach (CameraControlPlayback cameraControl in allElementList)
        //    {
        //        if (cameraControl == null)
        //            continue;

        //        cameraControl.Play(playRecordType, speed);
        //    }
        //}

        //public void RewindAllCamera(PlayRecordType playRecordType = PlayRecordType.Normal, double speed = 1.0)
        //{
        //    List<UIElement> allElementList = this.GetAllElements();
        //    foreach (CameraControlPlayback cameraControl in allElementList)
        //    {
        //        if (cameraControl == null)
        //            continue;

        //        cameraControl.Rewind(playRecordType, speed);
        //    }
        //}

        //public void FrameSearchAllCamera(FrameSearchType frameSearchType)
        //{
        //    List<UIElement> allElementList = this.GetAllElements();
        //    foreach (CameraControlPlayback cameraControl in allElementList)
        //    {
        //        if (cameraControl == null)
        //            continue;

        //        cameraControl.FrameSearch(frameSearchType);
        //    }
        //}

        //public void PauseAllIndependentCamera()
        //{
        //    //MessageBox.Show("Pause All Camera !!");

        //    List<UIElement> allElementList = this.GetAllElements();
        //    foreach (CameraControlPlayback cameraControl in allElementList)
        //    {
        //        if (cameraControl == null)
        //            continue;

        //        cameraControl.PauseIndependentVideoAudio();
        //    }
        //}

        //public void ResumeAllIndependentCamera()
        //{
        //    //MessageBox.Show("Resume All Camera !!");

        //    List<UIElement> allElementList = this.GetAllElements();
        //    foreach (CameraControlPlayback cameraControl in allElementList)
        //    {
        //        if (cameraControl == null)
        //            continue;

        //        cameraControl.ResumeIndependentVideoAudio();
        //    }
        //}

        //public void StopAllCamera()
        //{
        //    //MessageBox.Show("Stop All Camera !!");

        //    List<UIElement> allElementList = this.GetAllElements();
        //    foreach (CameraControlPlayback cameraControl in allElementList)
        //    {
        //        if (cameraControl == null)
        //            continue;

        //        cameraControl.Stop();
        //    }
        //}

        private void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            var panel = _multiViewPanel;
            if (panel == null)
                return;

            if (panel.RowCount == 1 && panel.ColumnCount == 1)
                return;

            if (sender is not MultiViewCell cell)
                return;

            _highlightCell = cell;
            DrawHighlightRect();
        }

        private void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            this._highlightCell = null;
            this.DrawHighlightRect();
        }

        //private void MultiGrid_CellDropCompleted(object sender, CellDropCompletedArgs e)
        //{
        //    this.CellDropCompleted?.Invoke(this, new CellElementDropCompletedEventArgs(this, e.Source, e.TargetCell));            
        //}

        private void MultiViewPanel_CellAdded(object? sender, CellAddedArgs e)
        {
            foreach (MultiViewCell item in e.AddedCells)
            {
                item.MouseEnter += this.Item_MouseEnter;
                item.MouseLeave += this.Item_MouseLeave;

                //item.eSlideChanged += this.cell_eSlideChanged;
                item.ItemAdded += this.Cell_ItemAdded;
                item.ItemRemoved += this.Cell_ItemRemoved;
                item.ViewAreaUpdated += this.Cell_ViewAreaUpdated;
                //item.eVideoElementCreated += this.item_eVideoElementCreated;                
                item.DropCompleted += Item_DropCompleted;
            }

            DrawHighlightRect();

            // TODO : RedrawOpnxControl 삭제 작업 (by jhlee)
            // this.RedrawOpnxControlAsync();
        }

        private void MultiViewPanel_CellRemoved(object? sender, CellRemovedArgs e)
        {
            foreach (MultiViewCell item in e.RemovedCells)
            {
                item.MouseEnter -= this.Item_MouseEnter;
                item.MouseLeave -= this.Item_MouseLeave;

                //item.eSlideChanged -= this.cell_eSlideChanged;
                item.ItemAdded -= this.Cell_ItemAdded;
                item.ItemRemoved -= this.Cell_ItemRemoved;
                item.ViewAreaUpdated -= this.Cell_ViewAreaUpdated;
                //item.eVideoElementCreated -= this.item_eVideoElementCreated;
                item.DropCompleted -= Item_DropCompleted;
            }

            ////Cell을 지운 경우 CameraController의 UI를 안 보이게 함
            //PTZManager.GetInstance().ShowCameraControllerUI(null, string.Empty, false);

            // TODO : RedrawOpnxControl 삭제 작업 (by jhlee)
            // this.RedrawOpnxControlAsync();
        }



        //private void MultiGrid_eControledCellChanged(object sender, ControlledCellChangedEventArgs e)
        //{
        //    this.DrawHighlightRect();
        //    this.DrawSelectionRect();
        //    //this.DrawControlModeRect();
        //}

        private void MultiViewPanel_SelectionChanged(object? sender, SelectionChangedArgs e)
        {
            //if (multiGridControlInitInfo.InitColumn == 1 && multiGridControlInitInfo.InitRow == 1)
            //    return;

            if (this.IsReceivedEventForZoom || sender is not MultiViewPanel mg)
                return; // Zoom 이벤트 수신 또는 sender가 MultiViewPanel이 아닌 경우 조기 반환


            this.DrawHighlightRect();
            this.DrawSelectionRect();
            //this.DrawControlModeRect();

            var selectedCells = e.SelectedCells;

            int selectedCellCount = selectedCells?.Count ?? 0;

            var firstCellChildren = selectedCells is { Count: > 0 } cells
                ? cells[0].GetAllElements()
                : [];

            var (rowCount, columnCount) = mg.GetSelectionGridCount();

            CellSelectionChanged?.Invoke(
                sender,
                new OpnxMultiViewSelectionChangedEventArgs(
                    selectedCellCount,
                    firstCellChildren,
                    rowCount,
                    columnCount));

            //var localCellSelectionChanged = this.eCellSelectionChanged;
            //if (localCellSelectionChanged != null)
            //{
            //    var (rowCount, columnCount) = mg.GetSelectionGridCount();
            //    //bool isSlidePlaying = mg.IsSelectionSlidePlaying();
            //    //if (isSlidePlaying)
            //    //{
            //    //    foreach (var element in firstCellChildren)
            //    //    {
            //    //        (element as CameraControl).IsEnableToggleButtonShowControlUI = false;
            //    //    }
            //    //}

            //    //int slideInterval = mg.GetSelectionSlideInterval();

            //    //bool isSlidePlaying = false;
            //    //int slideInterval = -1;

            //    localCellSelectionChanged(
            //        sender,
            //        new OpnxMultiViewSelectionChangedEventArgs(
            //            selectedCellCount, firstCellChildren, rowCount, columnCount));
            //}
        }

        private void MultiViewPanel_CellClicked(object? sender, EventArgs e)
        {
            this.CellClicked?.Invoke(this, new EventArgs());
        }

        private void MultiViewPanel_ZoomedCellChanged(object? sender, ZoomedCellChangeEventArgs e)
        {
            this.DrawHighlightRect();
            this.DrawSelectionRect();
            //this.DrawControlModeRect();
            //this.DrawZoomEndButton();

            //Full Screen Animation 종료후 Camera Update를 해줌 !!
            //this.UpdateViewArea(Rect.Empty, null);

            // 방어 처리.
            this.IsReceivedEventForZoom = false;

            // TODO : RedrawOpnxControl 삭제 작업 (by jhlee)
            // this.RedrawOpnxControlAsync();

            //줌 인/아웃 시에 이벤트 날려줌 (PrintList 항목 보이거나 안 보이도록 수정)

            this.RaiseLayoutChanged();
        }

        private void MultiViewPanel_ZoomedCellChanging(object? sender, ZoomedCellChangeEventArgs e)
        {
            if (sender is not MultiViewPanel mg)
                return;

            //var localCellFullScreenChanged = this.eCellFullScreenChanged;
            //if (localCellFullScreenChanged != null)
            //{
            //    // Sync로 받은 명령데 대하여 다시 Sync가 호출되는 상황을 방지하기 위하여 플래그를 사용하여 처리함.
            //    var eventArgs = new FullScreenChangedEventArgs { MultiViewSyncGuid = this.SyncGUID, UseSync = !this.IsReceivedEventForZoom };
            //    if (this.IsReceivedEventForZoom)
            //    {
            //        this.multiGrid.SelectionClear();
            //    }

            //    this.IsReceivedEventForZoom = false;

            //    if (e.Cell != null)
            //    {
            //        eventArgs.CellSyncGuid = mg.ZoomedCell.SyncGuid;
            //        eventArgs.IsZoomed = true;
            //    }
            //    else
            //    {
            //        eventArgs.CellSyncGuid = Guid.Empty;
            //        eventArgs.IsZoomed = false;
            //    }

            //    localCellFullScreenChanged(this, eventArgs);
            //}

            CellFullScreenChanged?.Invoke(
                this,
                new FullScreenChangedEventArgs
                {
                    MultiViewSyncId = this.SyncId,
                    UseSync = !this.IsReceivedEventForZoom,
                    CellSyncId = e.Cell?.SyncId ?? Guid.Empty,  // e.Cell이 null이면 Guid.Empty 사용
                    IsZoomed = e.Cell != null  // e.Cell이 null이 아니면 IsZoomed를 true로 설정
                });

            if (this.IsReceivedEventForZoom)
            {
                this._multiViewPanel?.SelectionClear();
            }

            this.IsReceivedEventForZoom = false;

            this.HighlightVisibility = Visibility.Hidden;
            this.SelectionVisibility = Visibility.Hidden;
            //this.ControlModeVisibility = Visibility.Hidden;
            //this.ZoomEndButtonVisibility = Visibility.Hidden;
        }


        private void MultiViewPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //this.DrawZoomEndButton();            
            //this.DrawControlModeRect();
        }

        private void RaiseLayoutChanged()
        {
            var syncId = this.SyncId;
            this.LayoutChanged?.Invoke(this, new OpnxMultiViewLayoutChangedEventArgs(syncId));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051")]
        private void OnEFavoriteLoaded()
        {
            FavoriteLoaded?.Invoke(this, new EventArgs());
            //var handler = this.FavoriteLoaded;
            //if (handler != null)
            //{
            //    handler(this, new EventArgs());
            //}
        }

        //private void ZoomEndButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (this.multiGrid != null)
        //    {
        //        this.multiGrid.ZoomedCell = null;
        //    }
        //}
        #endregion

        /// <summary>
        /// MultiGridControlInitInfo class.
        /// </summary>
        //private class MultiGridControlInitInfo
        //{
        //    #region Constructors and Destructors

        //    /// <summary>
        //    /// Initializes a new instance of the <see cref="MultiGridControlInitInfo"/> class.
        //    /// </summary>
        //    /// <param name="initRow">
        //    /// The init row.
        //    /// </param>
        //    /// <param name="initColumn">
        //    /// The init column.
        //    /// </param>
        //    /// <param name="isSelectionAll">
        //    /// The is selection all.
        //    /// </param>
        //    internal MultiGridControlInitInfo(int initRow, int initColumn, bool isSelectionAll, FrameworkElement initElement)
        //    {
        //        this.InitRow = initRow;
        //        this.InitColumn = initColumn;
        //        this.InitIsSelectionAll = isSelectionAll;
        //        this.InitElement = initElement;
        //    }

        //    #endregion

        //    #region Properties

        //    internal int InitColumn { get; private set; }

        //    internal bool InitIsSelectionAll { get; private set; }

        //    internal int InitRow { get; private set; }

        //    internal FrameworkElement InitElement { get; private set; }

        //    #endregion
        //}

        #region Sync

        /// <summary>
        /// Gets or sets a value indicating whether UseMultiViewSync.
        /// </summary>
        public bool UseMultiViewSync { get; set; }

        #endregion //Sync
    }
}









