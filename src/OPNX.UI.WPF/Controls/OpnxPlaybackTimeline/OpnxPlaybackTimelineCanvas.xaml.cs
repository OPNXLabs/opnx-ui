using OPNX.Lib.Data.ORM.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    /// <summary>
    /// OpnxPlaybackTimelineCanvas.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OpnxPlaybackTimelineCanvas : UserControl
    {
        #region Fields
        private const double drawCameraStartY = 7;
        private const double drawCameraHeight = 15;
        private const double drawCameraEmptySpace = 10;

        private readonly SolidColorBrush? cameraBackBrush = new BrushConverter().ConvertFromString("#66000000") as SolidColorBrush;
        private readonly SolidColorBrush? cameraBrush = new BrushConverter().ConvertFromString("#FFC000") as SolidColorBrush;
        private readonly SolidColorBrush? cameraSelectBrush = new BrushConverter().ConvertFromString("#FF73B2F2") as SolidColorBrush;

        //private InfiniteTimeLineControl timelineControl = null;

        private readonly ObservableCollection<PlaybackTimelineRecordData> timelineRecords = [];

        //private LoadingWindow loadingWindow = new LoadingWindow();
        //private static BackgroundWorker backgroundWorker = new BackgroundWorker();
        //private static bool isRunBackgroundWorker = false;
        //private static bool isReceiveRefreshMessage = false;

        //private static object lockObject = new object();

        //일정 주기로 Camera Data를 Update 함 !!
        //private DispatcherTimer realTimeRefreshTimer = new DispatcherTimer();

        //Event를 화면에 표시하기 위해 사용함
        //private BrushConverter brushConverter = new BrushConverter();

        //Panning시 Tooltip을 이동하기 위해서 Tooltip이 화면에 보여질때의 X좌표를 기억함
        //private double lastVisibleTooltipX = 0;

        //Camera Label 표시
        //private Pen cameraLabelEdgePen = new Pen(Brushes.White, 1);
        //private SolidColorBrush cameraLabelBackgroundBrush = null;

        //public bool IsPreviewMode { get; set; }
        //public CameraControlPlayback PreviewCameraControl { get; set; }

        //private TimelineTimePopup popup = new TimelineTimePopup(); 
        #endregion

        #region Constructors
        public OpnxPlaybackTimelineCanvas()
        {
            InitializeComponent();

            //Color color = (Color)ColorConverter.ConvertFromString("#A5000000");
            //this.cameraLabelBackgroundBrush = new SolidColorBrush(color);

            //this.IsPreviewMode = false;

            //backgroundWorker.WorkerReportsProgress = true;
            //backgroundWorker.WorkerSupportsCancellation = true;
            //backgroundWorker.DoWork += backgroundWorker_DoWork;
            //backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;

            //this.realTimeRefreshTimer.Tick += this.RealTimeRefreshTimer_Tick;
            ////this.realTimeRefreshTimer.Interval = TimeSpan.FromSeconds(constset.RealTimeTimelineUpdateIntervalSeconds);
            //this.realTimeRefreshTimer.Start();
            this.MouseMove += DrawPopup;
            this.MouseLeave += HidePopup;
            //this.popup.xPopup.Placement = PlacementMode.RelativePoint;
            //this.popup.xPopup.PlacementTarget = this;
        }
        #endregion

        #region Properties
        public ObservableCollection<PlaybackTimelineRecordData> TimelineRecords => timelineRecords;
        #endregion

        #region Dependency Properties
        public OpnxPlaybackTimeline ParentTimelineControl
        {
            get => (OpnxPlaybackTimeline)GetValue(ParentTimelineControlProperty);
            set => SetValue(ParentTimelineControlProperty, value);
        }

        public static readonly DependencyProperty ParentTimelineControlProperty =
            DependencyProperty.Register(
                nameof(ParentTimelineControl), 
                typeof(OpnxPlaybackTimeline), 
                typeof(OpnxPlaybackTimelineCanvas),
                new PropertyMetadata(null, OnParentTimelineControlChanged));
        #endregion

        #region Public Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate(); 
        }

        public PlaybackTimelineRecordData? AddEntity(IEntity entity)
        {
            if (timelineRecords.Any(x => x.Entity.ID == entity.ID))
                return null; ;

            var newRecordData = new PlaybackTimelineRecordData(entity);

            timelineRecords.Add(newRecordData);

            //this.RefreshTimeline_BackgroundWorker();
            this.InvalidateVisual();

            return newRecordData;
        }

        private static void OnParentTimelineControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is OpnxPlaybackTimeline timeline)
                timeline.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        }

        private PlaybackTimelineRecordData GetOrAddEntity(IEntity entity)
        {
            var recordData = timelineRecords.FirstOrDefault(x => x.Entity.ID == entity.ID);
            if (recordData != null)
                return recordData;

            recordData = new PlaybackTimelineRecordData(entity);
            timelineRecords.Add(recordData);
            return recordData;
        }

        public void RemoveEntity(int entityID)
        {
            var findRecordData = timelineRecords.FirstOrDefault(x => x.Entity.ID == entityID);
            if (findRecordData == null)
                return;
            findRecordData.ClearRecordData();
            findRecordData.ClearEventData();

            timelineRecords.Remove(findRecordData);

            this.InvalidateVisual();
        }

        public void AddRecordData(IEntity entity, long startUnixMs, long endUnixMs, PlaybackTimelineRecordingType recordingType)
        {
            var recordInfo = new PlaybackTimelineRecordInfo(startUnixMs, endUnixMs, recordingType);
            var findRecordData = GetOrAddEntity(entity);

            findRecordData.AddRecordInfo(recordInfo);

            this.InvalidateVisual();
        }

        public void AddEventData(IEntity entity, long startUnixMs, long endUnixMs, string eventType, SolidColorBrush eventColor, int mergeEventCount)
        {
            var startBeginDate = DateTimeOffset.FromUnixTimeMilliseconds(startUnixMs).UtcDateTime;

            var description = $"{eventType} ({startBeginDate})";

            var eventInfo = new PlaybackTimelineEventInfo(startUnixMs, endUnixMs, eventType, eventColor, description, mergeEventCount);
            var findRecordData = GetOrAddEntity(entity);

            findRecordData.AddEventInfo(eventInfo);

            this.InvalidateVisual();
        }
        #endregion

        private void HidePopup(object sender, MouseEventArgs e)
        {
            //if (this.popup.xPopup.IsOpen)
            //    this.popup.xPopup.IsOpen = false;
        }

        private void DrawPopup(object sender, MouseEventArgs e)
        {
            //if (!this.popup.xPopup.IsOpen)
            //    this.popup.xPopup.IsOpen = true;
            //this.popup.xPopup.VerticalOffset = e.GetPosition(sender as UserControl).Y - 25;
            //this.popup.xPopup.HorizontalOffset = e.GetPosition(sender as UserControl).X + 10;
            //var positionTime = TimeConverter.ConvertUnixTimeMiliSecondsToLocalDateTime((this.timelineControl.CenterUnixTimeMiliSeconds) -
            //                    this.timelineControl.VisibleTotalUnixTimeMiliSeconds *
            //                    (0.5 - e.GetPosition((sender as UserControl)).X / this.timelineControl.ActualWidth));
            //this.popup.xTimeText.Text = positionTime.ToString();
            //(this.DataContext as TimelineControlViewModel).RefreshPopup();
        }

        //public void StartRealTimeRefreshTimer()
        //{
        //    this.realTimeRefreshTimer.Start();
        //}

        //public void StopRealTimeRefreshTimer()
        //{
        //    this.realTimeRefreshTimer.Stop();
        //}

        //private void RealTimeRefreshTimer_Tick(object sender, EventArgs e)
        //{
        //    this.RefreshTimeline_BackgroundWorker();

        //    /*
        //    //realTimeRefreshBackgroundWorker.RunWorkerAsync();

        //    InnotiveDebug.Trace(1, "[blackRoot51] Timer 시작 !!");

        //    //Camera Data Request를 한적이 없으면 그냥 return 함 !!
        //    if (this.lastRequestedStartUnixTimeMiliSeconds == -1 || this.lastRequestedEndUnixTimeMiliSeconds == -1)
        //    {
        //        InnotiveDebug.Trace(1, "[blackRoot50] realTimeRefreshBackgroundWorker_DoWork() --> Request를 한번도 하지 않았기 때문에 그냥 return 함 !!");
        //        return;
        //    }

        //    //현재 화면에 마지막 시간이 보여지고 있는지 Check..
        //    if (!this.CheckLastTimeVisible())
        //    {
        //        InnotiveDebug.Trace(4, "[blackRoot50] 현재 시간이 Timeline에 표시되지 않고 있음 !!");
        //    }
        //    else
        //    {
        //        InnotiveDebug.Trace(2, "[blackRoot50] 현재 시간이 Timeline에 표시되고 있음 !!");

        //        this.RefreshTimeline_BackgroundWorker();
        //    }

        //    //보여지고 있다면 끝부분 Refresh.. 마지막 Refresh를 한 시간을 저장하고 있어야 할듯.. 
        //     마지막 Refresh 시간과 현재시간 사이의 CameraData를 구해서 Time값을 합치면 됨 !!
        //    */
        //}

        /*
        //Timeline 화면에 현재 시간이 보여지고 있는지 여부
        private bool CheckLastTimeVisible()
        {
            double currentUnixTimeMiliSeconds = TimeConverter.ConvertLocalDateTimeToUnixTimeMiliSeconds(DateTime.Now);
            double centerUnixTimeMiliSeconds = this.timelineControl.CenterUnixTimeMiliSeconds;
            double halfUnixTimeMiliSeconds = this.timelineControl.VisibleTotalUnixTimeMiliSeconds / 2;

            //현재 시간이 Timeline의 현재 보여지고 있는 화면에 포함이 되는지 Check.
            if (centerUnixTimeMiliSeconds - halfUnixTimeMiliSeconds < currentUnixTimeMiliSeconds &&
                centerUnixTimeMiliSeconds + halfUnixTimeMiliSeconds > currentUnixTimeMiliSeconds)
            {
                return true;
            }

            return false;
        }
        */


        //public void SelectEntity(List<int> entityIDList)
        //{
        //    this.UnselectAllCamera(false);
        //    this.timeLineControlData.SelectCamera(entityIDList);
        //    this.InvalidateVisual();
        //}

        //public void UnselectAllCamera(bool isInvalidateVisual = true)
        //{
        //    this.timeLineControlData.UnSelectAllEntity();

        //    if (isInvalidateVisual)
        //        this.InvalidateVisual();
        //}

        //public void ScrollSelectedEntityPosition()
        //{
        //    for (int i = 0; i < timeLineControlRecordDatas.Count; i++)
        //    {
        //        var recordData = timeLineControlRecordDatas[i];
        //        if (recordData.IsSelected)
        //        {
        //            double cameraTop = this.GetEntityTop(i - 1);
        //            //App.PlaybackControl.xTimelineControl.xScrollViewer.ScrollToVerticalOffset(cameraTop - 5);
        //            return;
        //        }
        //    }

        //    //    r (int i = 0; i < this.timeLineControlData.GetRecordDataCount(); i++)
        //    //{
        //    //    TimeLineControlRecordData recordData = this.timeLineControlData.GetRecordDataAt(i);
        //    //    if (recordData == null)
        //    //        continue;

        //    //    //선택된 Camera인 경우 선택 표시를 그려줌
        //    //    if (recordData.IsSelected)
        //    //    {
        //    //        double cameraTop = this.GetEntityTop(i - 1);
        //    //        //App.PlaybackControl.xTimelineControl.xScrollViewer.ScrollToVerticalOffset(cameraTop - 5);
        //    //        return;
        //    //    }
        //    //}
        //}

        //void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    ////lock (lockObject)
        //    //{
        //    //    //if (constset.UseSampleData)
        //    //    //    this.RequestSampleCameraData();
        //    //    //else
        //    //    //    this.RequestCameraData();

        //    //    if (!constset.UseSampleData)
        //    //        this.RequestCameraData();
        //    //}

        //    this.RequestRecordData();
        //}

        //void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    //InnotiveDebug.Trace(2, "[blackRoot08] Request Complete");
        //    isRunBackgroundWorker = false;

        //    //lock (lockObject)
        //    {
        //        if (this.timelineControl != null)
        //        {
        //            //var viewModel = this.timelineControl.DataContext as TimelineControlViewModel;
        //            //if (viewModel != null)
        //            //    (this.timelineControl.DataContext as TimelineControlViewModel).RefreshTimelineExceptRequestCamera();
        //        }
        //    }

        //    //최초 한번만 실행하고 Close 해줌 !!
        //    //if (this.loadingWindow != null)
        //    //{
        //    //    this.loadingWindow.CloseWindow();
        //    //    this.loadingWindow = null;
        //    //}

        //    //this.InvalidateVisual();
        //    this.RedrawCameraDataUI();

        //    //Refresh를 하는 동안 Refresh 명령이 다시 온 경우 한번 더 실행해줌 !!
        //    //if (isReceiveRefreshMessage)
        //    //{
        //    //    //InnotiveDebug.Trace(3, "[blackRoot08] Refresh를 하는동안 명령이 한번 더 옴 !!!");

        //    //    isReceiveRefreshMessage = false;
        //    //    this.RefreshTimeline_BackgroundWorker();
        //    //}

        //    // 다 업데이트 후에, 만일 모든 영상들의 끝에 도달했다면 재생을 멈춘다.
        //    //if (App.PlaybackControl != null && App.PlaybackControl.xTimeNavigationControl != null)
        //    //{
        //    //    if (AllCamerasGetEnd())
        //    //    {
        //    //        //App.PlaybackControl.xTimeNavigationControl.Pause();
        //    //    }
        //    //    if (AllCamerasPastStart())
        //    //    {
        //    //        App.PlaybackControl.xTimeNavigationControl.Pause();
        //    //    }
        //    //}
        //}

        //public bool AllEntitiesGetEnd()
        //{
        //    for (var i = 0; i < timeLineControlRecordDatas.Count; i++)
        //    {
        //        var recordData = this.timeLineControlRecordDatas[i];
        //        if (recordData == null)
        //            continue;
        //        var recordInfo = recordData.GetRecordInfo(recordData.GetRecordInfoCount() - 1);
        //        if (recordInfo == null) 
        //            continue;
        //        if (this.ParentTimelineControl.CenterTimeUnixMS < new DateTimeOffset(recordInfo.EndTime).ToUnixTimeMilliseconds())
        //        {
        //            return false;
        //        }
        //    }
        //    return true;

        //    //for (var count = 0; count < this.timeLineControlData.GetRecordDataCount(); count++)
        //    //{
        //    //    var recordData = this.timeLineControlData.GetRecordDataAt(count);
        //    //    if (recordData == null)
        //    //        continue;
        //    //    var recordInfo = recordData.GetRecordInfo(recordData.GetRecordInfoCount() - 1);
        //    //    if (recordInfo == null) continue;
        //    //    if (this.timelineControl.CenterUnixTimeMiliSeconds <
        //    //            TimeUtils.ToUnixTimeMilliseconds(recordInfo.EndTime))
        //    //    {
        //    //        return false;
        //    //    }
        //    //}
        //    //return true;
        //}

        //public bool AllEntitiesPastStart()
        //{
        //    for (var i = 0; i < timeLineControlRecordDatas.Count; i++)
        //    {
        //        var recordData = this.timeLineControlRecordDatas[i];
        //        if (recordData == null)
        //            continue;
        //        var recordInfo = recordData.GetRecordInfo(0);
        //        if (recordInfo == null) continue;
        //        if (this.ParentTimelineControl.CenterTimeUnixMS > new DateTimeOffset(recordInfo.StartTime).ToUnixTimeMilliseconds())
        //        {
        //            return false;
        //        }
        //    }
        //    return true;

        //    //return Enumerable.Range(0, timeLineControlData.GetRecordDataCount())
        //    //    .Select(i => timeLineControlData.GetRecordDataAt(i))
        //    //    .Where(rd => rd != null)
        //    //    .All(rd =>
        //    //    {
        //    //        var recordInfo = rd.GetRecordInfo(0);
        //    //        return recordInfo == null || timelineControl.CenterUnixTimeMiliSeconds <= TimeUtils.ToUnixTimeMilliseconds(recordInfo.StartTime);
        //    //    });
        //    //for (var count = 0; count < this.timeLineControlData.GetRecordDataCount(); count++)
        //    //{
        //    //    var recordData = this.timeLineControlData.GetRecordDataAt(count);
        //    //    if (recordData == null)
        //    //        continue;
        //    //    var recordInfo = recordData.GetRecordInfo(0);
        //    //    if (recordInfo == null) continue;
        //    //    if (this.timelineControl.CenterUnixTimeMiliSeconds >
        //    //            TimeUtils.ToUnixTimeMilliseconds(recordInfo.StartTime))
        //    //    {
        //    //        return false;
        //    //    }
        //    //}
        //    //return true;
        //}

        //public void RefreshTimeline_BackgroundWorker()
        //{
        //    //최초 Application이 뜰때 한번만 실행해줌 !!
        //    //if (this.loadingWindow != null)
        //    //{
        //    //    this.loadingWindow.ShowWindow();
        //    //}

        //    if (isRunBackgroundWorker)
        //    {
        //        //현재 하고 있는 Refresh가 끝난후 다시 Refresh를 해줌 !!
        //        isReceiveRefreshMessage = true;
        //        return;
        //    }

        //    //this.RedrawTimelineUI();

        //    //InnotiveDebug.Trace(2, "[blackRoot08] Request 시작");

        //    isRunBackgroundWorker = true;
        //    //if (!backgroundWorker.IsBusy)
        //    //    backgroundWorker.RunWorkerAsync();
        //}

        public void ClearAllRecordData()
        {
            foreach (PlaybackTimelineRecordData recordData in this.timelineRecords)
            {
                recordData.ClearRecordData();
            }
        }

        public void ClearAllEventData()
        {
            foreach (PlaybackTimelineRecordData recordData in this.timelineRecords)
            {
                recordData.ClearEventData();
            }
        }

        public void ClearTimeline()
        {
            ClearAllRecordData();
            ClearAllEventData();
        }

        //public void ShowEventTooltip(Point mousePointRelativeInnerCanvas)
        //{
        //    ////1시간보다 큰 경우 Tooltip을 보여주지 않음 !!
        //    //if (this.timelineControl.VisibleTotalUnixTimeMiliSeconds > 60 * 60 * 1000)
        //    //    return;

        //    //TimelineControlEventInfo findEventInfo = null;
        //    //double findEventLeft = -100;
        //    //double findEventTop = -100;

        //    //for (int i = 0; i < this.timelineControlTotalData.GetCameraDataCount(); i++)
        //    //{
        //    //    TimelineControlCameraData cameraData = this.timelineControlTotalData.GetCameraData(i);
        //    //    if (cameraData == null)
        //    //        continue;

        //    //    for (int j = 0; j < cameraData.GetEventInfoCount(); j++)
        //    //    {
        //    //        TimelineControlEventInfo eventInfo = cameraData.GetEventInfo(j);
        //    //        if (eventInfo == null)
        //    //            continue;

        //    //        var top = this.GetCameraTop(i);
        //    //        var left = this.GetPosition(
        //    //            this.timelineControl.CenterUnixTimeMiliSeconds,
        //    //            TimeConverter.ConvertLocalDateTimeToUnixTimeMiliSeconds(eventInfo.EventBeginTime));

        //    //        double eventDrawWidth = eventInfo.DrawWidth;

        //    //        if ((left - eventDrawWidth <= (int)mousePointRelativeInnerCanvas.X &&
        //    //            left + eventDrawWidth + 1 >= (int)mousePointRelativeInnerCanvas.X) &&
        //    //            (mousePointRelativeInnerCanvas.Y >= top &&
        //    //            mousePointRelativeInnerCanvas.Y <= top + this.drawCameraHeight))
        //    //        {
        //    //            findEventInfo = eventInfo;
        //    //            findEventLeft = left;
        //    //            findEventTop = top;
        //    //            break;
        //    //        }
        //    //    }

        //    //    if (findEventInfo != null)
        //    //        break;
        //    //}

        //    //if (findEventInfo != null)
        //    //{
        //    //    InnotiveDebug.Trace(2, "[blackRoot93] Bingo !! EventDescription = {0}", findEventInfo.EventDescription);
        //    //    //double left = this.GetPosition(
        //    //    //this.timelineControl.CenterUnixTimeMiliSeconds, 
        //    //    //TimeConverter.ConvertLocalDateTimeToUnixTimeMiliSeconds(findEventInfo.EventDateTime));

        //    //    //Event Group인 경우 Count를 표시함
        //    //    if (findEventInfo.MergeEventCount > 1)
        //    //    {
        //    //        this.timelineControl.xTextBlockEventInfo.Text = " Event Count : " + findEventInfo.MergeEventCount.ToString() + " ";
        //    //    }
        //    //    else
        //    //    {
        //    //        this.timelineControl.xTextBlockEventInfo.Text = " " + findEventInfo.EventDescription + " ";
        //    //    }

        //    //    Canvas.SetLeft(this.timelineControl.xGridEventInfo, findEventLeft);
        //    //    Canvas.SetTop(this.timelineControl.xGridEventInfo, findEventTop);
        //    //    this.timelineControl.xGridEventInfo.Visibility = System.Windows.Visibility.Visible;

        //    //    //Timeline Panning시 Tooltip 좌표를 이동하기 위해서 Tooltip이 보여질때의 좌표를 저장함
        //    //    this.lastVisibleTooltipX = findEventLeft;
        //    //}
        //    //else
        //    //{
        //    //    this.timelineControl.xTextBlockEventInfo.Text = string.Empty;
        //    //    this.timelineControl.xGridEventInfo.Visibility = System.Windows.Visibility.Collapsed;
        //    //}
        //}

        //public void MoveEventTooltip(double gapX)
        //{
        //    Canvas.SetLeft(this.ParentTimelineControl.xGridEventInfo, this.lastVisibleTooltipX + gapX);
        //}

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            //실제 Camera Data 그림
            try
            {
                this.DrawRecordData(dc);
            }
            catch
            {
            }
        }

        //private string GetCameraID(string cameraNumber)
        //{
        //    string cameraID = string.Empty;
        //    if (constset.UseSampleData)
        //    {
        //        BaseCameraData baseCameraData = CameraManager.Instance.GetCameraDataFromCameraNumber(cameraNumber);
        //        if (baseCameraData != null)
        //        {
        //            cameraID = baseCameraData.CameraID;
        //        }
        //    }
        //    else
        //    {
        //        CameraInfo cameraInfo = CameraManager.Instance.GetCameraInfoFromCameraNumber(cameraNumber);
        //        if (cameraInfo != null)
        //        {
        //            cameraID = cameraInfo.CameraId;
        //        }
        //    }

        //    return cameraID;
        //}

        //private double GetEntityTop(int totalDataIndex)
        //{
        //    double entityTop = drawCameraStartY + (drawCameraHeight + drawCameraEmptySpace) * totalDataIndex;
        //    return entityTop;
        //}

        private void DrawRecordData(DrawingContext dc)
        {
            var recordDatas = this.timelineRecords;
            if (recordDatas == null || recordDatas.Count == 0)
                return;

            double canvasInnerHeight = drawCameraStartY +
                (drawCameraHeight + drawCameraEmptySpace) * recordDatas.Count - drawCameraEmptySpace;

            if (canvasInnerHeight < ParentTimelineControl.xScrollViewer.ViewportHeight)
                canvasInnerHeight = ParentTimelineControl.xScrollViewer.ViewportHeight;

            if (Math.Abs(ParentTimelineControl.xCanvasInner.Height - canvasInnerHeight) > 0.1)
                ParentTimelineControl.xCanvasInner.Height = canvasInnerHeight;

            long visibleTimeRangeMS = ParentTimelineControl.VisibleTimeRangeMS;
            long centerTimeUnixMS = ParentTimelineControl.CenterTimeUnixMS;
            long leftTimeUnixMS = centerTimeUnixMS - (visibleTimeRangeMS / 2);
            long rightTimeUnixMS = centerTimeUnixMS + (visibleTimeRangeMS / 2);

            for (int i = 0; i < recordDatas.Count; i++)
            {
                var recordData = recordDatas[i];
                if (recordData == null) continue;

                double entityTop = drawCameraStartY + i * (drawCameraHeight + drawCameraEmptySpace);

                // 배경
                dc.DrawRectangle(cameraBackBrush, null, new Rect(0, entityTop, this.Width, drawCameraHeight));

                // 녹화 구간
                var records = recordData.RecordInfos;
                for (int j = 0; j < records.Count; j++)
                {
                    var r = records[j];
                    if (r.EndTimeUnixMS < leftTimeUnixMS || r.StartTimeUnixMS > rightTimeUnixMS)
                        continue;
                    double left = GetPosition(centerTimeUnixMS, r.StartTimeUnixMS, visibleTimeRangeMS);
                    double right = GetPosition(centerTimeUnixMS, r.EndTimeUnixMS, visibleTimeRangeMS);
                    double width = Math.Max(0, right - left);

                    if (right > 0 && left < this.Width)
                        dc.DrawRectangle(cameraBrush, null, new Rect(left, entityTop, width, drawCameraHeight));
                }

                // 이벤트 구간
                var events = recordData.EventInfos;
                for (int k = 0; k < events.Count; k++)
                {
                    var ev = events[k];
                    if (ev.StartTimeUnixMS < leftTimeUnixMS || ev.EndTimeUnixMS > rightTimeUnixMS)
                        continue;

                    double left = GetPosition(centerTimeUnixMS, ev.StartTimeUnixMS, visibleTimeRangeMS);
                    double right = GetPosition(centerTimeUnixMS, ev.EndTimeUnixMS, visibleTimeRangeMS);
                    double width = Math.Max(5, right - left);

                    if (right > 0 && left < this.Width)
                        dc.DrawRectangle(ev.EventColor, null, new Rect(left, entityTop - ev.DrawHeightGap, width, drawCameraHeight + ev.DrawHeightGap * 2));
                }
            }
        }


        //private void DrawRecordData(DrawingContext dc)
        //{
        //    var canvasInnerHeight = drawCameraStartY +
        //        (drawCameraHeight + drawCameraEmptySpace) * this.timeLineControlRecordDatas.Count - drawCameraEmptySpace;

        //    // ScrollViewer보다 작으면 최소 높이 보장
        //    if (canvasInnerHeight < this.ParentTimelineControl.xScrollViewer.Height)
        //        canvasInnerHeight = this.ParentTimelineControl.xScrollViewer.Height;

        //    this.ParentTimelineControl.xCanvasInner.Height = canvasInnerHeight;

        //    for (int i = 0; i < this.timeLineControlRecordDatas.Count; i++)
        //    {
        //        var recordData = this.timeLineControlRecordDatas[i];
        //        if (recordData == null)
        //            continue;

        //        // entityTop 계산
        //        double entityTop = drawCameraStartY + i * (drawCameraHeight + drawCameraEmptySpace);

        //        //string entityName = string.IsNullOrEmpty(recordData.Entity.DisplayText) ? "No Name" : recordData.Entity.DisplayText;

        //        //if (recordData.IsSelected)
        //        //    dc.DrawRectangle(cameraSelectBrush, null, new Rect(0, entityTop - 3, this.Width, drawCameraHeight + 6));

        //        // 배경
        //        dc.DrawRectangle(cameraBackBrush, null, new Rect(0, entityTop, this.Width, drawCameraHeight));

        //        // 녹화 구간
        //        for (int j = 0; j < recordData.GetRecordInfoCount(); j++)
        //        {
        //            var recordInfo = recordData.GetRecordInfo(j);

        //            long startUnixMs = new DateTimeOffset(recordInfo.StartTime).ToUnixTimeMilliseconds();
        //            long endUnixMs = new DateTimeOffset(recordInfo.EndTime).ToUnixTimeMilliseconds();

        //            var left = this.GetPosition(this.ParentTimelineControl.CenterTimeUnixMS, startUnixMs);
        //            var right = this.GetPosition(this.ParentTimelineControl.CenterTimeUnixMS, endUnixMs);
        //            var width = Math.Max(0, right - left);

        //            if (right > 0 && left < this.Width)
        //                dc.DrawRectangle(this.cameraBrush, null, new Rect(left, entityTop, width, drawCameraHeight));
        //        }

        //        // 이벤트 구간
        //        for (int k = 0; k < recordData.GetEventInfoCount(); k++)
        //        {
        //            var eventInfo = recordData.GetEventInfo(k);
        //            if (eventInfo == null) continue;

        //            long eventStartUnixMs = new DateTimeOffset(eventInfo.EventBeginTime).ToUnixTimeMilliseconds();
        //            long eventEndUnixMs = new DateTimeOffset(eventInfo.EventEndTime).ToUnixTimeMilliseconds();

        //            var left = this.GetPosition(this.ParentTimelineControl.CenterTimeUnixMS, eventStartUnixMs);
        //            var right = this.GetPosition(this.ParentTimelineControl.CenterTimeUnixMS, eventEndUnixMs);
        //            var width = Math.Max(5, right - left); // 최소 5px

        //            if (right > 0 && left < this.Width)
        //                dc.DrawRectangle(eventInfo.EventColor, null, new Rect(left, entityTop - eventInfo.DrawHeightGap, width, drawCameraHeight + eventInfo.DrawHeightGap * 2));
        //        }


        //        // Camera Label
        //        //var formattedText = new FormattedText(entityName, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Verdana"), 12, Brushes.White)
        //        //{
        //        //    MaxTextWidth = 150,
        //        //    MaxTextHeight = drawCameraHeight,
        //        //    MaxLineCount = 1,
        //        //    Trimming = TextTrimming.CharacterEllipsis
        //        //};

        //        //dc.DrawRectangle(this.cameraLabelBackgroundBrush, this.cameraLabelEdgePen, new Rect(2, entityTop, formattedText.Width + 8, drawCameraHeight));
        //        //dc.DrawText(formattedText, new Point(5, entityTop));
        //    }
        //}

        //Center를 기준으로한 좌표를 구함
        private double GetPosition(long centerTimeUnixMS, long positionTimeUnixMS, double visibleTimeRangeMS)
        {
            double actualWidth = ParentTimelineControl.xTimeLineGrid.ActualWidth;
            double gapRatio = (double)(centerTimeUnixMS - positionTimeUnixMS) / visibleTimeRangeMS;

            return (actualWidth / 2) - (actualWidth * gapRatio);
        }

        //Record Data를 합치기 위해서 기준이 되는 초를 계산함 (Interval)
        private int GetRecordingMergeIntervalSeconds()
        {
            int recordMergeIntervalSeconds = 10;

            TimeSpan timeSpan = TimeSpan.FromMilliseconds(this.ParentTimelineControl.VisibleTimeRangeMS);

            if (timeSpan.TotalHours > 1 && timeSpan.TotalHours <= 6)
                recordMergeIntervalSeconds = 30;
            else if (timeSpan.TotalHours > 6 && timeSpan.TotalHours <= 12)
                recordMergeIntervalSeconds = 60;
            else if (timeSpan.TotalHours > 12)
                recordMergeIntervalSeconds = 90;

            //UI 넓이가 기준보다 작을 경우 Interval값을 더 크게 잡아줌
            //this.timelineControl.Dispatcher.Invoke(new Action(() =>
            //{
            //    if (!double.IsNaN(this.timelineControl.xCanvasInner.Width))
            //        recordMergeIntervalSeconds = (int)(recordMergeIntervalSeconds * (1920 / this.timelineControl.xCanvasInner.Width));
            //}), null);

            if (recordMergeIntervalSeconds < 1)
                recordMergeIntervalSeconds = 1;

            return recordMergeIntervalSeconds;
        }

        //Event Data를 합치기 위해서 기준이 되는 초를 계산함 (Interval)
        private int GetEventMergeIntervalSeconds()
        {
            //1시간 (1-19)
            int eventMergeIntervalSeconds = 10;

            TimeSpan timeSpan = TimeSpan.FromMilliseconds(this.ParentTimelineControl.VisibleTimeRangeMS);

            //2시간 (20-59)
            if (timeSpan.TotalHours > 1 && timeSpan.TotalHours <= 2)
                eventMergeIntervalSeconds = 40;
            //6시간 (60-119)
            else if (timeSpan.TotalHours > 2 && timeSpan.TotalHours <= 6)
                eventMergeIntervalSeconds = 80;
            //12시간 (120-239)
            else if (timeSpan.TotalHours > 6 && timeSpan.TotalHours <= 12)
                eventMergeIntervalSeconds = 160;
            //12시간 초과 (240이상)
            else if (timeSpan.TotalHours > 12)
                eventMergeIntervalSeconds = 320;

            //UI 넓이가 기준보다 작을 경우 Interval값을 더 크게 잡아줌
            //this.timelineControl.Dispatcher.Invoke(new Action(() =>
            //{
            //    if (!double.IsNaN(this.timelineControl.xCanvasInner.Width))
            //        eventMergeIntervalSeconds = (int)(eventMergeIntervalSeconds * (1920 / this.timelineControl.xCanvasInner.Width));

            //    InnotiveDebug.Trace(1, "[blackRoot36] canvas width = {0}", this.timelineControl.xCanvasInner.Width);

            //}), null);

            if (eventMergeIntervalSeconds < 1)
                eventMergeIntervalSeconds = 1;

            return eventMergeIntervalSeconds;
        }

        //public void RequestRecordData()
        //{
        //// Data Polling.
        //HttpRequest httpRequest = new HttpRequest();

        //TimeSpan timeSpan = TimeSpan.FromMilliseconds(this.timelineControl.VisibleTotalUnixTimeMiliSeconds);
        //double visibleMinutes = timeSpan.TotalMinutes;

        //////Monitor전체 넓이 / 현재 Canvas의 넓이를 구해서 Buffer의 시간을 구함
        ////Rect screenTotalRect = ScreenUtils.GetTotalScreenBound();
        ////double bufferRatio = 1.5;
        ////this.timelineControl.Dispatcher.Invoke(new Action(() =>
        ////{
        ////    bufferRatio = screenTotalRect.Width / this.timelineControl.xCanvasInner.Width;
        ////}), null);

        ////int gapMinutesLeftSide = (int)(timeSpan.TotalMinutes * bufferRatio);
        ////int gapMinutesRightSide = (int)(timeSpan.TotalMinutes * bufferRatio);

        //int gapMinutesLeftSide = (int)(timeSpan.TotalMinutes * constset.TimelineLeftBufferRatio);
        //int gapMinutesRightSide = (int)(timeSpan.TotalMinutes * constset.TimelineRightBufferRatio);

        ////Recording Interval을 가져옴
        //int recordMergeIntervalSeconds = this.GetRecordingMergeIntervalSeconds();

        ////Event Interval을 가져옴
        //int eventMergeIntervalSeconds = this.GetEventMergeIntervalSeconds();


        ////기준시간 앞뒤 30분씩의 Data를 가져옴 !!
        //var centerDateTime = TimeConverter.ConvertUnixTimeMiliSecondsToLocalDateTime(this.timelineControl.CenterUnixTimeMiliSeconds);
        //var beginDateTime = centerDateTime.Subtract(TimeSpan.FromMinutes(gapMinutesLeftSide));
        //var endDateTime = centerDateTime.AddMinutes(gapMinutesRightSide);

        //string strBeginTime = (Math.Truncate(TimeConverter.ConvertLocalDateTimeToUnixTimeMiliSeconds(beginDateTime) / 1000)).ToString();
        //string strEndTime = (Math.Truncate(TimeConverter.ConvertLocalDateTimeToUnixTimeMiliSeconds(endDateTime) / 1000)).ToString();

        ////InnotiveDebug.Trace(3, "[blackRoot06] Timeline.RequestCameraData() 서버로 전송한 시간값 : start time = {0}, end time = {1}", beginDateTime, endDateTime);

        //try
        //{
        //    var cameraNumberList = new List<RecordCameraInfo>();
        //    for (int i = 0; i < this.timelineControlTotalData.GetCameraDataCount(); i++)
        //    {
        //        var cameraData = this.timelineControlTotalData.GetCameraData(i);
        //        if (cameraData == null)
        //            continue;
        //        var temp = new RecordCameraInfo();
        //        temp.RecordCameraNumber = cameraData.CameraNumber;
        //        temp.RecorderIP = cameraData.RecorderIp;
        //        cameraNumberList.Add(temp);
        //    }

        //    var requestData = constset.IsShowTimelineEventUI ?
        //        new TimelineRequestData(
        //            strBeginTime,
        //            strEndTime,
        //            recordMergeIntervalSeconds.ToString(),
        //            eventMergeIntervalSeconds.ToString(),
        //            cameraNumberList) :
        //        new TimelineRequestData(
        //            strBeginTime,
        //            strEndTime,
        //            recordMergeIntervalSeconds.ToString(),
        //            string.Empty,
        //            cameraNumberList);

        //    var xmlData = requestData.SaveDataToXML();

        //    var param = new PollingParameter()
        //    {
        //        Url = string.Format("{0}{1}", PlayerCommonsConfig.Instance.DataServiceUrl, "GetTimelineForPlayback"),
        //        EncodingOption = "UTF8",
        //        PostMessage = xmlData
        //    };

        //    //InnotiveDebug.Trace(1,
        //    //    "[blackRoot08] begintime = {0}, endtime = {1}, recordInterval = {2}, eventInterval = {3}, param = {4}",
        //    //    strBeginTime, strEndTime, recordMergeIntervalSeconds, eventMergeIntervalSeconds, param.Url);

        //    var strResponse = httpRequest.Request(param, true);
        //    //InnotiveDebug.Trace(1, "[blackRoot07] response = {0}", strResponse);
        //    //InnotiveDebug.Trace(1, "[blackRoot08] response 받음");

        //    if (string.IsNullOrWhiteSpace(strResponse))
        //    {
        //        InnotiveDebug.Log.Info("Timeline Camera Data를 요청했지만 Response Data가 Empty임 !!");
        //        return;
        //    }

        //    var responseData = TimelineResponseData.ReadDataFromXML(strResponse);

        //    //this.timelineControlTotalData.AllCreatedCount = 0;

        //    //this.timelineControlTotalData.ClearAllEventData();
        //    //this.timelineControlTotalData.ClearAllRecordingData();

        //    //Data를 받아오는 시간이 길 경우 Data를 받아오는 과정중에 MultiGrid쪽 Camera가 삭제된 경우가 발생함 !! 
        //    // 이 시점에서 MultiGrid 내부 Camera의 ID List를 얻어옴 !!
        //    var currentMultiGridCameraIDList = this.GetMultiGridCameraIDList();

        //    //InnotiveDebug.Trace(2, "[blackRoot08] AddData 시작. RecordMediaInfo.Count = {0}", responseData.RecordMediaInfos.Count);
        //    foreach (RecordMediaInfo recordMediaInfo in responseData.RecordMediaInfos)
        //    {
        //        //InnotiveDebug.Trace(2, "[blackRoot08] AddData 시작. TimelineCameraInfos.Count = {0}", recordMediaInfo.TimelineCameraInfos.Count);
        //        foreach (TimelineCameraInfo cameraInfo in recordMediaInfo.TimelineCameraInfos)
        //        {
        //            //InnotiveDebug.Trace(2,
        //            //    "[blackRoot08] AddData 시작. camera id = {0}, record data count = {1}, event data count = {2}",
        //            //    cameraInfo.Id, cameraInfo.TimelineDateInfos.Count, cameraInfo.TimelineEventInfos.Count);

        //            var cameraNumber = cameraInfo.Id;
        //            var cameraID = this.GetCameraID(cameraNumber);

        //            if (string.IsNullOrWhiteSpace(cameraNumber) || string.IsNullOrWhiteSpace(cameraID))
        //                continue;

        //            //Data를 받아오는 시간이 길 경우 Data를 받아오는 과정중에 MultiGrid쪽 Camera가 삭제된 경우가 발생함 !!
        //            var hasItem = currentMultiGridCameraIDList.Any(tempCameraID => tempCameraID.ToUpper() == cameraID.ToUpper());

        //            //MultiGrid에 Camera가 존재하지 않을 경우 다음으로 넘어감 !!
        //            if (!hasItem)
        //                continue;

        //            foreach (TimelineDateInfo dateInfo in cameraInfo.TimelineDateInfos)
        //            {
        //                double tempBeginDate = 0;
        //                double tempEndDate = 0;
        //                if (!double.TryParse(dateInfo.BeginDate, out tempBeginDate))
        //                    continue;
        //                if (!double.TryParse(dateInfo.EndDate, out tempEndDate))
        //                    continue;

        //                //
        //                this.AddRecordingData(
        //                    cameraNumber,
        //                    tempBeginDate * 1000,
        //                    tempEndDate * 1000,
        //                    TimelineObjectType.Red,
        //                    recordMergeIntervalSeconds);
        //            }

        //            foreach (TimelineEventInfo eventInfo in cameraInfo.TimelineEventInfos)
        //            {
        //                double beginDate = 0;
        //                if (!double.TryParse(eventInfo.BeginDate, out beginDate))
        //                    continue;

        //                double endDate = 0;
        //                if (!double.TryParse(eventInfo.EndDate, out endDate))
        //                    continue;

        //                var color = this.brushConverter.ConvertFromString(eventInfo.EventColor) as SolidColorBrush;
        //                if (color == null)
        //                    continue;

        //                int mergeEventCount = 1;
        //                if (!int.TryParse(eventInfo.MergeEventCount, out mergeEventCount))
        //                    mergeEventCount = 1;

        //                this.AddEventData(cameraNumber, beginDate * 1000, endDate * 1000, eventInfo.EventType, color, mergeEventCount);
        //            }
        //        }
        //    }

        //    //InnotiveDebug.Trace(3, "[blackRoot08] 새로 생성된 Rectangle의 갯수 = {0}", this.timelineControlTotalData.AllCreatedCount);

        //    this.timelineControlTotalData.RemoveUnavailableRecordingInfo(beginDateTime, endDateTime);

        //    //InnotiveDebug.Trace(2, "[blackRoot04] AddData 종료");
        //}
        //catch (Exception ex)
        //{
        //    InnotiveDebug.Trace(2, "[blackRoot04] Timeline.RequestCameraData() Exception = {0}", ex.Message);
        //}
        //}

        //private List<string> GetMultiGridCameraIDList()
        //{
        //    List<string> result = null;

        //    if (Application.Current.Dispatcher.CheckAccess())
        //    {
        //        result = this.GetMultiGridCameraIDList_Internal();
        //    }
        //    else
        //    {
        //        Application.Current.Dispatcher.Invoke(new Action(() =>
        //        {
        //            result = this.GetMultiGridCameraIDList_Internal();
        //        }), null);
        //    }

        //    return result;
        //}

        //private List<string> GetMultiGridCameraIDList_Internal()
        //{
        //    List<string> result = new List<string>();

        //    List<CameraControlPlayback> cameraControlList = new List<CameraControlPlayback>();

        //    if (this.IsPreviewMode)
        //        cameraControlList.Add(this.PreviewCameraControl);
        //    else
        //        cameraControlList = App.PlaybackControl.xMultiGridControl.GetAllCameraControls();

        //    for (int i = 0; i < cameraControlList.Count; i++)
        //    {
        //        CameraControlPlayback cameraControl = cameraControlList[i];
        //        if (cameraControl != null)
        //        {
        //            result.Add(cameraControl.ID);
        //        }
        //    }

        //    return result;
        //}

        //private bool HasMultiGridCamera(string cameraNumber)
        //{
        //    bool result = false;

        //    if (Application.Current.Dispatcher.CheckAccess())
        //    {
        //        result = this.HasMultiGridCamera_Internal(cameraNumber);
        //    }
        //    else
        //    {
        //        Application.Current.Dispatcher.Invoke(new Action(() =>
        //        {
        //            result = this.HasMultiGridCamera_Internal(cameraNumber);
        //        }), null);
        //    }

        //    return result;
        //}

        //private bool HasMultiGridCamera_Internal(string cameraNumber)
        //{
        //    if (string.IsNullOrWhiteSpace(cameraNumber))
        //        return false;

        //    string cameraID = this.GetCameraID(cameraNumber);

        //    List<CameraControlPlayback> cameraControlList = App.PlaybackControl.xMultiGridControl.GetAllCameraControls();
        //    bool hasItem = false;
        //    for (int i = 0; i < cameraControlList.Count; i++)
        //    {
        //        CameraControlPlayback cameraControl = cameraControlList[i];
        //        if (cameraControl != null)
        //        {
        //            if (cameraControl.ID.ToUpper() == cameraID.ToUpper())
        //            {
        //                hasItem = true;
        //                break;
        //            }
        //        }
        //    }

        //    return hasItem;
        //}

        //private void AddSampleCameraData()
        //{
        //    this.timelineControl.Dispatcher.Invoke(new Action(() =>
        //    {
        //        this.AddSampleCameraData("1");
        //        this.AddSampleCameraData("2");
        //        this.AddSampleCameraData("3");
        //        this.AddSampleCameraData("4");
        //        this.AddSampleCameraData("5");
        //    }), null);
        //}

        //private void AddSampleCameraData(string cameraNumber)
        //{
        //    if (cameraNumber.ToUpper() == "1".ToUpper())
        //        this.AddSampleCameraData_Internal("1", -23, -5, TimelineObjectType.Red);
        //    else if (cameraNumber.ToUpper() == "2".ToUpper())
        //        this.AddSampleCameraData_Internal("2", -10, 20, TimelineObjectType.Red);
        //    else if (cameraNumber.ToUpper() == "3".ToUpper())
        //        this.AddSampleCameraData_Internal("3", -26, -8, TimelineObjectType.Yellow);
        //    else if (cameraNumber.ToUpper() == "4".ToUpper())
        //    {
        //        this.AddSampleCameraData_Internal("4", -15, -14.8, TimelineObjectType.Red);
        //        this.AddSampleCameraData_Internal("4", -14.6, -14.4, TimelineObjectType.Red);
        //        this.AddSampleCameraData_Internal("4", -14.2, -12.5, TimelineObjectType.Red);
        //        this.AddSampleCameraData_Internal("4", 22, 25, TimelineObjectType.Red);
        //    }
        //    else if (cameraNumber.ToUpper() == "5".ToUpper())
        //        this.AddSampleCameraData_Internal("5", -14, 27, TimelineObjectType.Blue);
        //    else
        //        this.timelineControlTotalData.AddRecordingData(cameraNumber);
        //}

        //private void AddSampleCameraData_Internal(
        //    string cameraNumber,
        //    double centerDistanceStartMinutes,
        //    double centerDistanceEndMinutes,
        //    TimelineObjectType type)
        //{
        //    //DateTime dateTime = DateTime.Now;
        //    DateTime dateTime = constset.StartCenterTime;

        //    DateTime startTime, endTime;

        //    if (centerDistanceStartMinutes < 0)
        //        startTime = dateTime.Subtract(TimeSpan.FromMinutes(-centerDistanceStartMinutes));
        //    else
        //        startTime = dateTime.AddMinutes(centerDistanceStartMinutes);

        //    if (centerDistanceEndMinutes < 0)
        //        endTime = dateTime.Subtract(TimeSpan.FromMinutes(-centerDistanceEndMinutes));
        //    else
        //        endTime = dateTime.AddMinutes(centerDistanceEndMinutes);

        //    var objectInfo = new TimelineControlRecordingInfo(startTime, endTime, type);

        //    this.timelineControlTotalData.AddRecordingData(cameraNumber, objectInfo);
        //}
    }
}

