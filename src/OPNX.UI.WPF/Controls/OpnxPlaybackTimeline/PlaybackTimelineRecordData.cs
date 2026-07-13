using OPNX.Lib.Data.ORM.Interfaces;

namespace OPNX.UI.WPF.Controls
{
    public class PlaybackTimelineRecordData(IEntity entity)
    {
        private readonly IEntity recordEntity = entity;

        private readonly List<PlaybackTimelineRecordInfo> recordInfos = [];

        private readonly List<PlaybackTimelineEventInfo> eventInfos = [];

        public bool IsSelected { get; set; } = false;

        public IEntity Entity => recordEntity;

        public IReadOnlyList<PlaybackTimelineRecordInfo> RecordInfos => recordInfos;
        public IReadOnlyList<PlaybackTimelineEventInfo> EventInfos => eventInfos;

        public void ClearRecordData()
        {
            this.recordInfos.Clear();
        }

        public void ClearEventData()
        {
            this.eventInfos.Clear();
        }

        public void AddEventInfo(PlaybackTimelineEventInfo eventInfo)
        {
            ArgumentNullException.ThrowIfNull(eventInfo);
            eventInfos.Add(eventInfo);
        }

        public void AddRecordInfo(PlaybackTimelineRecordInfo recordInfo, int mergeIntervalSeconds = 0)
        {
            ArgumentNullException.ThrowIfNull(recordInfo);

            // Preserve exact server boundaries. Gap-based merging is currently disabled.
            mergeIntervalSeconds = 0;

            var overlappingRecords = recordInfos.Where(current => HasOverlapTimeRecordingInfo(
                        recordInfo,
                        current,
                        mergeIntervalSeconds)).ToList();

            if (overlappingRecords.Count == 0)
            {
                recordInfos.Add(recordInfo);
                return;
            }

            long mergedStart = Math.Min(recordInfo.StartTimeUnixMS, overlappingRecords.Min(record => record.StartTimeUnixMS));

            long mergedEnd = Math.Max(recordInfo.EndTimeUnixMS, overlappingRecords.Max(record => record.EndTimeUnixMS));

            var mergedRecord = overlappingRecords[0];
            mergedRecord.StartTimeUnixMS = mergedStart;
            mergedRecord.EndTimeUnixMS = mergedEnd;

            for (int i = 1; i < overlappingRecords.Count; i++)
                recordInfos.Remove(overlappingRecords[i]);
        }

        private static bool HasOverlapTimeRecordingInfo(PlaybackTimelineRecordInfo first, PlaybackTimelineRecordInfo second, int mergeIntervalSeconds = 0)
        {
            long tolerance = mergeIntervalSeconds * 1000L;

            return first.StartTimeUnixMS <= second.EndTimeUnixMS + tolerance &&
                   second.StartTimeUnixMS <= first.EndTimeUnixMS + tolerance;
        }

        public void RemoveUnavailableRecordInfo(long startTimeUnixMS, long endTimeUnixMS)
        {
            recordInfos.RemoveAll(record => record.EndTimeUnixMS < startTimeUnixMS || record.StartTimeUnixMS > endTimeUnixMS);
        }

        public int GetRecordInfoCount() => this.recordInfos.Count;

        public PlaybackTimelineRecordInfo? GetRecordInfo(int index) => index >= 0 && index < recordInfos.Count ? recordInfos[index] : null;

        public int GetEventInfoCount() => this.eventInfos.Count;

        public PlaybackTimelineEventInfo? GetEventInfo(int index) => index >= 0 && index < eventInfos.Count ? eventInfos[index] : null;
    }
}

