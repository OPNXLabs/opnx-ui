namespace OPNX.UI.WPF.Controls
{
    public delegate void PlaybackTimelineRequestEventHandler(object sender, PlaybackTimelineRequestEventArgs e);
    public class PlaybackTimelineRequestEventArgs : EventArgs
    {
        public long CenterTimeUnixMS { get; set; }
        public long VisibleTimeRangeMS { get; set; }
    }

    public delegate void PlaybackTimelineCenterTimeChangedEventHandler(object sender, PlaybackTimelineCenterTimeChangedEventArgs e);
    public class PlaybackTimelineCenterTimeChangedEventArgs : EventArgs
    {
        public long CenterTimeUnixMS { get; set; }
    }

    public delegate void PlaybackTimelineRangeChangedEventHandler(object sender, PlaybackTimelineRangeChangedEventArgs e);
    public class PlaybackTimelineRangeChangedEventArgs : EventArgs
    {
        public PlaybackTimelineRangeType TimeRange { get; set; }
    }

    public sealed class PlaybackTimelineSelectionChangedEventArgs : EventArgs
    {
        public PlaybackTimelineRecordData? SelectedRecordData { get; init; }
        public PlaybackTimelineHitResult? HitResult { get; init; }
    }
}

