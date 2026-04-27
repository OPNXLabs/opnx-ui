using OPNX.Lib.Data.ORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OPNX.UI.WPF.Controls.OpnxPlaybackTimeline
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

			//for (int i = 0; i < entityIDList.Count; i++)
			//{
			//    int entityID = entityIDList[i];

			//    TimeLineControlRecordData recordData = this.GetRecordData(entityID);
			//    if (recordData == null)
			//        continue;

			//    recordData.IsSelected = true;
			//}
		}

		public PlaybackTimelineRecordData? AddRecordData(IEntity entity)
		{
			if (entity == null)
				return null;

			// 리스트에서 해당 Entity 찾기
			var recordData = timelineRecordDatas
				.FirstOrDefault(r => r.Entity.ID == entity.ID);

			if (recordData == null)
			{
				// 새 RecordData 생성 후 추가
				recordData = new PlaybackTimelineRecordData(entity);
				timelineRecordDatas.Add(recordData);
			}

			return recordData;

			//TimeLineControlRecordData findRecordData = null;
			//for (int i = 0; i < this.timeLineControlRecordDatas.Count; i++)
			//{
			//    TimeLineControlRecordData recordData = this.timeLineControlRecordDatas[i];
			//    if (recordData == null)
			//        continue;

			//    if (recordData.Entity.ID == entity.ID)
			//    {
			//        findRecordData = recordData;
			//        break;
			//    }
			//}

			//if (findRecordData == null)
			//{
			//    TimeLineControlRecordData newRecordData = new TimeLineControlRecordData(entity);
			//    this.timeLineControlRecordDatas.Add(newRecordData);

			//    return newRecordData;
			//}

			//return findRecordData;
		}

		public void AddRecordData(IEntity entity, PlaybackTimelineRecordInfo recordInfo)
		{
			if (entity == null || recordInfo == null)
				return;

			// 리스트에서 해당 Entity 찾기
			var recordData = timelineRecordDatas
				.FirstOrDefault(r => r.Entity.ID == entity.ID);

			if (recordData != null)
			{
				// 존재하면 녹화 구간 추가
				recordData.AddRecordInfo(recordInfo);
			}
			else
			{
				// 새 RecordData 생성 후 추가
				var newRecordData = new PlaybackTimelineRecordData(entity);
				newRecordData.AddRecordInfo(recordInfo); // 기존 recordInfo 추가
				timelineRecordDatas.Add(newRecordData);
			}

			//TimeLineControlRecordData findRecordData = null;
			//for (int i = 0; i < this.timeLineControlRecordDatas.Count; i++)
			//{
			//    TimeLineControlRecordData recordData = this.timeLineControlRecordDatas[i];
			//    if (recordData == null)
			//        continue;

			//    if (recordData.Entity.ID == entity.ID)
			//    {
			//        findRecordData = recordData;
			//        break;
			//    }
			//}

			//if (findRecordData != null)
			//{
			//    findRecordData.AddRecordInfo(recordInfo);
			//}
			//else
			//{
			//    TimeLineControlRecordData newRecordData = new TimeLineControlRecordData(entity);
			//    newRecordData.AddRecordInfo(recordInfo);

			//    this.timeLineControlRecordDatas.Add(newRecordData);
			//}

			//this.Trace();
		}

		public void AddEventData(IEntity entity, PlaybackTimelineEventInfo eventInfo)
		{
			if (entity == null || eventInfo == null)
				return;

			// 리스트에서 해당 Entity 찾기
			var recordData = timelineRecordDatas
				.FirstOrDefault(r => r.Entity.ID == entity.ID);

			if (recordData != null)
			{
				// 존재하면 이벤트 추가
				recordData.AddEventInfo(eventInfo);
			}
			else
			{
				// 새 RecordData 생성 후 추가
				var newRecordData = new PlaybackTimelineRecordData(entity);
				newRecordData.AddEventInfo(eventInfo);
				timelineRecordDatas.Add(newRecordData);
			}
			//if (eventInfo == null)
			//    return;

			//if (entity == null)
			//    return;

			//TimeLineControlRecordData findRecordData = null;
			//for (int i = 0; i < this.timeLineControlRecordDatas.Count; i++)
			//{
			//    var recordData = this.timeLineControlRecordDatas[i];
			//    if (recordData == null)
			//        continue;

			//    if (recordData.Entity.ID == entity.ID)
			//    {
			//        findRecordData = recordData;
			//        break;
			//    }
			//}

			//if (findRecordData != null)
			//{
			//    findRecordData.AddEventInfo(eventInfo);
			//}
			//else
			//{
			//    var newCameraData = new TimeLineControlRecordData(entity);
			//    newCameraData.AddEventInfo(eventInfo);

			//    this.timeLineControlRecordDatas.Add(newCameraData);
			//}

			//this.Trace();
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

		//TRACE
		//private void Trace()
		//{
		//    for (int i = 0; i < this.timelineControlCameraDataList.Count; i++)
		//    {
		//        TimelineControlCameraData cameraData = this.timelineControlCameraDataList[i];
		//        if (cameraData == null)
		//            continue;

		//        InnotiveDebug.Trace(2, "[blackRoot05] camera number = {0}, object count = {1}", cameraData.CameraNumber, cameraData.GetRecordingInfoCount());
		//    }
		//}

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

			//this.Trace();
		}
	}
}

