using OPNX.UI.WPF.Interactivity.DragDrop;
using OPNX.UI.WPF.Utilities;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls.OpnxMultiView
{
    public class MultiViewCell : ContentControl, IDisposable
    {
        #region Constants and Fields
        private Border? _border = new();

        private Grid? _grid = new();

        /// <summary>
        /// 셀 보더 색상
        /// </summary>
        private readonly Brush? _cellBorderBrush;

        /// <summary>
        /// 셀 보더 두께.
        /// </summary>
        private readonly Thickness _cellBorderThickness;

        private readonly Thickness _cellBorderThicknessForHidden;

        /// <summary> 
        /// 셀 보더 배경.
        /// </summary>
        private readonly Brush _cellBorderBackground;

        public bool IsSelectedCell { get; set; }

        //private System.Timers.Timer synchronousAssignTimeoutTimer;

        //private bool useGridGuidLines = false;
        //private string useGridGuidLineColor = string.Empty;
        //private int useGridGuidLineThickness = 0;
        //private string cellBackground = string.Empty;      
        private readonly DropTargetAdvisor _targetDropAdvisor = new();
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="Cell"/> class.
        /// </summary>
        static MultiViewCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiViewCell), new FrameworkPropertyMetadata(typeof(MultiViewCell)));
        }

        internal MultiViewCell()
        {
            this._cellBorderBrush = new BrushConverter().ConvertFromString("#202228") as SolidColorBrush;
            this._cellBorderThickness = new Thickness(1);
            this._cellBorderThicknessForHidden = new Thickness(0);
            this._cellBorderBackground = Brushes.Black;

            this.ClipToBounds = true;

            this._border.Child = this._grid;

            this._border.BorderBrush = this._cellBorderBrush;
            this._border.BorderThickness = this._cellBorderThickness;
            this._border.Background = this._cellBorderBackground;

            this.Content = this._border;

            this.SyncId = Guid.NewGuid();

            _targetDropAdvisor.DropCompleted += TargetDropAdvisor_DropCompleted;
            DragDropManager.SetDropTargetAdvisor(this, _targetDropAdvisor);
            //if (CommonsConfig.Instance.UseSynchronousAssign)
            //{
            //    this.synchronousAssignTimeoutTimer = new System.Timers.Timer(CommonsConfig.Instance.SynchronousAssignTimeout) { AutoReset = false };
            //    this.synchronousAssignTimeoutTimer.Elapsed += this.synchronousAssignTimeoutTimer_Elapsed;
            //}
        }

        #endregion

        #region Events

        internal event EventHandler<DropTargetAdvisorDropCompletedEventArgs>? DropCompleted;
        public void OnDropCompleted(DropTargetAdvisorDropCompletedEventArgs e)
        {
            DropCompleted?.Invoke(this, e);
        }

        internal event EventHandler<CellElementChangedEventArgs>? ItemAdded;

        public void OnItemAdded(CellElementChangedEventArgs e)
        {
            ItemAdded?.Invoke(this, e);
        }

        internal event EventHandler<CellElementChangedEventArgs>? ItemRemoved;

        public void OnItemRemoved(CellElementChangedEventArgs e)
        {
            ItemRemoved?.Invoke(this, e);
        }

        //internal event EventHandler<SlideChangedEventArgs> eSlideChanged;

        //public void OnESlideChanged(SlideChangedEventArgs e)
        //{
        //    var handler = eSlideChanged;
        //    if (handler != null) handler(this, e);
        //}

        internal event EventHandler<EventArgs>? ViewAreaUpdated;

        public void OnViewAreaUpdated(EventArgs e)
        {
            ViewAreaUpdated?.Invoke(this, e);
        }

        internal event EventHandler<EventArgs>? SelectedCell;
        public void OnSelectedCell(EventArgs e)
        {
            SelectedCell?.Invoke(this, e);
        }

        /// <summary>
        /// VideoElement의 SetDisplayRect 호출 후 첫 프레임 도착 알림 이벤트
        /// </summary>
        public event EventHandler? PreparedRendering;

        public void OnPreparedRendering(EventArgs e)
        {
            PreparedRendering?.Invoke(this, e);
        }

        #endregion

        #region Properties
        public Grid? InnerGrid => _grid;
        /// <summary>
        /// 그리드 분할선 보이기 여부 속성.
        /// </summary>
        public bool UseGridGuidLines
        {
            get { return (bool)this.GetValue(UseGridGuidLinesProperty); }
            set { this.SetValue(UseGridGuidLinesProperty, value); }
        }

        /// <summary>
        /// 그리드 분할선 색 속성.
        /// </summary>
        public string UseGridGuidLineColor
        {
            get { return (string)this.GetValue(UseGridGuidLineColorProperty); }
            set { this.SetValue(UseGridGuidLineColorProperty, value); }
        }


        /// <summary>
        /// 그리드 분할선 두께 속성.
        /// </summary>
        public int UseGridGuidLineThickness
        {
            get { return (int)this.GetValue(UseGridGuidLineThicknessProperty); }
            set { this.SetValue(UseGridGuidLineThicknessProperty, value); }
        }

        /// <summary>
        /// 배경색 속성.
        /// </summary>
        public string CellBackground
        {
            get { return (string)this.GetValue(CellBackgroundProperty); }
            set { this.SetValue(CellBackgroundProperty, value); }
        }

        public bool HasSameTargetForDragAndDrop
        {
            get;
            set;
        }

        //public bool IsIncludeLayout
        //{
        //    get
        //    {
        //        if (this._grid == null) return false;

        //        foreach (var control in this._grid.Children)
        //        {
        //            if (control is LayoutControl) return true;
        //        }

        //        return false;
        //    }
        //}

        public bool IsZoomed
        {
            get { return (bool)this.GetValue(IsZoomedProperty); }
            set { this.SetValue(IsZoomedProperty, value); }
        }

        //private CameraControl CreateCameraControlFromInfo(CameraInformation info, Visibility visibility = Visibility.Visible)
        //{
        //    if (info.cameraGuid == Guid.Empty)
        //    {
        //        info.cameraGuid = Guid.NewGuid();
        //    }

        //    if (info.cameraGuid == Guid.Empty) info.cameraGuid = Guid.NewGuid();
        //    if (info.screenWidth == 0 || info.screenHeight == 0)
        //    {
        //        if (CommonsConfig.Instance.DirectX == 9)
        //        {
        //            return new CameraControl(info.cameraGuid)
        //            {
        //                ID = info.ID,
        //                PlayOnControlMode = info.playOnControlMode,
        //                Visibility = visibility,
        //                Width = 1,
        //                Height = 1,
        //                IsAdded = info.isAdded,
        //                IsInStage = info.isInStage,
        //                SyncGUID = info.SyncGuid,
        //                IsAudioDisabledForGrid = info.isAudioDisabled,
        //                IsZoomInCell = this.IsZoomed
        //            };
        //        }
        //        else
        //        {
        //            return new CameraControl(info.cameraGuid)
        //            {
        //                ID = info.ID,
        //                PlayOnControlMode = info.playOnControlMode,
        //                Visibility = visibility,
        //                Width = 1,
        //                Height = 1,
        //                IsAdded = info.isAdded,
        //                IsInStage = info.isInStage,
        //                SyncGUID = info.SyncGuid,
        //                IsAudioDisabledForGrid = info.isAudioDisabled,
        //                IsZoomInCell = this.IsZoomed,
        //                MainVideoSurfaceId = info.stageKey
        //            };
        //        }
        //    }
        //    else
        //    {
        //        var rds = new RdsViewerControl(info.cameraGuid)
        //        {
        //            ID = info.ID,
        //            SyncGUID = info.SyncGuid,
        //            ScreenHeight = info.screenHeight,
        //            ScreenWidth = info.screenWidth,
        //            PlayOnControlMode = info.playOnControlMode,
        //            Visibility = visibility,
        //            Width = 1,
        //            Height = 1,
        //            RdsEnabledDisplay = false,
        //            RdsControlEnabled = true,
        //            //RdsControlEnabled = info.rdsControlEnabled,
        //            //RdsEnabledDisplay = info.rdsEnabledDisplay,
        //            IsZoomInCell = this.IsZoomed
        //        };

        //        if (info.rdsControlModeChangedEvent != null)
        //        {
        //            rds.eRDSControlModeChanged += info.rdsControlModeChangedEvent;
        //        }

        //        rds.ToggleControlling(false);
        //        return rds;
        //    }
        //}

        /// <summary>
        /// Gets or sets SlideIndex.
        /// 현재 Visible상태인 Element의 Index이다.
        /// Sync 메시지를 보내준다.
        /// </summary>

        //private int slideIndex = -1;

        //internal int SlideIndex
        //{
        //    get
        //    {
        //        return this.slideIndex;
        //    }

        //    set
        //    {
        //        var newControl = ChangeVisibleSlideCamera(value);

        //        this.OnESlideChanged(new SlideChangedEventArgs(Guid.Empty, this.SyncGuid, newControl, value));
        //    }
        //}

        //private int slideIntervalSeconds = 10;
        //internal int SlideIntervalSeconds
        //{
        //    get
        //    {
        //        return this.slideIntervalSeconds;
        //    }

        //    set
        //    {
        //        //if (value < 10)
        //        //    this.slideIntervalSeconds = 10; //테스트로 인한 디폴트 세팅 해제
        //        //else
        //        this.slideIntervalSeconds = value;
        //    }
        //}

        public Guid SyncId { get; set; }

        public Rect RectForCanvas => MultiViewPanel.GetLocation(this);
        #endregion

        #region Denpendency Properties
        internal static readonly DependencyProperty IsZoomedProperty = DependencyProperty.Register(
           nameof(IsZoomed), typeof(bool), typeof(MultiViewCell), new FrameworkPropertyMetadata(false));

        internal static readonly DependencyProperty UseGridGuidLinesProperty = DependencyProperty.Register(
           nameof(UseGridGuidLines), typeof(bool), typeof(MultiViewCell), new FrameworkPropertyMetadata(false));

        internal static readonly DependencyProperty UseGridGuidLineColorProperty = DependencyProperty.Register(
            nameof(UseGridGuidLineColor), typeof(string), typeof(MultiViewCell), new FrameworkPropertyMetadata("#A9A9A9"));

        internal static readonly DependencyProperty UseGridGuidLineThicknessProperty = DependencyProperty.Register(
            nameof(UseGridGuidLineThickness), typeof(int), typeof(MultiViewCell), new FrameworkPropertyMetadata(10));

        internal static readonly DependencyProperty CellBackgroundProperty = DependencyProperty.Register(
            nameof(CellBackground), typeof(string), typeof(MultiViewCell), new FrameworkPropertyMetadata(string.Empty));

        #endregion

        #region Methods

        private void TargetDropAdvisor_DropCompleted(object? sender, DropTargetAdvisorDropCompletedEventArgs e)
        {
            OnDropCompleted(e);
        }

        public void HideBorder()
        {
            // Dispose 후 호출되는 경우가 있어 예외처리
            if (this._border == null)
            {
                return;
            }

            this._border.BorderThickness = this._cellBorderThicknessForHidden;
        }

        public void ShowBorder()
        {
            // Dispose 후 호출되는 경우가 있어 예외처리
            if (this._border == null)
            {
                return;
            }

            this._border.BorderThickness = this._cellBorderThickness;
        }

        ///// <summary>
        ///// 가지고 있는 모든 레이아웃을 반환한다.
        ///// </summary>
        ///// <returns>
        ///// 레이아웃 목록을 반환한다.
        ///// </returns>
        //public List<LayoutControl> GetAllLayout()
        //{
        //    var result = new List<LayoutControl>();

        //    if (this._grid == null)
        //    {
        //        return result;
        //    }

        //    var childList = new List<UIElement>();
        //    for (int i = 0; i < this._grid.Children.Count; i++)
        //    {
        //        childList.Add(this._grid.Children[i]);
        //    }

        //    for (int i = 0; i < childList.Count; i++)
        //    {
        //        var layoutControl = childList[i] as LayoutControl;
        //        if (layoutControl != null)
        //        {
        //            result.Add(layoutControl);
        //        }
        //    }

        //    return result;
        //}

        //public List<MapControl> GetAllMapControl()
        //{
        //    var result = new List<MapControl>();

        //    if (this._grid == null)
        //    {
        //        return result;
        //    }

        //    var childList = new List<UIElement>();
        //    for (int i = 0; i < this._grid.Children.Count; i++)
        //    {
        //        childList.Add(this._grid.Children[i]);
        //    }

        //    for (int i = 0; i < childList.Count; i++)
        //    {
        //        var layoutControl = childList[i] as LayoutControl;
        //        if (layoutControl != null && layoutControl.Name.Equals("DynamicMapLayout"))
        //        {
        //            if (layoutControl.GetElementsCounts() > 0)
        //            {
        //                var mapControl = layoutControl.GetFirstElement() as MapControl;

        //                if (mapControl != null)
        //                {
        //                    result.Add(mapControl);
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        //internal void UpdateElementSizeBinding(UIElement element)
        internal void UpdateElementSizeBinding()
        {
            //if (element is FrameworkElement)
            //{
            //    (element as FrameworkElement).SetBinding(
            //        WidthProperty,
            //        new Binding(nameof(ActualWidth))
            //        {
            //            Mode = BindingMode.OneWay,
            //            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            //            Source = this._grid
            //        });

            //    (element as FrameworkElement).SetBinding(
            //        HeightProperty,
            //        new Binding(nameof(ActualHeight))
            //        {
            //            Mode = BindingMode.OneWay,
            //            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            //            Source = this._grid
            //        });
            //}

            this.OnViewAreaUpdated(new EventArgs());
        }

        internal static void UnbindElementSize(UIElement element)
        {
            if (element is FrameworkElement)
            {
                BindingOperations.ClearBinding(element, WidthProperty);
                BindingOperations.ClearBinding(element, HeightProperty);
            }
        }

        internal void AddElementWithoutBinding(UIElement element, int index = -1)
        {
            var children = _grid?.Children;
            if (children == null)
                return;

            if (index >= 0 && index <= children.Count)
            {
                children.Insert(index, element);
            }
            else
            {
                children.Add(element);
            }

            OnItemAdded(new CellElementChangedEventArgs(element));
            OnViewAreaUpdated(EventArgs.Empty);
        }

        /// <summary>
        /// 셀에 컨텐츠 엘리먼트를 등록한다.
        /// </summary>
        /// <param name="element">컨텐츠 엘리먼트</param>
        internal void Add(UIElement element)
        {
            AddElement(element);

            this.UpdateElementSizeBinding();

            this.OnItemAdded(new CellElementChangedEventArgs(element));
        }

        internal void AddElement(UIElement element)
        {
            this.UpdateElementSizeBinding();

            this._grid?.Children.Add(element);
        }
        //private void RegisterPTZEnabledButtonEvent(UIElement element)
        //{
        //    if (!(element is RdsViewerControl) && (element is CameraControl))
        //    {
        //        // Stage에 올라가는 CameraControl의 PTZControl은 PTZEnableState.Always
        //        (element as CameraControl).PTZEnabled = PTZEnableState.Always;
        //    }
        //}

        /// <summary>
        /// 셀의 모든 자식 제거
        /// </summary>
        internal void Clear()
        {
            if (_grid == null) return;

            int count = _grid.Children.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                Remove(_grid.Children[i]);
            }
        }

        internal void ClearElements()
        {
            if (_grid == null) return;

            _grid.Children.Clear();
        }

        internal List<UIElement> GetAllElements()
        {
            return this._grid?.Children.Cast<UIElement>().ToList() ?? [];
        }

        internal UIElement? GetElement(int index)
        {
            return this._grid?.Children[index];
        }

        internal int? GetElementCount()
        {
            return this._grid?.Children.Count;
        }

        //public bool HasRdsViewerControl()
        //{
        //    foreach (var element in this._grid.Children)
        //    {
        //        if (element is RdsViewerControls.RdsViewerControl)
        //            return true;
        //    }

        //    foreach (var element in this.cameraList)
        //    {
        //        if (element.screenWidth > 0 && element.screenHeight > 0)
        //            return true;
        //    }

        //    return false;
        //}

        /// <summary>
        /// 현재 화면에 표출되고 있는 Element를 반환한다.
        /// </summary>
        /// <returns>
        /// 현재 화면에 표출되고 있는 Element.
        /// </returns>
        internal UIElement? GetVisibleElement()
        {
            if (this._grid == null) return null;

            return this._grid.Children.Cast<UIElement>().FirstOrDefault(x => x.Visibility == Visibility.Visible);

            //foreach (UIElement element in this._grid.Children)
            //{
            //    if (element.Visibility == Visibility.Visible)
            //    {
            //        return element;
            //    }
            //}

            //return null;
        }

        //public List<CameraControl> GetAllCameraControl()
        //{
        //    var cameraControlList = new List<CameraControl>();
        //    var elementList = this.GetAllElements();

        //    foreach (UIElement element in elementList)
        //    {
        //        if (element is CameraControl)
        //        {
        //            cameraControlList.Add(element as CameraControl);
        //        }
        //    }

        //    return cameraControlList;
        //}

        //public bool IsSlideMode()
        //{
        //    return this.cameraList.Count > 1;
        //}

        //internal bool IsSlidePlaying()
        //{
        //    // Alvin 100908 : 슬라이드 전환 싱크를 맞추기 위해 DispatcherTimer가 아닌 Timer를 사용. 기존 타이머는 주석처리.
        //    //return this.slideDispatcherTimer != null && this.slideDispatcherTimer.IsEnabled;
        //    return this.slideThreadingTimer != null;
        //}

        //internal void NextSlide()
        //{
        //    if (this.cameraList.Count < 2)
        //    {        
        //        return;
        //    }

        //    if (this.SlideIndex < this.cameraList.Count - 1)
        //    {
        //        this.SlideIndex++;
        //    }
        //    else
        //    {
        //        this.SlideIndex = 0;
        //    }
        //}

        //internal void AddCamera(CameraInformation information)
        //{
        //    this.cameraList.Add(information);
        //}

        //public int GetCameraCount()
        //{
        //    return this.cameraList.Count;
        //}

        //public CameraInformation GetCamera(int index)
        //{
        //    return this.cameraList[index];
        //}

        //internal void PlaySingleCamera()
        //{
        //    if (this.cameraList.Count != 1) return;
        //    if (this._grid.Children.Count != 0) return;

        //    this.Add(CreateCameraControlFromInfo(this.cameraList[0]));
        //}

        //internal bool PlaySlide(int interval = 0)
        //{
        //    if (Public.GetProgramType() != ProgramType.iCommand && Public.GetProgramType() != ProgramType.iViewer)
        //    {
        //        return false;
        //    }

        //    if (this.cameraList.Count < 2)
        //    {
        //        if (this._grid.Children.Count == 0)
        //        {
        //            this.slideIndex = -1;
        //            for (int i = 0; i < this.cameraList.Count; i++)
        //            {
        //                if (this.cameraList[i].initiallyVisible)
        //                {
        //                    this.SlideIndex = i;
        //                    break;
        //                }
        //            }
        //        }

        //        return false;
        //    }

        //    if (this.SlideIndex == -1) this.SlideIndex = 0;

        //    if (interval != 0)
        //    {
        //        this.SlideIntervalSeconds = interval;
        //    }

        //    if (this.slideThreadingTimer == null)
        //    {
        //        this.slideThreadingTimer = new Timer(this.ThreadingTimerTick);
        //        this.slideThreadingTimer.Change(TimeSpan.FromSeconds(this.SlideIntervalSeconds), TimeSpan.FromSeconds(this.SlideIntervalSeconds));
        //    }
        //    else
        //    {
        //        this.slideThreadingTimer.Change(TimeSpan.FromSeconds(this.SlideIntervalSeconds), TimeSpan.FromSeconds(this.SlideIntervalSeconds));
        //    }

        //    // Slide시작시에도 현재 화면상에 표출되고 있는 element를 외부에 알려주기 위해 이벤트를 날린다.
        //    this.OnESlideChanged(new SlideChangedEventArgs(Guid.Empty, this.SyncGuid, this._grid.Children[0], this.SlideIndex));

        //    return true;
        //}

        //private void ThreadingTimerTick(object state)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(this.TimerProc));
        //}

        //private void TimerProc()
        //{
        //    if (this.cameraList.Count < 2)
        //    {
        //        this.StopSlide();
        //        return;
        //    }

        //    this.NextSlide();
        //}

        //internal void TimerRestart()
        //{
        //    if (!this.IsSlideMode())
        //    {
        //        return;
        //    }

        //    if (this.slideThreadingTimer != null)
        //    {
        //        this.slideThreadingTimer.Change(TimeSpan.FromSeconds(this.SlideIntervalSeconds), TimeSpan.FromSeconds(this.SlideIntervalSeconds));
        //    }
        //}

        //internal void PrevSlide()
        //{
        //    if (this.cameraList.Count < 2)
        //    {        
        //        return;
        //    }

        //    if (this.SlideIndex > 0)
        //    {
        //        --this.SlideIndex;
        //    }
        //    else
        //    {
        //        this.SlideIndex = this.cameraList.Count - 1;
        //    }
        //}

        internal void Remove(UIElement element, bool aEventOccurrence = true)
        {
            if (element == null) return;

            //if (element is LayoutControl layoutControl)
            //{
            //    // 맵에 대한 리소스 해제
            //    var children = layoutControl.GetElements();

            //    foreach (var mapControl in children.OfType<MapControl>().Select(child => child))
            //    {
            //        mapControl.Dispose();
            //    }
            //}
            //else if (element is CameraControl cameraControl)
            //{
            //    cameraControl.ePreparedRendering -= this.control_ePreparedRendering;
            //}

            if (element is IDisposable disposable)
            {
                disposable.Dispose();
            }

            this._grid?.Children.Remove(element);

            if (!aEventOccurrence) return;

            this.OnItemRemoved(new CellElementChangedEventArgs(element));
            this.OnViewAreaUpdated(new EventArgs());
        }

        ///// <summary>
        ///// 슬라이드 정지.
        ///// </summary>
        //internal void StopSlide()
        //{
        //    if (this.slideThreadingTimer == null)
        //    {
        //        return;
        //    }

        //    this.slideThreadingTimer.Dispose();
        //    this.slideThreadingTimer = null;        
        //}

        /// <summary>
        /// 속성 변경 알림 이벤트 핸들러.
        /// </summary>
        /// <param name="e">Dependency Property Changed Event Args.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            var name = e.Property.Name;

            bool shouldUpdate = name == "Left" || name == "Top" ||
                                name == nameof(ActualWidth) || name == nameof(ActualHeight) ||
                                (name == nameof(IsVisible) && (bool)e.NewValue);

            if (shouldUpdate)
            {
                OnViewAreaUpdated(EventArgs.Empty);
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
        }

        ///// <summary>
        ///// 현재 순서 이외의 모든 카메라 컨트롤을 제거한다.
        ///// </summary>
        //internal void RemovePrevSlideCamera()
        //{
        //    if (!this.Dispatcher.CheckAccess())
        //    {
        //        this.Dispatcher.BeginInvoke(new Action(this.RemovePrevSlideCamera));
        //        return;
        //    }

        //    var cameraControls = this.GetAllCameraControl();

        //    var cameraControlsCount = cameraControls.Count;

        //    if (cameraControlsCount <= 1)
        //    {
        //        return;
        //    }

        //    var cameraId = this.cameraList[this.slideIndex].ID;

        //    for (var i = cameraControlsCount - 1; i >= 0; i--)
        //    {
        //        if (cameraControls[i].ID.Equals(cameraId))
        //        {
        //            continue;
        //        }

        //        Remove(cameraControls[i]);
        //    }
        //}

        ///// <summary>
        ///// Slide에서 현재 보여지고 있는 Camera를 변경한다. Sync 메시지를 보내지 않고 단순 변경만 한다.
        ///// </summary>
        //internal CameraControl ChangeVisibleSlideCamera(int idx)
        //{
        //    if (idx < 0 || idx >= this.cameraList.Count)
        //    {        
        //        return null;
        //    }

        //    // 이전 슬라이드 카메라 컨트롤 제거
        //    if (this._grid.Children.Count != 0)
        //    {
        //        //if (this._grid.Children.Count > 1 || this.slideIndex == idx)
        //        if (this.slideIndex == idx)
        //        {
        //            return null;
        //        }

        //        var oldControl = this._grid.Children[0] as CameraControl;

        //        if (oldControl == null)
        //        {
        //            return null;
        //        }

        //        // SynchronousAssign 옵션 사용 중일 경우는 ePreparedRendering 이벤트 수신 시 제거한다.
        //        if (CommonsConfig.Instance.UseSynchronousAssign)
        //        {
        //            if (this.synchronousAssignTimeoutTimer.Enabled)
        //            {
        //                this.synchronousAssignTimeoutTimer.Interval = CommonsConfig.Instance.SynchronousAssignTimeout;
        //            }
        //            else
        //            {
        //                this.synchronousAssignTimeoutTimer.Start();
        //            }
        //        }
        //        else
        //        {
        //            this.Remove(oldControl, false);
        //        }
        //    }

        //    try
        //    {
        //        var newControl = CreateCameraControlFromInfo(this.cameraList[idx]);
        //        if (!newControl.IsAudioDisabledForGrid && this.IsSelectedCell)
        //        {
        //            newControl.EnableAudio();
        //        }
        //        else
        //        {
        //            newControl.DisableAudio();
        //        }
        //        newControl.ePreparedRendering += this.control_ePreparedRendering;

        //        this._grid.Children.Insert(0, newControl);

        //        this.BindingElementSize(newControl);

        //        this.slideIndex = idx;

        //        return newControl;
        //    }
        //    catch (ArgumentOutOfRangeException)
        //    {
        //        Debug.WriteLine("SlideIndex set failed: " + idx + "/" + this.cameraList.Count + ". cameraList might be changed");
        //        return null;
        //    }
        //}

        //private void control_ePreparedRendering(object sender, EventArgs e)
        //{
        //    this.OnEPreparedRendering(new EventArgs());

        //    //if (CommonsConfig.Instance.UseSynchronousAssign && this.IsSlideMode())
        //    //{
        //    //    this.synchronousAssignTimeoutTimer.Stop();
        //    //    this.RemovePrevSlideCamera();
        //    //}
        //}

        //private void synchronousAssignTimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    //this.RemovePrevSlideCamera();
        //}
        public void Dispose()
        {
            try
            {

                _targetDropAdvisor.DropCompleted -= TargetDropAdvisor_DropCompleted;
                DragDropManager.SetDropTargetAdvisor(this, null);

                DragDropManager.SetDragSourceAdvisor(this, null);

                this.Clear();

                UIHelper.RemoveRoutedEventHandlers(_grid);

                UIHelper.RemoveEventHandlerByReflection(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("[Cell Dispose Error] {0}", ex));
            }
            finally
            {
                _border = null;
                _grid = null;
            }

            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// CellSplitException class.
    /// </summary>
    [Serializable]
    public class CellSplitException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellSplitException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CellSplitException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CellSplitException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cellStatus">The cell status.</param>
        public CellSplitException(string message, string cellStatus)
            : base(message)
        {
            CellStatus = cellStatus;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CellSplitException"/> class for deserialization.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        //protected CellSplitException(SerializationInfo info, StreamingContext context)
        //    : base(info, context)
        //{
        //    CellStatus = info.GetString(nameof(CellStatus));
        //}

        /// <summary>
        /// Gets or sets CellStatus.
        /// </summary>
        public string? CellStatus { get; set; }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate.</param>
        /// <param name="context">The destination for this serialization.</param>
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    if (info == null) throw new ArgumentNullException(nameof(info));

        //    base.GetObjectData(info, context);
        //    info.AddValue(nameof(CellStatus), CellStatus);
        //}
    }
}





