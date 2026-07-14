using OPNX.Lib.Data.ORM.Interfaces;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public partial class OpnxPlaybackTimelineCanvas : UserControl
    {
        #region Fields
        private readonly ObservableCollection<PlaybackTimelineRecordData> timelineRecords = [];
        private readonly Dictionary<int, EntityTextCacheEntry> entityTextCache = [];
        #endregion

        #region Constructors
        public OpnxPlaybackTimelineCanvas()
        {
            InitializeComponent();

            this.MouseMove += DrawPopup;
            this.MouseLeave += HidePopup;
            this.Loaded += (_, _) =>
            {
                RefreshTimelineLayout();
            };
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

        public PlaybackTimelineRecordData? AddTimelineItem(IEntityIdentity entity)
        {
            if (timelineRecords.Any(x => x.Entity.ID == entity.ID))
                return null;

            var newRecordData = new PlaybackTimelineRecordData(entity);

            timelineRecords.Add(newRecordData);

            RefreshTimelineLayout();

            return newRecordData;
        }

        public PlaybackTimelineRecordData? GetRecordData(IEntityIdentity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            return GetRecordData(entity.ID);
        }

        public PlaybackTimelineRecordData? GetRecordData(int entityID) =>
            timelineRecords.FirstOrDefault(x => x.Entity.ID == entityID);

        private static void OnParentTimelineControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is OpnxPlaybackTimeline timeline)
                timeline.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        }

        private PlaybackTimelineRecordData GetOrAddTimelineItem(IEntityIdentity entity)
        {
            var recordData = timelineRecords.FirstOrDefault(x => x.Entity.ID == entity.ID);
            if (recordData != null)
                return recordData;

            recordData = new PlaybackTimelineRecordData(entity);
            timelineRecords.Add(recordData);
            return recordData;
        }

        public void RemoveTimelineItem(int entityID)
        {
            var findRecordData = timelineRecords.FirstOrDefault(x => x.Entity.ID == entityID);
            if (findRecordData == null)
                return;
            findRecordData.ClearRecordData();
            findRecordData.ClearEventData();

            timelineRecords.Remove(findRecordData);
            entityTextCache.Remove(entityID);

            RefreshTimelineLayout();
        }

        public void AddRecordData(IEntityIdentity entity, long startUnixMs, long endUnixMs, PlaybackTimelineRecordingType recordingType)
        {
            var recordInfo = new PlaybackTimelineRecordInfo(startUnixMs, endUnixMs, recordingType);
            var findRecordData = GetOrAddTimelineItem(entity);

            findRecordData.AddRecordInfo(recordInfo);

            RefreshTimelineLayout();
        }

        public void AddEventData(IEntityIdentity entity, long startUnixMs, long endUnixMs, string eventType, SolidColorBrush eventColor, int mergeEventCount)
        {
            var startBeginDate = DateTimeOffset.FromUnixTimeMilliseconds(startUnixMs).UtcDateTime;

            var description = $"{eventType} ({startBeginDate})";

            var eventInfo = new PlaybackTimelineEventInfo(startUnixMs, endUnixMs, eventType, eventColor, description, mergeEventCount);
            var findRecordData = GetOrAddTimelineItem(entity);

            findRecordData.AddEventInfo(eventInfo);

            RefreshTimelineLayout();
        }

        public PlaybackTimelineHitResult? HitTestTimeline(Point point)
        {
            int recordDataIndex = GetRecordDataIndex(point.Y);
            if (recordDataIndex < 0)
                return null;

            var recordData = timelineRecords[recordDataIndex];
            double recordingBarTop = GetRecordingBarTop(recordDataIndex);

            for (int i = recordData.EventInfos.Count - 1; i >= 0; i--)
            {
                var eventInfo = recordData.EventInfos[i];
                if (GetEventRect(eventInfo, recordingBarTop).Contains(point))
                {
                    return new PlaybackTimelineHitResult
                    {
                        HitType = PlaybackTimelineHitType.Event,
                        RecordData = recordData,
                        EventInfo = eventInfo
                    };
                }
            }

            for (int i = recordData.RecordInfos.Count - 1; i >= 0; i--)
            {
                var recordInfo = recordData.RecordInfos[i];
                if (GetRecordRect(recordInfo, recordingBarTop).Contains(point))
                {
                    return new PlaybackTimelineHitResult
                    {
                        HitType = PlaybackTimelineHitType.Recording,
                        RecordData = recordData,
                        RecordInfo = recordInfo
                    };
                }
            }

            return new PlaybackTimelineHitResult
            {
                HitType = PlaybackTimelineHitType.Entity,
                RecordData = recordData
            };
        }
        #endregion

        private int GetRecordDataIndex(double y)
        {
            double relativeY = y - ParentTimelineControl.TimelineTopOffset;
            if (relativeY < 0)
                return -1;

            double rowStride = ParentTimelineControl.TimelineRowHeight;
            int index = (int)(relativeY / rowStride);
            if (index < 0 || index >= timelineRecords.Count)
                return -1;

            double rowTop = GetEntityTop(index);
            return y < rowTop + ParentTimelineControl.TimelineRowHeight ? index : -1;
        }

        private double GetEntityTop(int index) =>
            ParentTimelineControl.TimelineTopOffset + index * ParentTimelineControl.TimelineRowHeight;

        private double GetRecordingBarTop(int index) =>
            GetEntityTop(index) +
            (ParentTimelineControl.TimelineRowHeight - ParentTimelineControl.RecordingBarHeight) / 2;

        private Rect GetRecordRect(PlaybackTimelineRecordInfo recordInfo, double entityTop)
        {
            long visibleTimeRangeMS = ParentTimelineControl.VisibleTimeRangeMS;
            long centerTimeUnixMS = ParentTimelineControl.CenterTimeUnixMS;
            double left = GetPosition(centerTimeUnixMS, recordInfo.StartTimeUnixMS, visibleTimeRangeMS);
            double right = GetPosition(centerTimeUnixMS, recordInfo.EndTimeUnixMS, visibleTimeRangeMS);
            return new Rect(left, entityTop, Math.Max(0, right - left), ParentTimelineControl.RecordingBarHeight);
        }

        private Rect GetEventRect(PlaybackTimelineEventInfo eventInfo, double entityTop)
        {
            long visibleTimeRangeMS = ParentTimelineControl.VisibleTimeRangeMS;
            long centerTimeUnixMS = ParentTimelineControl.CenterTimeUnixMS;
            double left = GetPosition(centerTimeUnixMS, eventInfo.StartTimeUnixMS, visibleTimeRangeMS);
            double right = GetPosition(centerTimeUnixMS, eventInfo.EndTimeUnixMS, visibleTimeRangeMS);
            return new Rect(
                left,
                entityTop - eventInfo.DrawHeightGap,
                Math.Max(5, right - left),
                ParentTimelineControl.RecordingBarHeight + eventInfo.DrawHeightGap * 2);
        }

        private void RefreshTimelineVisual()
        {
            InvalidateVisual();
        }

        private void RefreshTimelineLayout()
        {
            UpdateTimelineExtent();
            InvalidateVisual();
        }

        public void UpdateTimelineExtent()
        {
            if (ParentTimelineControl == null)
                return;

            double contentHeight = ParentTimelineControl.TimelineTopOffset +
                ParentTimelineControl.TimelineRowHeight * timelineRecords.Count;

            double actualHeight = ParentTimelineControl.xScrollViewer.ActualHeight;
            double viewportHeight = ParentTimelineControl.xScrollViewer.ViewportHeight;
            double availableHeight = 0;

            if (double.IsFinite(actualHeight) && actualHeight > 0)
                availableHeight = actualHeight;

            if (double.IsFinite(viewportHeight) && viewportHeight > 0)
                availableHeight = Math.Max(availableHeight, viewportHeight);

            double desiredHeight = Math.Max(contentHeight, availableHeight);

            if (desiredHeight > 0)
            {
                if (Math.Abs(ParentTimelineControl.xCanvasInner.Height - desiredHeight) > 0.1)
                    ParentTimelineControl.xCanvasInner.Height = desiredHeight;

                if (Math.Abs(Height - desiredHeight) > 0.1)
                    Height = desiredHeight;
            }

            double desiredWidth = ParentTimelineControl.xCanvasInner.ActualWidth;
            if (double.IsNaN(desiredWidth) || desiredWidth <= 0)
                desiredWidth = ParentTimelineControl.xScrollViewer.ViewportWidth;
            if (double.IsNaN(desiredWidth) || desiredWidth <= 0)
                desiredWidth = ParentTimelineControl.xScrollViewer.ActualWidth;
            if (double.IsNaN(desiredWidth) || desiredWidth <= 0)
                desiredWidth = ParentTimelineControl.xTimeLineGrid.ActualWidth;

            if (!double.IsNaN(desiredWidth) && desiredWidth > 0 && Math.Abs(Width - desiredWidth) > 0.1)
                Width = desiredWidth;
        }

        private void HidePopup(object sender, MouseEventArgs e)
        {
        }

        private void DrawPopup(object sender, MouseEventArgs e)
        {
        }

        public void ClearAllRecordData()
        {
            foreach (PlaybackTimelineRecordData recordData in this.timelineRecords)
            {
                recordData.ClearRecordData();
            }

            RefreshTimelineVisual();
        }

        public void ClearAllEventData()
        {
            foreach (PlaybackTimelineRecordData recordData in this.timelineRecords)
            {
                recordData.ClearEventData();
            }

            RefreshTimelineVisual();
        }

        public void ClearTimeline()
        {
            foreach (PlaybackTimelineRecordData recordData in this.timelineRecords)
            {
                recordData.ClearRecordData();
                recordData.ClearEventData();
            }

            RefreshTimelineVisual();
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            try
            {
                DrawRecordData(dc);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"[OpnxPlaybackTimelineCanvas] OnRender failed: {exception}");
                throw;
            }
        }
        private void DrawRecordData(DrawingContext dc)
        {
            var recordDatas = this.timelineRecords;
            if (recordDatas == null || recordDatas.Count == 0)
                return;

            long visibleTimeRangeMS = ParentTimelineControl.VisibleTimeRangeMS;
            long centerTimeUnixMS = ParentTimelineControl.CenterTimeUnixMS;
            long leftTimeUnixMS = centerTimeUnixMS - (visibleTimeRangeMS / 2);
            long rightTimeUnixMS = centerTimeUnixMS + (visibleTimeRangeMS / 2);

            for (int i = 0; i < recordDatas.Count; i++)
            {
                var recordData = recordDatas[i];
                if (recordData == null) continue;

                double recordingBarTop = GetRecordingBarTop(i);

                bool isSelected = ReferenceEquals(recordData, ParentTimelineControl.SelectedRecordData);
                dc.DrawRectangle(
                    isSelected ? ParentTimelineControl.SelectedTimelineRowBackground : ParentTimelineControl.TimelineRowBackground,
                    null,
                    new Rect(0, recordingBarTop, this.Width, ParentTimelineControl.RecordingBarHeight));

                var records = recordData.RecordInfos;
                for (int j = 0; j < records.Count; j++)
                {
                    var r = records[j];
                    if (r.EndTimeUnixMS < leftTimeUnixMS || r.StartTimeUnixMS > rightTimeUnixMS)
                        continue;
                    Rect recordRect = GetRecordRect(r, recordingBarTop);

                    if (recordRect.Right > 0 && recordRect.Left < this.Width)
                        dc.DrawRectangle(ParentTimelineControl.RecordingBrush, null, recordRect);
                }

                var events = recordData.EventInfos;
                for (int k = 0; k < events.Count; k++)
                {
                    var ev = events[k];
                    if (ev.EndTimeUnixMS < leftTimeUnixMS || ev.StartTimeUnixMS > rightTimeUnixMS)
                        continue;

                    Rect eventRect = GetEventRect(ev, recordingBarTop);

                    if (eventRect.Right > 0 && eventRect.Left < this.Width)
                        dc.DrawRectangle(ev.EventColor, null, eventRect);
                }

                if (!ParentTimelineControl.ShowEntityNameOnTimeline)
                    continue;

                FormattedText formattedText = GetEntityFormattedText(recordData);
                dc.DrawText(formattedText, new Point(5, recordingBarTop));
            }
        }

        private FormattedText GetEntityFormattedText(PlaybackTimelineRecordData recordData)
        {
            string text = string.IsNullOrEmpty(recordData.Entity.DisplayText) ? "No Name" : recordData.Entity.DisplayText;
            CultureInfo culture = CultureInfo.CurrentUICulture;
            double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            int entityID = recordData.Entity.ID;
            Typeface typeface = new(
                ParentTimelineControl.FontFamily,
                ParentTimelineControl.FontStyle,
                ParentTimelineControl.FontWeight,
                ParentTimelineControl.FontStretch);
            double fontSize = ParentTimelineControl.FontSize;
            Brush foreground = ParentTimelineControl.Foreground;

            if (entityTextCache.TryGetValue(entityID, out var cached) &&
                cached.Text == text &&
                cached.CultureName == culture.Name &&
                cached.PixelsPerDip.Equals(pixelsPerDip) &&
                cached.Typeface.Equals(typeface) &&
                cached.FontSize.Equals(fontSize) &&
                Equals(cached.Foreground, foreground))
            {
                return cached.FormattedText;
            }

            var formattedText = new FormattedText(
                text,
                culture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                foreground,
                pixelsPerDip)
            {
                MaxTextWidth = 150,
                MaxTextHeight = ParentTimelineControl.RecordingBarHeight,
                MaxLineCount = 1,
                Trimming = TextTrimming.CharacterEllipsis
            };

            entityTextCache[entityID] = new EntityTextCacheEntry(
                text,
                culture.Name,
                pixelsPerDip,
                typeface,
                fontSize,
                foreground,
                formattedText);
            return formattedText;
        }

        private double GetPosition(long centerTimeUnixMS, long positionTimeUnixMS, double visibleTimeRangeMS)
        {
            double actualWidth = ParentTimelineControl.xTimeLineGrid.ActualWidth;
            double gapRatio = (double)(centerTimeUnixMS - positionTimeUnixMS) / visibleTimeRangeMS;

            return (actualWidth / 2) - (actualWidth * gapRatio);
        }

        private sealed record EntityTextCacheEntry(
            string Text,
            string CultureName,
            double PixelsPerDip,
            Typeface Typeface,
            double FontSize,
            Brush Foreground,
            FormattedText FormattedText);
    }
}

