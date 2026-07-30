using OPNX.Lib.Data.ORM.Interfaces;
using OPNX.UI.WPF.Utilities;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace OPNX.UI.WPF.Controls
{
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


        private readonly DispatcherTimer scrollTimer = new();

        private bool isListBoxWheelHooked = false;
        private bool suppressSelectionChangedEvent = false;
        #endregion

        #region Constructors
        public OpnxPlaybackTimeline()
        {
            InitializeComponent();

            SetValue(ForegroundProperty, Brushes.White);

            UpdateEntityListPadding();

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
        }

        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty CenterTimeUnixMSProperty = DependencyProperty.Register(
            nameof(CenterTimeUnixMS),
            typeof(long),
            typeof(OpnxPlaybackTimeline),
            new PropertyMetadata(0L, OnCenterTimeUnixMSChanged),
            IsValidUnixTimeMilliseconds);

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

        public static readonly DependencyProperty LeftTimeForegroundProperty = DependencyProperty.Register(nameof(LeftTimeForeground), typeof(Brush), typeof(OpnxPlaybackTimeline), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x8F, 0xA3, 0xB8))));
        public static readonly DependencyProperty CenterTimeForegroundProperty = DependencyProperty.Register(nameof(CenterTimeForeground), typeof(Brush), typeof(OpnxPlaybackTimeline), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xC9, 0x4A, 0x46))));
        public static readonly DependencyProperty RightTimeForegroundProperty = DependencyProperty.Register(nameof(RightTimeForeground), typeof(Brush), typeof(OpnxPlaybackTimeline), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x8F, 0xA3, 0xB8))));
        public static readonly DependencyProperty TimeTickBrushProperty = DependencyProperty.Register(nameof(TimeTickBrush), typeof(Brush), typeof(OpnxPlaybackTimeline), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x0E, 0x16, 0x21))));
        public static readonly DependencyProperty TimeTickThicknessProperty = DependencyProperty.Register(nameof(TimeTickThickness), typeof(double), typeof(OpnxPlaybackTimeline), new PropertyMetadata(1d), IsValidThickness);
        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush), typeof(OpnxPlaybackTimeline), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x0E, 0x16, 0x21))));
        public static readonly DependencyProperty SeparatorThicknessProperty = DependencyProperty.Register(nameof(SeparatorThickness), typeof(double), typeof(OpnxPlaybackTimeline), new PropertyMetadata(2d), IsValidThickness);
        public static readonly DependencyProperty TimelineRowHeightProperty = DependencyProperty.Register(nameof(TimelineRowHeight), typeof(double), typeof(OpnxPlaybackTimeline), new PropertyMetadata(26d, OnTimelineLayoutAppearanceChanged), IsValidPositiveDouble);
        public static readonly DependencyProperty RecordingBarHeightProperty = DependencyProperty.Register(nameof(RecordingBarHeight), typeof(double), typeof(OpnxPlaybackTimeline), new PropertyMetadata(16d, OnTimelineLayoutAppearanceChanged), IsValidPositiveDouble);
        public static readonly DependencyProperty TimelineTopOffsetProperty = DependencyProperty.Register(nameof(TimelineTopOffset), typeof(double), typeof(OpnxPlaybackTimeline), new PropertyMetadata(7d, OnTimelineLayoutAppearanceChanged), IsValidThickness);
        public static readonly DependencyProperty TimelineRowBackgroundProperty = DependencyProperty.Register(nameof(TimelineRowBackground), typeof(Brush), typeof(OpnxPlaybackTimeline), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x00, 0x00)), OnTimelineAppearanceChanged));
        public static readonly DependencyProperty RecordingBrushProperty = DependencyProperty.Register(nameof(RecordingBrush), typeof(Brush), typeof(OpnxPlaybackTimeline), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xFF, 0xC0, 0x00)), OnTimelineAppearanceChanged));
        public static readonly DependencyProperty SelectedTimelineRowBackgroundProperty = DependencyProperty.Register(nameof(SelectedTimelineRowBackground), typeof(Brush), typeof(OpnxPlaybackTimeline), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x73, 0xB2, 0xF2)), OnTimelineAppearanceChanged));

        public static readonly DependencyProperty SelectedRecordDataProperty = DependencyProperty.Register(
            nameof(SelectedRecordData),
            typeof(PlaybackTimelineRecordData),
            typeof(OpnxPlaybackTimeline),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedRecordDataChanged));
        #endregion

        #region Events
        public event PlaybackTimelineCenterTimeChangedEventHandler? CenterTimeChanged;
        public event PlaybackTimelineRangeChangedEventHandler? TimeRangeChanged;
        public event PlaybackTimelineRequestEventHandler? RequestTimeline;
        public event EventHandler<PlaybackTimelineSelectionChangedEventArgs>? SelectionChanged;
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

        public Brush LeftTimeForeground { get => (Brush)GetValue(LeftTimeForegroundProperty); set => SetValue(LeftTimeForegroundProperty, value); }
        public Brush CenterTimeForeground { get => (Brush)GetValue(CenterTimeForegroundProperty); set => SetValue(CenterTimeForegroundProperty, value); }
        public Brush RightTimeForeground { get => (Brush)GetValue(RightTimeForegroundProperty); set => SetValue(RightTimeForegroundProperty, value); }
        public Brush TimeTickBrush { get => (Brush)GetValue(TimeTickBrushProperty); set => SetValue(TimeTickBrushProperty, value); }
        public double TimeTickThickness { get => (double)GetValue(TimeTickThicknessProperty); set => SetValue(TimeTickThicknessProperty, value); }
        public Brush SeparatorBrush { get => (Brush)GetValue(SeparatorBrushProperty); set => SetValue(SeparatorBrushProperty, value); }
        public double SeparatorThickness { get => (double)GetValue(SeparatorThicknessProperty); set => SetValue(SeparatorThicknessProperty, value); }
        public double TimelineRowHeight { get => (double)GetValue(TimelineRowHeightProperty); set => SetValue(TimelineRowHeightProperty, value); }
        public double RecordingBarHeight { get => (double)GetValue(RecordingBarHeightProperty); set => SetValue(RecordingBarHeightProperty, value); }
        public double TimelineTopOffset { get => (double)GetValue(TimelineTopOffsetProperty); set => SetValue(TimelineTopOffsetProperty, value); }
        public Brush TimelineRowBackground { get => (Brush)GetValue(TimelineRowBackgroundProperty); set => SetValue(TimelineRowBackgroundProperty, value); }
        public Brush RecordingBrush { get => (Brush)GetValue(RecordingBrushProperty); set => SetValue(RecordingBrushProperty, value); }
        public Brush SelectedTimelineRowBackground { get => (Brush)GetValue(SelectedTimelineRowBackgroundProperty); set => SetValue(SelectedTimelineRowBackgroundProperty, value); }

        public PlaybackTimelineRecordData? SelectedRecordData
        {
            get => (PlaybackTimelineRecordData?)GetValue(SelectedRecordDataProperty);
            set => SetValue(SelectedRecordDataProperty, value);
        }

        public PlaybackTimelineHitResult? SelectedTimelineItem { get; private set; }

        public long VisibleTimeRangeMS { get; private set; }

        public ObservableCollection<PlaybackTimelineRecordData> TimelineRecords => xTimelineCanvas.TimelineRecords;
        #endregion

        #region Public Methods
        public void UpdateTimeline(long elapsedMilliseconds)
        {
            // User interaction owns the center time while dragging, so playback updates must not compete with it.
            if (isMouseDown || isMouseDrag)
                return;

            CenterTimeUnixMS += elapsedMilliseconds;
            RedrawTimelineUI();
        }
        public void AddRecordData(IEntityIdentity entity, long startUnixTimeMS, long endUnixTimeMS, PlaybackTimelineRecordingType recordingType)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(() => AddRecordData(entity, startUnixTimeMS, endUnixTimeMS, recordingType));
                return;
            }
            xTimelineCanvas.AddRecordData(entity, startUnixTimeMS, endUnixTimeMS, recordingType);
        }

        public void AddTimelineItem(IEntityIdentity entity)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(() => AddTimelineItem(entity));
                return;
            }
            xTimelineCanvas.AddTimelineItem(entity);
        }

        public void AddEventData(IEntityIdentity entity, long startUnixTimeMS, long endUnixTimeMS, string eventType, SolidColorBrush eventColor, int mergeEventCount)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(() => AddEventData(entity, startUnixTimeMS, endUnixTimeMS, eventType, eventColor, mergeEventCount));
                return;
            }
            xTimelineCanvas.AddEventData(entity, startUnixTimeMS, endUnixTimeMS, eventType, eventColor, mergeEventCount);
        }

        public PlaybackTimelineRecordData? GetRecordData(IEntityIdentity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            return xTimelineCanvas.GetRecordData(entity);
        }

        public PlaybackTimelineRecordData? GetRecordData(int entityID) =>
            xTimelineCanvas.GetRecordData(entityID);

        public void ClearRecordData()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(() => ClearRecordData());
                return;
            }
            SetCurrentValue(SelectedRecordDataProperty, null);
            xTimelineCanvas.ClearAllRecordData();
        }

        public void ClearEventData()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(() => ClearEventData());
                return;
            }
            SetCurrentValue(SelectedRecordDataProperty, null);
            xTimelineCanvas.ClearAllEventData();
        }

        public void ClearTimeline()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(() => ClearTimeline());
                return;
            }

            SetCurrentValue(SelectedRecordDataProperty, null);
            xTimelineCanvas.ClearTimeline();
        }

        public void RemoveTimelineItem(IEntityIdentity entity) => RemoveTimelineItem(entity.ID);

        public void RemoveTimelineItem(int entityID)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(() => RemoveTimelineItem(entityID));
                return;
            }

            PlaybackTimelineRecordData? recordData = xTimelineCanvas.GetRecordData(entityID);
            if (recordData == null)
                return;

            if (ReferenceEquals(SelectedRecordData, recordData))
                SetCurrentValue(SelectedRecordDataProperty, null);

            xTimelineCanvas.RemoveTimelineItem(entityID);
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

        private static bool IsValidUnixTimeMilliseconds(object value)
        {
            if (value is not long unixTimeMilliseconds)
                return false;

            try
            {
                DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        private static bool IsValidThickness(object value) => value is double thickness && double.IsFinite(thickness) && thickness >= 0;

        private static bool IsValidPositiveDouble(object value) => value is double number && double.IsFinite(number) && number > 0;

        private static void OnTimelineLayoutAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPlaybackTimeline control)
            {
                control.UpdateEntityListPadding();
                control.xTimelineCanvas.UpdateTimelineExtent();
                control.xTimelineCanvas.InvalidateVisual();
            }
        }

        private static void OnTimelineAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OpnxPlaybackTimeline control)
                control.xTimelineCanvas.InvalidateVisual();
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

        private static void OnSelectedRecordDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not OpnxPlaybackTimeline control)
                return;

            var selectedRecordData = e.NewValue as PlaybackTimelineRecordData;
            control.SelectedTimelineItem = selectedRecordData == null
                ? null
                : new PlaybackTimelineHitResult
                {
                    HitType = PlaybackTimelineHitType.Entity,
                    RecordData = selectedRecordData
                };
            control.xTimelineCanvas.InvalidateVisual();
            if (control.suppressSelectionChangedEvent)
                return;

            control.SelectionChanged?.Invoke(control, new PlaybackTimelineSelectionChangedEventArgs
            {
                SelectedRecordData = selectedRecordData,
                HitResult = control.SelectedTimelineItem
            });
        }

        private void SelectTimelineItem(PlaybackTimelineHitResult? hitResult)
        {
            var selectedRecordData = hitResult?.RecordData;

            if (!ReferenceEquals(SelectedRecordData, selectedRecordData))
            {
                suppressSelectionChangedEvent = true;
                try
                {
                    SetCurrentValue(SelectedRecordDataProperty, selectedRecordData);
                }
                finally
                {
                    suppressSelectionChangedEvent = false;
                }
            }

            SelectedTimelineItem = hitResult;
            xTimelineCanvas.InvalidateVisual();
            SelectionChanged?.Invoke(this, new PlaybackTimelineSelectionChangedEventArgs
            {
                SelectedRecordData = selectedRecordData,
                HitResult = hitResult
            });
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

            return [.. TimelineRangeItems.OfType<PlaybackTimelineRangeItem>()];
        }

        private void UpdateLeftPanelVisibility()
        {
            xLeftPanel.Visibility = IsLeftPanelVisible ? Visibility.Visible : Visibility.Collapsed;
            xLeftPanelColumn.Width = IsLeftPanelVisible ? new GridLength(150) : new GridLength(0);
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
        }

        private int GetLineIntervalMinutes()
        {
            // Round upward so adjacent labels never become denser than the visible range can support.
            double gapMinutes = this.VisibleTimeRangeMS / 10000 / 6 / 6;
            double tempGapMinutes = (double)(int)gapMinutes;

            if (gapMinutes != tempGapMinutes)
                gapMinutes = tempGapMinutes + 1;

            if (gapMinutes < 1)
                gapMinutes = 1;

            return (int)gapMinutes;
        }


        private DateTime GetLeftTimeNearestCenter()
        {
            DateTime centerDateTime = DateTimeOffset.FromUnixTimeMilliseconds(this.CenterTimeUnixMS).LocalDateTime;

            int gapMinutes = this.GetLineIntervalMinutes();

            double totalMinutes = (centerDateTime.Hour * 60) + centerDateTime.Minute;
            double leftMinutes = ((int)(totalMinutes / gapMinutes)) * gapMinutes;

            DateTime leftDateTime = centerDateTime.Subtract(TimeSpan.FromMinutes(totalMinutes - leftMinutes));

            return leftDateTime;
        }

        private double GetPosition(double centerUnixTime, double positionUnixTime)
        {
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

            xTextBlockVisibleStartDate.Text = startDT.ToString("yyyy. MM. dd. dddd", culture);
            xTextBlockVisibleCenterDate.Text = centerDT.ToString("yyyy. MM. dd. dddd", culture);
            xTextBlockVisibleEndDate.Text = endDT.ToString("yyyy. MM. dd. dddd", culture);

            xTextBlockVisibleStartTime.Text = startDT.ToString("tt hh:mm:ss.fff", culture);
            xTextBlockVisibleCenterTime.Text = centerDT.ToString("tt hh:mm:ss.fff", culture);
            xTextBlockVisibleEndTime.Text = endDT.ToString("tt hh:mm:ss.fff", culture);

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
                // Snap markers to whole pixels to keep one-pixel lines crisp during panning.
                int x = (int)Math.Round(GetPosition(centerUnixTime, new DateTimeOffset(lineTimes[i]).ToUnixTimeMilliseconds()));
                Canvas.SetLeft(lines[i], x);

                timeTexts[i].Text = $"{lineTimes[i]:HH mm}";
                Canvas.SetLeft(timeTexts[i], x - (int)(timeTexts[i].ActualWidth / 2));
            }

            int centerX = (int)Math.Round(GetPosition(centerUnixTime, centerUnixTime));
            Canvas.SetLeft(xRectangleLineCenter, centerX);
        }

        private void Refresh()
        {
            this.RedrawTimelineUI();
            this.xTimelineCanvas.InvalidateVisual();
        }

        private void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void HandleInnerCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.xCanvasInner.IsMouseCaptured)
                this.xCanvasInner.CaptureMouse();

            this.clickPoint = e.GetPosition(this.xCanvasInner);
            this.clickCenterUnixTime = this.CenterTimeUnixMS;
            this.isMouseDown = true;
            this.isMouseDrag = false;
        }

        private void HandleInnerCanvasMouseLeave(object sender, MouseEventArgs e)
        {
            this.xGridEventInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void HandleInnerCanvasMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (!this.isMouseDown)
            {
                return;
            }

            Point currentPoint = e.GetPosition(this.xCanvasInner);
            if (!isMouseDrag &&
                Math.Abs(currentPoint.X - clickPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPoint.Y - clickPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            if (!isMouseDrag)
            {
            }

            this.isMouseDrag = true;

            double gapX = currentPoint.X - this.clickPoint.X;

            long visibleTimeRangeMS = this.VisibleTimeRangeMS;
            double ratio = gapX / this.xCanvasInner.ActualWidth;
            long movedMs = (long)(visibleTimeRangeMS * ratio);
            this.CenterTimeUnixMS = this.clickCenterUnixTime - movedMs;

            this.Refresh();
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

            bool wasMouseDrag = this.isMouseDrag;
            this.isMouseDown = false;
            this.isMouseDrag = false;

            if (!wasMouseDrag)
            {
                Point hitPoint = e.GetPosition(xTimelineCanvas);
                SelectTimelineItem(xTimelineCanvas.HitTestTimeline(hitPoint));
            }

            OnRequestTimeline();
        }

        private void HandleOuterCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.xScrollViewer.Width = this.xCanvasOuter.ActualWidth;

            double timeCanvasTop = Canvas.GetTop(this.xCanvasTime);
            if (double.IsNaN(timeCanvasTop))
                timeCanvasTop = 0;

            double scrollViewerTop = timeCanvasTop + this.xCanvasTime.ActualHeight;
            Canvas.SetTop(this.xScrollViewer, scrollViewerTop);

            Point canvasOrigin = this.xCanvasOuter.TranslatePoint(new Point(), this.xLeftPanel);
            double timelineTop = canvasOrigin.Y + scrollViewerTop;
            if (double.IsFinite(timelineTop) && timelineTop >= 0)
                this.xLeftHeaderRow.Height = new GridLength(timelineTop);

            double height = this.xCanvasOuter.ActualHeight - scrollViewerTop;
            if (height < 0)
                height = 0;

            this.xScrollViewer.Height = height;

            this.xCanvasTime.Width = this.xCanvasOuter.ActualWidth;

            this.xRectangleLineCenter.Height = this.xCanvasOuter.ActualHeight;
        }

        private void UpdateEntityListPadding()
        {
            xEntityListBox.Padding = new Thickness(0, TimelineTopOffset, 0, 0);
        }

        private void HandleScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.xCanvasInner.Width = e.NewSize.Width;
            this.xCanvasInner.Height = e.NewSize.Height;
            this.xTimelineCanvas.UpdateTimelineExtent();

            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(this.xTimelineCanvas.UpdateTimelineExtent));
        }

        private void HandleInnerCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.xTimelineCanvas.Width = this.xCanvasInner.Width;
            this.xTimelineCanvas.Height = this.xCanvasInner.Height;
            this.xTimelineCanvas.UpdateTimelineExtent();

            this.Refresh();
        }

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
                return;
            }

            if (!scrollTimer.IsEnabled)
            {
                this.xScrollViewer.Focus();
                if (!this.xCanvasInner.IsMouseCaptured)
                    this.xCanvasInner.CaptureMouse();
            }
            else
            {
                scrollTimer.Stop();
            }
            scrollTimer.Start();

            double gapX = e.Delta / 2;

            long visibleTimeRangeMS = this.VisibleTimeRangeMS;

            long movedMs = (long)(visibleTimeRangeMS * gapX / this.xCanvasInner.ActualWidth);

            this.CenterTimeUnixMS -= movedMs;

            this.Refresh();
        }

        private void HandleScrollTimerTick(object? sender, EventArgs e)
        {
            scrollTimer.Stop();

            if (this.xCanvasInner.IsMouseCaptured)
            {
                this.xCanvasInner.ReleaseMouseCapture();
            }
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

