namespace OPNX.UI.WPF.Controls
{
    public enum PlaybackTimelineHitType
    {
        None,
        Entity,
        Recording,
        Event
    }

    public sealed class PlaybackTimelineHitResult
    {
        public PlaybackTimelineHitType HitType { get; init; }
        public PlaybackTimelineRecordData? RecordData { get; init; }
        public PlaybackTimelineRecordInfo? RecordInfo { get; init; }
        public PlaybackTimelineEventInfo? EventInfo { get; init; }
    }
}
