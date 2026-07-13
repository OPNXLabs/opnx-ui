using OPNX.Lib.Data.ORM.Interfaces;

namespace OPNX.UI.WPF.Controls
{
    public class PlaybackTimelineDataStore
    {
        private readonly List<PlaybackTimelineRecordData> timelineRecordDatas;

        public PlaybackTimelineDataStore()
        {
            this.timelineRecordDatas = [];
        }

        public void ClearAllRecordData()
        {
            foreach (PlaybackTimelineRecordData recordData in this.timelineRecordDatas)
            {
                recordData.ClearRecordData();
            }
        }

        public void ClearAllEventData()
        {
            foreach (PlaybackTimelineRecordData recordData in this.timelineRecordDatas)
            {
                recordData.ClearEventData();
            }
        }

        public void UnSelectAllEntity()
        {
            foreach (PlaybackTimelineRecordData recordData in this.timelineRecordDatas)
            {
                recordData?.IsSelected = false;
            }
        }

        public void SelectCamera(List<int> entityIDList)
        {
            if (entityIDList == null || entityIDList.Count == 0)
                return;

            foreach (var entityID in entityIDList)
            {
                var recordData = GetRecordData(entityID);
                recordData?.IsSelected = true;
            }
        }

        public PlaybackTimelineRecordData? AddRecordData(IEntity entity)
        {
            if (entity == null)
                return null;

            var recordData = timelineRecordDatas
                .FirstOrDefault(r => r.Entity.ID == entity.ID);

            if (recordData == null)
            {
                recordData = new PlaybackTimelineRecordData(entity);
                timelineRecordDatas.Add(recordData);
            }

            return recordData;
        }

        public void AddRecordData(IEntity entity, PlaybackTimelineRecordInfo recordInfo)
        {
            if (entity == null || recordInfo == null)
                return;

            var recordData = timelineRecordDatas
                .FirstOrDefault(r => r.Entity.ID == entity.ID);

            if (recordData != null)
            {
                recordData.AddRecordInfo(recordInfo);
            }
            else
            {
                var newRecordData = new PlaybackTimelineRecordData(entity);
                newRecordData.AddRecordInfo(recordInfo);
                timelineRecordDatas.Add(newRecordData);
            }
        }

        public void AddEventData(IEntity entity, PlaybackTimelineEventInfo eventInfo)
        {
            if (entity == null || eventInfo == null)
                return;

            var recordData = timelineRecordDatas
                .FirstOrDefault(r => r.Entity.ID == entity.ID);

            if (recordData != null)
            {
                recordData.AddEventInfo(eventInfo);
            }
            else
            {
                var newRecordData = new PlaybackTimelineRecordData(entity);
                newRecordData.AddEventInfo(eventInfo);
                timelineRecordDatas.Add(newRecordData);
            }
        }

        public void Remove(int entityID)
        {
            PlaybackTimelineRecordData? recordData = this.GetRecordData(entityID);
            if (recordData == null)
                return;

            recordData.ClearRecordData();
            recordData.ClearEventData();
            this.timelineRecordDatas.Remove(recordData);
        }

        public int GetRecordDataCount()
        {
            return this.timelineRecordDatas.Count;
        }

        public PlaybackTimelineRecordData? GetRecordDataAt(int index)
        {
            if (index < 0 || index >= this.timelineRecordDatas.Count)
                return null;

            return this.timelineRecordDatas[index];
        }

        public PlaybackTimelineRecordData? GetRecordData(int entityID)
        {
            foreach (PlaybackTimelineRecordData recordData in this.timelineRecordDatas)
            {
                if (recordData.Entity.ID == entityID)
                    return recordData;
            }

            return null;
        }

        public void RemoveUnavailableRecordingInfo(long startTimeUnixMS, long endTimeUnixMS)
        {
            foreach (PlaybackTimelineRecordData recordData in this.timelineRecordDatas)
            {
                recordData.RemoveUnavailableRecordInfo(startTimeUnixMS, endTimeUnixMS);
            }
        }
    }
}


