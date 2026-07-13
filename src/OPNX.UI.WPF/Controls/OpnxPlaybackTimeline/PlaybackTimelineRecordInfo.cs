namespace OPNX.UI.WPF.Controls
{
    public enum PlaybackTimelineRecordingType { Normal }

    public class PlaybackTimelineRecordInfo(long startTimeUnixMS, long endTimeUnixMS, PlaybackTimelineRecordingType recordingType)
    {
        public long StartTimeUnixMS { get; set; } = startTimeUnixMS;
        public long EndTimeUnixMS { get; set; } = endTimeUnixMS;
        public PlaybackTimelineRecordingType RecordingType { get; } = recordingType;
    }
}

