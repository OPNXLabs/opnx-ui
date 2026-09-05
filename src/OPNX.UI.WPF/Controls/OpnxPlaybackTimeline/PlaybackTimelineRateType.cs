namespace OPNX.UI.WPF.Controls
{
    public enum PlaybackTimelineRateType
    {
        X025,
        X05,
        X1,
        X2,
        X4,
        X8,
    }

    public sealed class PlaybackTimelineRateItem
    {
        public PlaybackTimelineRateType RateType { get; init; }

        public string DisplayName { get; init; } = string.Empty;
    }
}
