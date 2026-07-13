namespace OPNX.UI.WPF.Controls
{
    public enum PlaybackTimelineRangeType
    {
        None,
        M5,
        M15,
        M30,
        H1,
        H3,
        H6,
        H12,
        H24,
        D3,
    }

    public sealed class PlaybackTimelineRangeItem
    {
        public PlaybackTimelineRangeType RangeType { get; init; }

        public string DisplayName { get; init; } = string.Empty;
    }
}

