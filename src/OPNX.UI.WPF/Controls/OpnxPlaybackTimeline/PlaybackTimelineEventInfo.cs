using System.Windows.Media;

namespace OPNX.UI.WPF.Controls
{
    public class PlaybackTimelineEventInfo(
        long startTimeUnixMS,
        long endTimeUnixMS,
        string eventType,
        SolidColorBrush eventColor,
        string eventDescription,
        int mergeEventCount = 1)
    {
        public long StartTimeUnixMS { get; set; } = startTimeUnixMS;
        public long EndTimeUnixMS { get; set; } = endTimeUnixMS;
        public string EventType { get; set; } = eventType;
        public SolidColorBrush EventColor { get; set; } = eventColor;
        public string EventDescription { get; set; } = eventDescription;
        public int MergeEventCount { get; set; } = mergeEventCount;

        public int DrawWidth { get; set; } = 1;
        public int DrawHeightGap { get; set; } = 0;
    }
}

