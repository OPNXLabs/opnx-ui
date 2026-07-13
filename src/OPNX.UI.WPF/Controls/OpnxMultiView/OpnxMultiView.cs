using OPNX.UI.WPF.Interactivity.DragDrop;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    [TemplatePart(Name = "xMultiViewPanel", Type = typeof(MultiViewPanel))]
    [TemplatePart(Name = "xZoomControl", Type = typeof(Canvas))]
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

        private MultiViewCell? _highlightCell;

        private MultiViewPanel? _multiViewPanel;

        private Canvas? _zoomControl;

        private MultiViewInitInfo? _pendingInitInfo;

        private readonly int _edgeThickness = 1;
        #endregion

        #region Constructors and Destructors

        static OpnxMultiView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpnxMultiView), new FrameworkPropertyMetadata(typeof(OpnxMultiView)));
        }

        public OpnxMultiView()
        {
            this.Background = Brushes.Black;

        }

        #endregion

        #region Events

        public event EventHandler<CellElementDropCompletedEventArgs>? CellDropCompleted;

        public event EventHandler<CellElementChangedEventArgs>? CellElementAdded;

        public event EventHandler<CellElementChangedEventArgs>? CellElementRemoved;

        public event EventHandler<FullScreenChangedEventArgs>? CellFullScreenChanged;

        public event EventHandler<OpnxMultiViewSelectionChangedEventArgs>? CellSelectionChanged;

        public event EventHandler<EventArgs>? CellClicked;

        public event EventHandler<OpnxMultiViewLayoutChangedEventArgs>? LayoutChanged;

        #endregion

        #region Enums

        internal enum BorderStyle
        {
            None,

            SelectionEnabled,

            SelectionDisabled,
        }

        #endregion

        #region Properties
        public bool IsEnabledDragDrop { get; set; } = true;
        public bool IsCellSelected
        {
            get => this._multiViewPanel?.IsCellSelected() ?? false;
        }

        public bool IsSelectionAlignable
        {
            get => this._multiViewPanel?.IsSelectionAlignable() ?? false;
        }

        public bool IsZoomed => this._multiViewPanel?.ZoomedCell is not null;


        public MultiViewCell? ZoomedCell
        {
            get => this._multiViewPanel?.ZoomedCell;
            set => this._multiViewPanel?.ZoomedCell = value;
        }

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

        public bool PlayOnControlMode { get; set; }

        // Prevents a remotely received zoom change from being broadcast back to synchronization peers.
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

        public int GetSelectedRowCount()
        {
            return this._multiViewPanel?.GetSelectionRowCount() ?? 0;
        }

        public int GetSelectedColumnCount()
        {
            return this._multiViewPanel?.GetSelectionColumnCount() ?? 0;
        }

        public void ChangeFullScreen(Guid cellSyncId)
        {
            if (this._multiViewPanel == null)
            {
                return;
            }

            this.IsReceivedEventForZoom = true;

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

            this._multiViewPanel.ZoomedCell = this._multiViewPanel.GetAllCells().FirstOrDefault(cell => cell.SyncId == cellSyncId);

        }

        public void Clear()
        {
            ClearAllCellElement();

            this._multiViewPanel?.Clear();
        }

        public void ClearAllCellElement()
        {
            if (this._multiViewPanel == null)
                return;

            this.DeleteCellElements(this._multiViewPanel.GetAllCells());

            this.SendSyncData();
        }

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

        public void SendSyncData()
        {
            this.RaiseLayoutChanged();
        }

        public MultiViewCell? AddRowAndResizeCells()
        {
            return this._multiViewPanel?.AddRowAndResizeCells();
        }

        public List<UIElement> GetCellElementsByGuid(Guid guid)
        {
            return this._multiViewPanel?.GetAllCells().FirstOrDefault(cell => cell.SyncId == guid)?.GetAllElements() ?? [];
        }

        public static List<UIElement> GetCellElements(MultiViewCell cell)
        {
            return cell?.GetAllElements() ?? [];
        }

        public List<MultiViewCell>? GetCellsWithinSelectionArea()
        {
            return this._multiViewPanel?.GetCellsWithinSelectionArea();
        }

        public List<MultiViewCell>? GetCellsWithoutSelectionArea()
        {
            return this._multiViewPanel?.GetCellsWithoutSelectionArea();
        }

        public MultiViewCell? GetCell(UIElement uiElement)
        {
            return this._multiViewPanel?.GetCell(uiElement);
        }


        public MultiViewCell? GetNextEmptyCell()
        {
            return this._multiViewPanel?.GetNextEmptyCell();
        }

        public MultiViewCell? GetNextTopLeftEmptyCell()
        {
            return this._multiViewPanel?.GetNextTopLeftEmptyCell();
        }

        public List<MultiViewCell>? GetAllCells()
        {
            return this._multiViewPanel?.GetAllCells();
        }

        public int GetSelectionCellCount()
        {
            return this._multiViewPanel?.GetSelectedCells().Count ?? 0;

        }

        public List<MultiViewCell>? GetSelectionCell()
        {
            return this._multiViewPanel?.GetSelectedCells() ?? null;

        }

        public List<UIElement> GetSelectionElements()
        {
            return this._multiViewPanel?.GetSelectedCells().SelectMany(cell => cell.GetAllElements()).ToList() ?? [];
        }

        public List<UIElement> GetAllElements()
        {
            var result = this._multiViewPanel?.GetAllCells().SelectMany(cell => cell.GetAllElements()).ToList() ?? [];
            var zoomElements = GetZoomedElements();
            if (zoomElements.Count > 0)
                result.AddRange(zoomElements);

            return result;
        }

        public List<UIElement> GetZoomedElements()
        {
            return this._multiViewPanel?.ZoomCanvas?.Children.Cast<UIElement>().ToList() ?? [];
        }

        public void Init(int rowCount, int columnCount, bool isSelectionAll, FrameworkElement? initElement)
        {
            if (this._multiViewPanel == null)
            {
                _pendingInitInfo = new MultiViewInitInfo(rowCount, columnCount, isSelectionAll, initElement);
                return;
            }

            _pendingInitInfo = null;
            InitCore(rowCount, columnCount, isSelectionAll, initElement);
        }


        public override void OnApplyTemplate()
        {
            if (this._multiViewPanel != null)
            {
                this._multiViewPanel.CellAdded -= this.MultiViewPanel_CellAdded;
                this._multiViewPanel.CellRemoved -= this.MultiViewPanel_CellRemoved;
                this._multiViewPanel.SelectionChanged -= this.MultiViewPanel_SelectionChanged;
                this._multiViewPanel.ZoomedCellChanging -= this.MultiViewPanel_ZoomedCellChanging;
                this._multiViewPanel.ZoomedCellChanged -= this.MultiViewPanel_ZoomedCellChanged;
            }

            base.OnApplyTemplate();

            this._multiViewPanel = this.Template.FindName("xMultiViewPanel", this) as MultiViewPanel;
            if (this._multiViewPanel != null)
            {
                this._multiViewPanel.MultiView = this;

                this._multiViewPanel.CellAdded += this.MultiViewPanel_CellAdded;
                this._multiViewPanel.CellRemoved += this.MultiViewPanel_CellRemoved;
                this._multiViewPanel.SelectionChanged += this.MultiViewPanel_SelectionChanged;
                this._multiViewPanel.ZoomedCellChanging += this.MultiViewPanel_ZoomedCellChanging;
                this._multiViewPanel.ZoomedCellChanged += this.MultiViewPanel_ZoomedCellChanged;

                if (_pendingInitInfo is not null)
                {
                    // Init may run before the control template creates the panel; defer it until the panel is available.
                    var pendingInitInfo = _pendingInitInfo;
                    _pendingInitInfo = null;

                    InitCore(
                        pendingInitInfo.RowCount,
                        pendingInitInfo.ColumnCount,
                        pendingInitInfo.IsSelectionAll,
                        pendingInitInfo.InitElement);
                }
            }
            this._zoomControl = this.Template.FindName("xZoomControl", this) as Canvas;
            if (this._zoomControl != null)
            {
                Panel.SetZIndex(this._zoomControl, -1);

                this._zoomControl.Children.Clear();

                this._zoomControl.Visibility = Visibility.Collapsed;
                this._zoomControl.IsHitTestVisible = false;
                this._zoomControl.Background = Brushes.Transparent;
                this._zoomControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                this._zoomControl.VerticalAlignment = VerticalAlignment.Stretch;
            }

            this.SelectionVisibility = Visibility.Collapsed;
        }

        private void InitCore(int rowCount, int columnCount, bool isSelectionAll, FrameworkElement? initElement)
        {
            if (this._multiViewPanel == null)
                return;

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


        public void RemoveCellElements(IList<UIElement> elements)
        {
            if (this._multiViewPanel == null || !this._multiViewPanel.IsCellSelected() || elements == null || elements.Count <= 0)
            {
                return;
            }

            var selectedCellList = this._multiViewPanel.GetSelectedCells();
            if (selectedCellList.Count <= 0) return;

            foreach (UIElement element in elements)
            {
                foreach (MultiViewCell selectedCell in selectedCellList)
                {
                    selectedCell.Remove(element);
                }
            }

            var existElementList = selectedCellList.SelectMany(cell => cell.GetAllElements()).ToList();

            foreach (var selectedCell in selectedCellList)
            {
                selectedCell.Clear();
            }

            if (existElementList.Count > 0)
            {
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

        public void SelectionAll()
        {
            this._multiViewPanel?.SelectionAll();
        }

        public void SelectionClear()
        {
            this._multiViewPanel?.SelectionClear();

        }

        public MultiViewCell? SelectionMerge()
        {
            if (this._multiViewPanel == null)
            {
                return null;
            }

            if (this._multiViewPanel.ZoomedCell != null)
                this._multiViewPanel.ZoomedCell = null;

            this.DeleteCellElements(this._multiViewPanel.GetSelectedCells());

            var result = this._multiViewPanel.SelectionMerge();


            this.SendSyncData();

            return result;

        }

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
        }

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
        public void SetCellElements(IList<UIElement> elementList)
        {

            if (this._multiViewPanel == null || elementList.Count < 1)
            {
                return;
            }

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

        public void SetCellElementSilenceMode(MultiViewCell cell, UIElement element)
        {
            if (this._multiViewPanel == null || cell == null || element == null)
            {
                return;
            }

            OpnxMultiView.DeleteCellElements(cell);

            cell.Clear();
            cell.Add(element);
}

        #endregion

        #region Private / Protected Methods

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

                    this.DeleteCellElements(this._multiViewPanel.GetAllCells());

                    this._multiViewPanel.Clear();
                    this._multiViewPanel = null;
                }
            }

            base.DoDispose(isManage);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            this.DrawHighlightRect();
            this.DrawSelectionRect();
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










        private void DrawSelectionRect()
        {
            if (this._multiViewPanel == null)
                return;


            try
            {
                if (this._multiViewPanel.IsCellSelected())
                {
                    this.SelectionVisibility = Visibility.Visible;

                    if (this._multiViewPanel.ZoomedCell != null)
                    {
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
                Debug.WriteLine($"Error in DrawSelectionRect: {ex.Message}");
            }
        }

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
        }


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

        private void MultiViewPanel_CellAdded(object? sender, CellAddedArgs e)
        {
            foreach (MultiViewCell item in e.AddedCells)
            {
                item.MouseEnter += this.Item_MouseEnter;
                item.MouseLeave += this.Item_MouseLeave;

                item.ItemAdded += this.Cell_ItemAdded;
                item.ItemRemoved += this.Cell_ItemRemoved;
                item.ViewAreaUpdated += this.Cell_ViewAreaUpdated;
                item.DropCompleted += Item_DropCompleted;
            }

            DrawHighlightRect();
        }

        private void MultiViewPanel_CellRemoved(object? sender, CellRemovedArgs e)
        {
            foreach (MultiViewCell item in e.RemovedCells)
            {
                item.MouseEnter -= this.Item_MouseEnter;
                item.MouseLeave -= this.Item_MouseLeave;

                item.ItemAdded -= this.Cell_ItemAdded;
                item.ItemRemoved -= this.Cell_ItemRemoved;
                item.ViewAreaUpdated -= this.Cell_ViewAreaUpdated;
                item.DropCompleted -= Item_DropCompleted;
            }
        }

        private void MultiViewPanel_SelectionChanged(object? sender, SelectionChangedArgs e)
        {
            if (this.IsReceivedEventForZoom || sender is not MultiViewPanel mg)
                return;

            this.DrawHighlightRect();
            this.DrawSelectionRect();

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
        }

        private void MultiViewPanel_CellClicked(object? sender, EventArgs e)
        {
            this.CellClicked?.Invoke(this, new EventArgs());
        }

        private void MultiViewPanel_ZoomedCellChanged(object? sender, ZoomedCellChangeEventArgs e)
        {
            this.DrawHighlightRect();
            this.DrawSelectionRect();

            this.IsReceivedEventForZoom = false;

            this.RaiseLayoutChanged();
        }

        private void MultiViewPanel_ZoomedCellChanging(object? sender, ZoomedCellChangeEventArgs e)
        {
            if (sender is not MultiViewPanel mg)
                return;

            CellFullScreenChanged?.Invoke(
                this,
                new FullScreenChangedEventArgs
                {
                    MultiViewSyncId = this.SyncId,
                    // Remote zoom changes are applied locally without echoing the same synchronization message.
                    UseSync = !this.IsReceivedEventForZoom,
                    CellSyncId = e.Cell?.SyncId ?? Guid.Empty,
                    IsZoomed = e.Cell != null
                });

            if (this.IsReceivedEventForZoom)
            {
                this._multiViewPanel?.SelectionClear();
            }

            this.IsReceivedEventForZoom = false;

            this.HighlightVisibility = Visibility.Hidden;
            this.SelectionVisibility = Visibility.Hidden;
        }

        private void RaiseLayoutChanged()
        {
            var syncId = this.SyncId;
            this.LayoutChanged?.Invoke(this, new OpnxMultiViewLayoutChangedEventArgs(syncId));
        }
        #endregion

        private sealed class MultiViewInitInfo(
            int rowCount,
            int columnCount,
            bool isSelectionAll,
            FrameworkElement? initElement)
        {
            internal int RowCount { get; } = rowCount;

            internal int ColumnCount { get; } = columnCount;

            internal bool IsSelectionAll { get; } = isSelectionAll;

            internal FrameworkElement? InitElement { get; } = initElement;
        }

        #region Sync

        public bool UseMultiViewSync { get; set; }

        #endregion //Sync
    }
}