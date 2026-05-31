using System;
using System.Collections.Generic;
using System.Text;

namespace OPNX.UI.WPF.Controls
{
    public enum PlaybackTimelineRecordingType { Normal }

    public class PlaybackTimelineRecordInfo(long startTimeUnixMS, long endTimeUnixMS, PlaybackTimelineRecordingType recordingType)
    {
        public long StartTimeUnixMS { get; set; } = startTimeUnixMS;
        public long EndTimeUnixMS { get; set; } = endTimeUnixMS;
        public PlaybackTimelineRecordingType RecordingType { get; } = recordingType;

        //public static string GetColor(PlaybackTimelineRecordingType type)
        //{
        //    // 타입별 색상 매핑 (추후 필요 시 switch 추가)
        //    return "#7FFF0000";
        //}
    }
}

