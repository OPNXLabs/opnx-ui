using OPNX.Lib.Data.ORM.Interfaces;
using OPNX.UI.WPF.Utilities;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls
{
    /// <summary>
    /// OpnxPlaybackTimeline.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OpnxPlaybackTimeline : UserControl
    {
        private static readonly IReadOnlyList<PlaybackTimelineRangeItem> DefaultTimeRangeItems =
        [
            new() { RangeType = PlaybackTimelineRangeType.M5, DisplayName = "5M" },
            new() { RangeType = PlaybackTimelineRangeType.M15, DisplayName = "15M" },
            new() { RangeType = PlaybackTimelineRangeType.M30, DisplayName = "30M" },
            new() { RangeType = PlaybackTimelineRangeType.H1, DisplayName = "1H" },
            new() { RangeType = PlaybackTimelineRangeType.H3, DisplayName = "3H" },
            new() { RangeType = PlaybackTimelineRangeType.H6, DisplayName = "6H" },
            new() { RangeType = PlaybackTimelineRangeType.H12, DisplayName = "12H" },
            new() { RangeType = PlaybackTimelineRangeType.H24, DisplayName = "24H" },
            new() { RangeType = PlaybackTimelineRangeType.D3, DisplayName = "3D" },
        ];

        #region Fields


        private bool isMouseDown = false;
        private bool isMouseDrag = false;
        private Point clickPoint = new(0, 0);
        private long clickCenterUnixTime = 0;

        //private long timelineKeyLastTime = 0;
        //private bool isTypingAllowed = false;

        private readonly DispatcherTimer scrollTimer = new();

        private bool isListBoxWheelHooked = false;
        #endregion

        #region Constructors
        public OpnxPlaybackTimeline()
        {
            InitializeComponent();

            this.xCanvasOuter.SizeChanged += HandleOuterCanvasSizeChanged;
            this.xScrollViewer.SizeChanged += HandleScrollViewerSizeChanged;
            this.xCanvasInner.SizeChanged += HandleInnerCanvasSizeChanged;

            this.xCanvasInner.MouseLeftButtonDown += HandleInnerCanvasMouseLeftButtonDown;
            this.xCanvasInner.MouseMove += HandleInnerCanvasMouseMove;
            this.xCanvasInner.MouseLeftButtonUp += HandleInnerCanvasMouseLeftButtonUp;
            this.xCanvasInner.MouseLeave += HandleInnerCanvasMouseLeave;

            this.xScrollViewer.PreviewMouseWheel += HandleTimelineMouseWheel;

            scrollTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            scrollTimer.Tick += HandleScrollTimerTick;

            CurrentTimeRange = PlaybackTimelineRangeType.H1;

            this.CenterTimeUnixMS = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(5)).ToUnixTimeMilliseconds();

            this.PreviewKeyDown += (a, e) =>
            {
                if (Keyboard.FocusedElement is not TextBox)
                {
                    e.Handled = true;
                    HandlePreviewKeyDown(a, e);
                }
            };

            xScrollViewer.ScrollChanged += (s, ev) =>
            {
                ScrollViewer? listBoxScrollViewer = UIHelper.FindChild<ScrollViewer>(xEntityListBox);
                if (listBoxScrollViewer != null)
                {
                    if (!isListBoxWheelHooked)
                    {
                        listBoxScrollViewer.PreviewMouseWheel += (s, e) =>
                        {
                            e.Handled = true;
                        };
                        isListBoxWheelHooked = true;
                    }
                    listBoxScrollViewer.ScrollToVerticalOffset(ev.VerticalOffset);
                }
            };


            //this.PreviewKeyUp += (a, e) =>
            //{
            //    //timelineKeyLastTime = 0;
            //};

            //this.Loaded += (a1, e2) =>
            //{
            //    //App.PlaybackWindow.xPlaybackControl.xMultiGridControl.PreviewMouseUp += (a, e) =>
            //    //{
            //    //    isTypingAllowed = false;
            //    //};

            //};
            //this.xCanvasInner.GotMouseCapture += (a, e) =>
            //{
            //    var mWin = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            //    isTypingAllowed = (mWin.GetType() != typeof(UIs.MultiExportProgressWindow)) ? true : false;
            //};
            //


            //if (listBoxScrollViewer != null)
            //{
            //    // xScrollViewer와 동기화
            //    xScrollViewer.ScrollChanged += (s, ev) =>
            //    {
            //        listBoxScrollViewer.ScrollToVerticalOffset(ev.VerticalOffset);
            //    };
            //}
        }

        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty CenterTimeUnixMSProperty = DependencyProperty.Register(
            nameof(CenterTimeUnixMS),
            typeof(long),
            typeof(OpnxPlaybackTimeline),
            new PropertyMetadata(0L, OnCenterTimeUnixMSChanged));

        public static readonly DependencyProperty CurrentTimeRangeProperty = DependencyProperty.Register(
            nameof(CurrentTimeRange),
            typeof(PlaybackTimelineRangeType),
            typeof(OpnxPlaybackTimeline),
            new PropertyMetadata(PlaybackTimelineRangeType.None, OnCurrentTimeRangeChanged));

        public static readonly DependencyProperty TimelineRangeItemsProperty = DependencyProperty.Register(
            nameof(TimelineRangeItems),
            typeof(IEnumerable),
            typeof(OpnxPlaybackTimeline),
            new PropertyMetadata(DefaultTimeRangeItems, OnTimelineRangeItemsChanged));

        public static readonly DependencyProperty TimeRangeHeaderProperty = DependencyProperty.Register(
            nameof(TimeRangeHeader),
            typeof(string),
            typeof(OpnxPlaybackTimeline),
            new PropertyMetadata("Time Range"));

        public static readonly DependencyProperty SelectedTimeRangeItemProperty = DependencyProperty.Register(
            nameof(SelectedTimeRangeItem),
            typeof(PlaybackTimelineRangeItem),
            typeof(OpnxPlaybackTimeline),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeRangeItemChanged));

        public static readonly DependencyProperty IsLeftPanelVisibleProperty = DependencyProperty.Register(
            nameof(IsLeftPanelVisible),
            typeof(bool),
            typeof(OpnxPlaybackTimeline),
            new PropertyMetadata(true, OnIsLeftPanelVisibleChanged));

        public static readonly DependencyProperty ShowEntityNameOnTimelineProperty = DependencyProperty.Register(
            nameof(ShowEntityNameOnTimeline),
            typeof(bool),
            typeof(OpnxPlaybackTimeline),
            new PropertyMetadata(true, OnShowEntityNameOnTimelineChanged));
        #endregion

        #region Events
        public event PlaybackTimelineCenterTimeChangedEventHandler? CenterTimeChanged;
        public event PlaybackTimelineRangeChangedEventHandler? TimeRangeChanged;
        public event PlaybackTimelineRequestEventHandler? RequestTimeline;
        #endregion

        #region Properties
        public long CenterTimeUnixMS
        {
            get => (long)GetValue(CenterTimeUnixMSProperty);
            set => SetValue(CenterTimeUnixMSProperty, value);
        }

        public PlaybackTimelineRangeType CurrentTimeRange
        {
            get => (PlaybackTimelineRangeType)GetValue(CurrentTimeRangeProperty);
            set => SetValue(CurrentTimeRangeProperty, value);
        }

        public IEnumerable? TimelineRangeItems
        {
            get => (IEnumerable?)GetValue(TimelineRangeItemsProperty);
            set => SetValue(TimelineRangeItemsProperty, value);
        }

        public string TimeRangeHeader
        {
            get => (string)GetValue(TimeRangeHeaderProperty);
            set => SetValue(TimeRangeHeaderProperty, value);
        }

        public PlaybackTimelineRangeItem? SelectedTimeRangeItem
        {
            get => (PlaybackTimelineRangeItem?)GetValue(SelectedTimeRangeItemProperty);
            set => SetValue(SelectedTimeRangeItemProperty, value);
        }

        public bool IsLeftPanelVisible
        {
            get => (bool)GetValue(IsLeftPanelVisibleProperty);
            set => SetValue(IsLeftPanelVisibleProperty, value);
        }

        public bool ShowEntityNameOnTimeline
        {
            get => (bool)GetValue(ShowEntityNameOnTimelineProperty);
            set => SetValue(ShowEntityNameOnTimelineProperty, value);
        }

        public long VisibleTimeRangeMS { get; private set; }
        #endregion

        #region Public Methods

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //ScrollViewer listBoxScrollViewer = UIHelper.FindChild<ScrollViewer>(xEntityListBox);
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //ScrollViewer listBoxScrollViewer = UIHelper.FindChild<ScrollViewer>(xEntityListBox);

        }
        public void UpdateTimeline(long elapsedMilliseconds)
        {
            if (isMouseDown || isMouseDrag)
                return;

            // 드래그 등으로 CenterUnixTimeMiliSeconds가 바뀌어도 자연스럽게 누적
            CenterTimeUnixMS += elapsedMilliseconds;
            RedrawTimelineUI();
        }
        public void AddRecordData(IEntity entity, long startUnixTimeMS, long endUnixTimeMS, PlaybackTimelineRecordingType recordingType)
        {
            xTimelineCanvas.AddRecordData(entity, startUnixTimeMS, endUnixTimeMS, recordingType);
        }

        public void AddEntity(IEntity entity)
        {
            xTimelineCanvas.AddEntity(entity);
        }
        public void RemoveEntity(int entityID)
        {
            xTimelineCanvas.RemoveEntity(entityID);
        }
        #endregion

        #region Private / Protected Methods

        private static void OnCenterTimeUnixMSChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPlaybackTimeline control)
            {
                long newValue = (long)e.NewValue;
                control.CenterTimeChanged?.Invoke(control, new PlaybackTimelineCenterTimeChangedEventArgs()
                {
                    CenterTimeUnixMS = newValue
                });
                control.RedrawTimelineUI();

                control.Dispatcher.Invoke(() => control.xTimelineCanvas.InvalidateVisual());
            }
        }

        private static void OnCurrentTimeRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPlaybackTimeline control)
            {
                PlaybackTimelineRangeType newTimeRange = (PlaybackTimelineRangeType)e.NewValue;

                control.VisibleTimeRangeMS = newTimeRange switch
                {
                    PlaybackTimelineRangeType.M5 => (long)TimeSpan.FromMinutes(5).TotalMilliseconds,
                    PlaybackTimelineRangeType.M15 => (long)TimeSpan.FromMinutes(15).TotalMilliseconds,
                    PlaybackTimelineRangeType.M30 => (long)TimeSpan.FromMinutes(30).TotalMilliseconds,
                    PlaybackTimelineRangeType.H1 => (long)TimeSpan.FromHours(1).TotalMilliseconds,
                    PlaybackTimelineRangeType.H3 => (long)TimeSpan.FromHours(3).TotalMilliseconds,
                    PlaybackTimelineRangeType.H6 => (long)TimeSpan.FromHours(6).TotalMilliseconds,
                    PlaybackTimelineRangeType.H12 => (long)TimeSpan.FromHours(12).TotalMilliseconds,
                    PlaybackTimelineRangeType.H24 => (long)TimeSpan.FromHours(24).TotalMilliseconds,
                    PlaybackTimelineRangeType.D3 => (long)TimeSpan.FromDays(3).TotalMilliseconds,

                    _ => throw new ArgumentOutOfRangeException(newTimeRange.ToString(), newTimeRange, null)
                };

                control.RedrawTimelineUI();
                control.xTimelineCanvas.UpdateTimelineExtent();
                control.xTimelineCanvas.InvalidateVisual();
                control.TimeRangeChanged?.Invoke(control, new PlaybackTimelineRangeChangedEventArgs()
                {
                    TimeRange = newTimeRange
                });

                control.UpdateSelectedTimeRangeItem(newTimeRange);
            }
        }

        private static void OnTimelineRangeItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPlaybackTimeline control)
                control.UpdateSelectedTimeRangeItem(control.CurrentTimeRange);
        }

        private static void OnSelectedTimeRangeItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPlaybackTimeline control &&
                e.NewValue is PlaybackTimelineRangeItem item &&
                control.CurrentTimeRange != item.RangeType)
            {
                control.CurrentTimeRange = item.RangeType;
            }
        }

        private static void OnIsLeftPanelVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPlaybackTimeline control)
                control.UpdateLeftPanelVisibility();
        }

        private static void OnShowEntityNameOnTimelineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPlaybackTimeline control)
                control.xTimelineCanvas.InvalidateVisual();
        }

        private void UpdateSelectedTimeRangeItem(PlaybackTimelineRangeType timeRange)
        {
            var items = GetTimeRangeItems();
            var selectedItem = items.FirstOrDefault(item => item.RangeType == timeRange);

            if (selectedItem is null && items.Count > 0)
            {
                selectedItem = items[0];
                if (CurrentTimeRange != selectedItem.RangeType)
                    CurrentTimeRange = selectedItem.RangeType;
            }

            if (!Equals(SelectedTimeRangeItem, selectedItem))
                SelectedTimeRangeItem = selectedItem;
        }

        private List<PlaybackTimelineRangeItem> GetTimeRangeItems()
        {
            if (TimelineRangeItems is null)
                return [];

            return TimelineRangeItems.OfType<PlaybackTimelineRangeItem>().ToList();
        }

        private void UpdateLeftPanelVisibility()
        {
            xLeftPanel.Visibility = IsLeftPanelVisible ? Visibility.Visible : Visibility.Collapsed;
            xLeftPanelColumn.Width = IsLeftPanelVisible ? new GridLength(150) : new GridLength(0);
        }

        private void ClearTimeline()
        {
            xTimelineCanvas.ClearTimeline();
        }

        private void OnRequestTimeline()
        {
            long centetTimeUnixMS = 0;
            long visibleTimeRangeMS = 0;
            Application.Current.Dispatcher.Invoke(() =>
            {
                centetTimeUnixMS = CenterTimeUnixMS;
                visibleTimeRangeMS = VisibleTimeRangeMS;
            });

            RequestTimeline?.Invoke(this, new PlaybackTimelineRequestEventArgs()
            {
                CenterTimeUnixMS = centetTimeUnixMS,
                VisibleTimeRangeMS = visibleTimeRangeMS
            });
            //xTimelineCanvas.RefreshTimeline_BackgroundWorker();        
        }

        private int GetLineIntervalMinutes()
        {
            //int gapMinutes = (int)(this.timelineControl.VisibleTotalUnixTimeMiliSeconds / 10000 / 6 / 6);
            double gapMinutes = this.VisibleTimeRangeMS / 10000 / 6 / 6;
            double tempGapMinutes = (double)(int)gapMinutes;

            //각 line들 사이 간격이 소숫점으로 나올 경우 올림을 함 (ex. 2.5 --> 3)
            if (gapMinutes != tempGapMinutes)
                gapMinutes = tempGapMinutes + 1;

            //각 line들 사이 간격이 1분보다 작을 경우 1분으로 세팅함
            if (gapMinutes < 1)
                gapMinutes = 1;

            return (int)gapMinutes;
        }


        private DateTime GetLeftTimeNearestCenter()
        {
            DateTime centerDateTime = DateTimeOffset.FromUnixTimeMilliseconds(this.CenterTimeUnixMS).LocalDateTime;

            //각 Line들 사이의 간격
            int gapMinutes = this.GetLineIntervalMinutes();

            double totalMinutes = (centerDateTime.Hour * 60) + centerDateTime.Minute;
            double leftMinutes = ((int)(totalMinutes / gapMinutes)) * gapMinutes;

            DateTime leftDateTime = centerDateTime.Subtract(TimeSpan.FromMinutes(totalMinutes - leftMinutes));

            return leftDateTime;
        }

        private double GetPosition(double centerUnixTime, double positionUnixTime)
        {
            //1시간을 기준으로 시간이동을 구함
            double visibleTotalMiliSeconds = this.VisibleTimeRangeMS;
            double gapRatio = (centerUnixTime - positionUnixTime) / visibleTotalMiliSeconds;

            return (this.xTimeLineGrid.ActualWidth / 2) - (this.xTimeLineGrid.ActualWidth * gapRatio);
        }

        private void RedrawTimelineUI()
        {
            long centerUnixTime = this.CenterTimeUnixMS;
            DateTime centerDT = DateTimeOffset.FromUnixTimeMilliseconds(centerUnixTime).LocalDateTime;
            CultureInfo culture = CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.CurrentCulture;

            DateTime startDT = centerDT.AddMilliseconds(-VisibleTimeRangeMS / 2);
            DateTime endDT = centerDT.AddMilliseconds(VisibleTimeRangeMS / 2);

            // UI 날짜/시간 갱신
            xTextBlockVisibleStartDate.Text = startDT.ToString("yyyy. MM. dd. dddd", culture);
            xTextBlockVisibleCenterDate.Text = centerDT.ToString("yyyy. MM. dd. dddd", culture);
            xTextBlockVisibleEndDate.Text = endDT.ToString("yyyy. MM. dd. dddd", culture);

            xTextBlockVisibleStartTime.Text = startDT.ToString("tt hh:mm:ss.fff", culture);
            xTextBlockVisibleCenterTime.Text = centerDT.ToString("tt hh:mm:ss.fff", culture);
            xTextBlockVisibleEndTime.Text = endDT.ToString("tt hh:mm:ss.fff", culture);

            // 라인과 시간 텍스트 위치 계산
            int gapMinutes = GetLineIntervalMinutes();
            DateTime leftTimeNearestCenter = GetLeftTimeNearestCenter();
            DateTime startLineDT = new(leftTimeNearestCenter.Year, leftTimeNearestCenter.Month, leftTimeNearestCenter.Day,
                                       leftTimeNearestCenter.Hour, leftTimeNearestCenter.Minute, 0);

            var lineTimes = new[]
            {
                startLineDT.AddMinutes(-gapMinutes * 2),
                startLineDT.AddMinutes(-gapMinutes),
                startLineDT,
                startLineDT.AddMinutes(gapMinutes),
                startLineDT.AddMinutes(gapMinutes * 2),
                startLineDT.AddMinutes(gapMinutes * 3)
            };

            var lines = new[] { xRectangleLine01, xRectangleLine02, xRectangleLine03, xRectangleLine04, xRectangleLine05, xRectangleLine06 };
            var timeTexts = new[] { xTextBlockTime01, xTextBlockTime02, xTextBlockTime03, xTextBlockTime04, xTextBlockTime05, xTextBlockTime06 };

            for (int i = 0; i < lines.Length; i++)
            {
                // 정수 픽셀로 위치 설정
                int x = (int)Math.Round(GetPosition(centerUnixTime, new DateTimeOffset(lineTimes[i]).ToUnixTimeMilliseconds()));
                Canvas.SetLeft(lines[i], x);

                // 시간 텍스트 중앙 정렬
                timeTexts[i].Text = $"{lineTimes[i]:HH mm}";
                Canvas.SetLeft(timeTexts[i], x - (int)(timeTexts[i].ActualWidth / 2));
            }

            // 중앙 기준 라인 좌표
            int centerX = (int)Math.Round(GetPosition(centerUnixTime, centerUnixTime));
            Canvas.SetLeft(xRectangleLineCenter, centerX);
        }

        private void Refresh()
        {
            this.RedrawTimelineUI();
            this.xTimelineCanvas.InvalidateVisual();
        }

        //private double lastCameraPlayUnixTimeMiliSeconds = 0;

        private void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            //var dataContext = App.PlaybackControl.xTimeNavigationControl.DataContext as TimeNavigationControlViewModel;
            //var currentTime = TimeConverter.GetCurrentUnixTime();
            //var targetTime = timelineKeyLastTime;
            //timelineKeyLastTime = currentTime;
            //if (!isTypingAllowed || (currentTime - targetTime) < 1)
            //{
            //    return;
            //}

            //// Key frame navigation (After-Effects style)
            //switch (e.SystemKey)
            //{
            //    case Key.Left:
            //        dataContext.SearchBeforeKeyFrame();
            //        break;
            //    case Key.Right:
            //        dataContext.SearchNextKeyFrame();
            //        break;
            //    default:
            //        break;
            //}

            //// General navigation (Commonly used style)
            //switch (e.Key)
            //{
            //    case Key.Space:
            //        if (dataContext.TimeNavigationControlData.IsCheckedPlay)
            //        {
            //            App.PlaybackControl.xTimeNavigationControl.Pause();
            //        }
            //        else
            //        {
            //            App.PlaybackControl.xTimeNavigationControl.Play();
            //        }
            //        break;
            //    case Key.J:
            //        App.PlaybackControl.xTimeNavigationControl.Rewind();
            //        break;
            //    case Key.K:
            //        App.PlaybackControl.xTimeNavigationControl.Pause();
            //        break;
            //    case Key.L:
            //        App.PlaybackControl.xTimeNavigationControl.Play();
            //        break;
            //    case Key.Left:
            //        dataContext.SearchBeforeFrame();
            //        break;
            //    case Key.Right:
            //        dataContext.SearchNextFrame();
            //        break;
            //    case Key.Home:
            //        dataContext.SearchFirst();
            //        break;
            //    case Key.End:
            //        dataContext.SearchLast();
            //        break;
            //    case Key.OemMinus:
            //        if (xComboBoxTimePeriod.SelectedIndex != 9)
            //        {
            //            xComboBoxTimePeriod.SelectedIndex++;
            //        }
            //        break;
            //    case Key.OemPlus:
            //        if (xComboBoxTimePeriod.SelectedIndex != 0)
            //        {
            //            xComboBoxTimePeriod.SelectedIndex--;
            //        }
            //        break;
            //    default:
            //        break;
            //}
        }

        private void HandleInnerCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.xCanvasInner.IsMouseCaptured)
                this.xCanvasInner.CaptureMouse();

            this.clickPoint = e.GetPosition(this.xCanvasInner);
            this.clickCenterUnixTime = this.CenterTimeUnixMS;
            //this.lastCameraPlayUnixTimeMiliSeconds = 0;
            this.isMouseDown = true;
            this.isMouseDrag = false;

            //실시간 Camera Data Update 중지 !!
            //this.xInnerControl.StopRealTimeRefreshTimer();
        }

        //private void ShowEventTooltip(Point mousePointRelativeInnerCanvas)
        //{
        //    //this.xInnerControl.ShowEventTooltip(mousePointRelativeInnerCanvas);
        //}

        //private void MoveEventTooltip(double gapX)
        //{
        //    //this.xInnerControl.MoveEventTooltip(gapX);
        //}

        private void HandleInnerCanvasMouseLeave(object sender, MouseEventArgs e)
        {
            this.xGridEventInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void HandleInnerCanvasMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (!this.isMouseDown)
            {
                //Point movePoint = e.GetPosition(this.xCanvasInner);
                //ShowEventTooltip(movePoint);
                return;
            }

            //최초 Move시 전체 Camera를 Pause함 !!
            if (!isMouseDrag)
            {
                //App.PlaybackControl.xTimeNavigationControl.Pause();
            }

            this.isMouseDrag = true;

            //새로운 Center 시간을 구함
            Point clickPoint = e.GetPosition(this.xCanvasInner);
            double gapX = clickPoint.X - this.clickPoint.X;

            ////1시간을 기준으로 시간이동을 구함
            //TimeSpan timeSpan = TimeSpan.FromMilliseconds(this.VisibleTotalUnixTimeMiliSeconds);
            //double movedMiliSeconds = timeSpan.TotalMilliseconds * gapX / this.xCanvasInner.ActualWidth;
            //this.CenterUnixTimeMiliSeconds = this.clickCenterUnixTime - movedMiliSeconds;

            long visibleTimeRangeMS = this.VisibleTimeRangeMS;
            double ratio = gapX / this.xCanvasInner.ActualWidth;
            long movedMs = (long)(visibleTimeRangeMS * ratio);
            this.CenterTimeUnixMS = this.clickCenterUnixTime - movedMs;

            //(this.DataContext as TimelineControlViewModel).RefreshTimelineExceptRequestCamera();
            this.Refresh();

            //Tooltip 좌표를 변경함
            //if (this.xGridEventInfo.Visibility == System.Windows.Visibility.Visible)
            //{
            //    this.MoveEventTooltip(gapX);
            //}

            //BaseTime이 변경됐음을 알림 !! 다른 UI 시간 동기화 진행 !!
            //bool isSeekAllCamera = false;
            //if (constset.IsSeekAllCameraWhenTimelineDragging)
            //    isSeekAllCamera = true;

            //if (isPreviewMode)
            //{
            //    BaseTimeManager.Instance.PreviewTimelineBaseTimeChanged(this.CenterUnixTimeMiliSeconds, isSeekAllCamera);
            //}
            //else
            //{
            //    BaseTimeManager.Instance.TimelineBaseTimeChanged(this.CenterUnixTimeMiliSeconds, isSeekAllCamera);
            //}
        }

        private void HandleInnerCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (this.xCanvasInner.IsMouseCaptured)
            {
                this.xCanvasInner.ReleaseMouseCapture();
            }

            if (!this.isMouseDown)
                return;

            this.isMouseDown = false;
            this.isMouseDrag = false;

            ////Drag중에 Camera에서 Play 진행시간이 들어온 경우 마지막 값만 처리해줌 !!
            //if (this.lastCameraPlayUnixTimeMiliSeconds != 0)
            //{
            //    //this.CameraPlayTimeChanged(this.lastCameraPlayUnixTimeMiliSeconds);
            //}

            //this.lastCameraPlayUnixTimeMiliSeconds = 0;


            //(this.DataContext as TimelineControlViewModel).RefreshTimeline();
            OnRequestTimeline();

            //실시간 Camera Data Update 시작 !!
            //this.xInnerControl.StartRealTimeRefreshTimer();

            //if (!isPreviewMode)
            //{
            //    //BaseTime이 변경됐음을 알림 !! 다른 UI 시간 동기화 진행 !!
            //    //BaseTimeManager.Instance.TimelineBaseTimeChanged(this.CenterUnixTimeMiliSeconds);

            //    // VideoPlayControlManager  [2014. 02. 17 엄태영]
            //    //List<Innotive.InnoWatch.DLLs.CameraControls.CameraControlPlayback> cameraControlList = App.PlaybackControl.xMultiGridControl.GetAllCameraControls();
            //    //cameraControlList.ForEach(item =>
            //    //{
            //    //    Innotive.InnoWatch.Commons.CameraManagers.VideoPlayControlManager.Instance.ChangPosition(item.VideoElement, "MoveTimeline");
            //    //});
            //}
            //else
            //{
            //    //BaseTimeManager.Instance.PreviewTimelineBaseTimeChanged(this.CenterUnixTimeMiliSeconds);
            //    //if (previewCameraControl != null && previewCameraControl.VideoElement != null)
            //    //{
            //    //    Innotive.InnoWatch.Commons.CameraManagers.VideoPlayControlManager.Instance.ChangPosition(previewCameraControl.VideoElement, "MoveTimeline");
            //    //}
            //}
        }

        //public void CameraPlayTimeChanged(double playUnixTimeMiliSeconds)
        //{
        //    //Drag중이면 처리하지 않음 !!
        //    if (this.isMouseDown)
        //        return;

        //    this.CenterUnixTimeMiliSeconds = playUnixTimeMiliSeconds;

        //    //(this.DataContext as TimelineControlViewModel).RefreshTimelineExceptRequestCamera();
        //    this.RefreshTimelineExceptRequestCamera();
        //    RefreshTimeline();
        //    //BaseTime이 변경됐음을 알림 !! 다른 UI 시간 동기화 진행 !!
        //    //BaseTimeManager.Instance.TimelineBaseTimeChanged(this.CenterUnixTime);
        //}

        //public void CameraSeekCompleted(double playUnixTimeMiliSeconds)
        //{
        //    //Drag중이면 처리하지 않음
        //    if (this.isMouseDown)
        //        return;

        //    this.CenterUnixTimeMiliSeconds = playUnixTimeMiliSeconds;

        //    //(this.DataContext as TimelineControlViewModel).RefreshTimelineExceptRequestCamera();
        //    this.RefreshTimelineExceptRequestCamera();
        //    RefreshTimeline();

        //    //BaseTime이 변경됐음을 알림 !! 다른 UI 시간 동기화 진행 !!
        //    //BaseTimeManager.Instance.TimelineBaseTimeChanged(this.CenterUnixTime);
        //}

        //public void CameraFrameSearchCompleted(double playUnixTimeMiliSeconds)
        //{
        //    //Drag중이면 처리하지 않고 값만 저장해둠 !!
        //    if (this.isMouseDown)
        //        return;

        //    this.CenterUnixTimeMiliSeconds = playUnixTimeMiliSeconds;

        //    //(this.DataContext as TimelineControlViewModel).RefreshTimelineExceptRequestCamera();
        //    this.RefreshTimelineExceptRequestCamera();

        //    //BaseTime이 변경됐음을 알림 !! 다른 UI 시간 동기화 진행 !!
        //    //BaseTimeManager.Instance.TimelineBaseTimeChanged(this.CenterUnixTime);
        //}

        private void HandleOuterCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.xScrollViewer.Width = this.xCanvasOuter.ActualWidth;

            double height = this.xCanvasOuter.ActualHeight - this.xCanvasTime.Height;
            if (height < 0)
                height = 0;

            this.xScrollViewer.Height = height;

            this.xCanvasTime.Width = this.xCanvasOuter.ActualWidth;
            //Canvas.SetTop(this.xCanvasTime, this.xScrollViewer.Height);

            this.xRectangleLineCenter.Height = this.xCanvasOuter.ActualHeight;
        }

        private void HandleScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.xCanvasInner.Width = this.xScrollViewer.Width;
            this.xCanvasInner.Height = this.xScrollViewer.Height;
            this.xTimelineCanvas.UpdateTimelineExtent();
        }

        private void HandleInnerCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.xTimelineCanvas.Width = this.xCanvasInner.Width;
            this.xTimelineCanvas.Height = this.xCanvasInner.Height;
            this.xTimelineCanvas.UpdateTimelineExtent();

            //TimelineControlViewModel timelineControlViewModel = this.DataContext as TimelineControlViewModel;
            //if (timelineControlViewModel != null)
            //    timelineControlViewModel.RefreshTimelineExceptRequestCamera();
            this.Refresh();
        }


        //public void SelectEntity(List<int> entityIDList)
        //{
        //    if (entityIDList.Count < 1)
        //        return;

        //    xInnerControl.SelectEntity(entityIDList);                
        //}

        //public void ScrollSelectedEntityPosition()
        //{
        //    xInnerControl.ScrollSelectedEntityPosition();            
        //}

        //public void SetPreviewCameraControl(CameraControlPlayback cameraControl)
        //{
        //    this.previewCameraControl = cameraControl;
        //    this.xInnerControl.PreviewCameraControl = this.previewCameraControl;
        //}

        private void HandleTimelineMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            ScrollViewer? scrollviewer = sender as ScrollViewer;

            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                if (e.Delta > 0)
                    scrollviewer?.LineUp();
                else
                    scrollviewer?.LineDown();

                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Alt) > 0)
            {
                //if (e.Delta > 0)
                //{
                //    if (xComboBoxTimePeriod.SelectedIndex != 0)
                //        xComboBoxTimePeriod.SelectedIndex--;
                //}
                //else
                //{
                //    if (xComboBoxTimePeriod.SelectedIndex != 9)
                //        xComboBoxTimePeriod.SelectedIndex++;
                //}

                return;
            }

            if (!scrollTimer.IsEnabled)
            {
                this.xScrollViewer.Focus();
                if (!this.xCanvasInner.IsMouseCaptured)
                    this.xCanvasInner.CaptureMouse();

                //this.xTimelineCanvas.StopRealTimeRefreshTimer();
                //최초 Move시 전체 Camera를 Pause함 !!
                //App.PlaybackControl.xTimeNavigationControl.Pause();
            }
            else
            {
                scrollTimer.Stop();
            }
            scrollTimer.Start();

            ////새로운 Center 시간을 구함
            //double gapX = e.Delta / 2;

            ////1시간을 기준으로 시간이동을 구함
            //TimeSpan timeSpan = TimeSpan.FromMilliseconds(this.VisibleTotalUnixTimeMiliSeconds);
            //double movedMiliSeconds = timeSpan.TotalMilliseconds * gapX / this.xCanvasInner.ActualWidth;
            //this.CenterUnixTimeMiliSeconds -= movedMiliSeconds;

            // Canvas 이동 거리
            double gapX = e.Delta / 2;

            // Visible 범위를 long으로 안전하게 처리
            long visibleTimeRangeMS = this.VisibleTimeRangeMS;

            // 이동 시간 계산
            long movedMs = (long)(visibleTimeRangeMS * gapX / this.xCanvasInner.ActualWidth);

            // 새로운 Center Unix Time 계산
            this.CenterTimeUnixMS -= movedMs;

            //(this.DataContext as TimelineControlViewModel).RefreshTimelineExceptRequestCamera();
            this.Refresh();

            //Tooltip 좌표를 변경함
            //this.MoveEventTooltip(gapX);

            //BaseTime이 변경됐음을 알림 !! 다른 UI 시간 동기화 진행 !!
            //bool isSeekAllCamera = false;
            //if (constset.IsSeekAllCameraWhenTimelineDragging)
            //    isSeekAllCamera = true;

            //if (isPreviewMode)
            //{
            //    BaseTimeManager.Instance.PreviewTimelineBaseTimeChanged(this.CenterUnixTimeMiliSeconds, isSeekAllCamera);
            //}
            //else
            //{
            //    BaseTimeManager.Instance.TimelineBaseTimeChanged(this.CenterUnixTimeMiliSeconds, isSeekAllCamera);
            //}
        }

        private void HandleScrollTimerTick(object? sender, EventArgs e)
        {
            scrollTimer.Stop();

            if (this.xCanvasInner.IsMouseCaptured)
            {
                this.xCanvasInner.ReleaseMouseCapture();
            }

            //(this.DataContext as TimelineControlViewModel).RefreshTimeline();

            //RefreshTimeline();

            //실시간 Camera Data Update 시작 !!
            //this.xInnerControl.StartRealTimeRefreshTimer();

            //if (!isPreviewMode)
            //{
            //    //BaseTime이 변경됐음을 알림 !! 다른 UI 시간 동기화 진행 !!
            //    //BaseTimeManager.Instance.TimelineBaseTimeChanged(this.CenterUnixTimeMiliSeconds);

            //    //List<CameraControlPlayback> cameraControlList = App.PlaybackControl.xMultiGridControl.GetAllCameraControls();
            //    //cameraControlList.ForEach(item =>
            //    //{
            //    //    Commons.CameraManagers.VideoPlayControlManager.Instance.ChangPosition(item.VideoElement, "MoveTimeline");
            //    //});
            //}
            //else
            //{
            //    //BaseTimeManager.Instance.PreviewTimelineBaseTimeChanged(this.CenterUnixTimeMiliSeconds);
            //    //if (previewCameraControl != null && previewCameraControl.VideoElement != null)
            //    //{
            //    //    Commons.CameraManagers.VideoPlayControlManager.Instance.ChangPosition(previewCameraControl.VideoElement, "MoveTimeline");
            //    //}
            //}
        }

        private void HandleIncreaseTimeRangeClick(object sender, RoutedEventArgs e)
        {
            var items = GetTimeRangeItems();

            int currentIndex = items.FindIndex(item => item.RangeType == CurrentTimeRange);

            if (currentIndex >= 0 && currentIndex < items.Count - 1)
                CurrentTimeRange = items[currentIndex + 1].RangeType;
        }

        private void HandleDecreaseTimeRangeClick(object sender, RoutedEventArgs e)
        {
            var items = GetTimeRangeItems();

            int currentIndex = items.FindIndex(item => item.RangeType == CurrentTimeRange);

            if (currentIndex > 0)
                CurrentTimeRange = items[currentIndex - 1].RangeType;
        }
        #endregion
    }
}

