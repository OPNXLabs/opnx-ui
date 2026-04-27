using OPNX.Lib.Data.ORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OPNX.UI.WPF.Controls.OpnxPlaybackTimeline
{
    public class PlaybackTimelineRecordData(IEntity entity)
    {
        private readonly IEntity recordEntity = entity;

        //저장 정보 (저장이 끊어진 단위를 기준으로 사각형으로 그릴수 있는 묶음)
        private readonly List<PlaybackTimelineRecordInfo> recordInfos = [];

        //Event 발생 시간 정보
        private readonly List<PlaybackTimelineEventInfo> eventInfos = [];

        private readonly static Lock recordingLock = new();
        private readonly static Lock eventLock = new();

        //선택된 Camera의 Background를 표시하기 위함
        public bool IsSelected { get; set; } = false;

        public IEntity Entity => recordEntity;

        //public TimeLineControlRecordData
        //{
        //    this.entity = entity;
        //    this.IsSelected = false;

        //    //this.AddTempEventInfo(200, 200, 1);
        //}


        public List<PlaybackTimelineRecordInfo> RecordInfos => recordInfos;
        public List<PlaybackTimelineEventInfo> EventInfos => eventInfos;


        //private void AddTempEventInfo(int leftCount, int rightCount, int intervalSeconds)
        //{
        //    lock (eventLock)
        //    {
        //        for (int i = 0; i < leftCount; i++)
        //        {
        //            TimelineControlEventInfo info = new TimelineControlEventInfo(DateTime.Now.Subtract(TimeSpan.FromSeconds(intervalSeconds * i)), "type", Brushes.Red, this.CameraNumber.ToString() + "_Desc_L" + i.ToString());
        //            this.timelineControlEventInfoList.Add(info);
        //        }

        //        for (int i = 0; i < rightCount; i++)
        //        {
        //            TimelineControlEventInfo info = new TimelineControlEventInfo(DateTime.Now.Add(TimeSpan.FromSeconds(intervalSeconds * i)), "type", Brushes.Yellow, this.CameraNumber.ToString() + "_Desc_R" + i.ToString());
        //            this.timelineControlEventInfoList.Add(info);
        //        }
        //    }
        //}

        public void ClearRecordData()
        {
            lock (recordingLock)
            {
                this.recordInfos.Clear();
            }
        }

        public void ClearEventData()
        {
            lock (eventLock)
            {
                this.eventInfos.Clear();
            }
        }

        public void AddEventInfo(PlaybackTimelineEventInfo eventInfo)
        {
            lock (eventLock)
            {
                eventInfos.Add(eventInfo);
            }
        }

        /// <summary>
        /// 새로운 녹화 구간을 타임라인에 추가.
        /// 기존 구간과 시간이 겹치면 병합 처리.
        /// </summary>
        /// <param name="recordInfo">추가할 녹화 구간 정보</param>
        /// <param name="mergeIntervalSeconds">지정된 초보다 작은 시간 간격은 병합 (0이면 무시)</param>
        public void AddRecordInfo(PlaybackTimelineRecordInfo recordInfo, int mergeIntervalSeconds = 0)
        {
            if (recordInfo == null)
                return;

            lock (recordingLock)
            {
                // 서버에서 이미 병합 처리된 경우 mergeIntervalSeconds 무시
                mergeIntervalSeconds = 0;

                var overlapList = new List<PlaybackTimelineRecordInfo>();

                // 1. 기존 구간과 겹치는지 확인
                foreach (var current in recordInfos)
                {
                    if (HasOverlapTimeRecordingInfo(recordInfo, current, mergeIntervalSeconds))
                    {
                        // 겹치는 경우, 시작/종료 시간을 확장
                        if (recordInfo.StartTimeUnixMS < current.StartTimeUnixMS)
                            current.StartTimeUnixMS = recordInfo.StartTimeUnixMS;

                        if (recordInfo.EndTimeUnixMS > current.EndTimeUnixMS)
                            current.EndTimeUnixMS = recordInfo.EndTimeUnixMS;

                        overlapList.Add(current);
                    }
                }

                // 2. 겹치는 구간이 없으면 새로 추가
                if (overlapList.Count == 0)
                {
                    recordInfos.Add(recordInfo);
                    return;
                }

                // 3. 겹치는 구간이 2개 이상이면 하나로 병합
                if (overlapList.Count > 1)
                {
                    // 최소 시작 시간과 최대 종료 시간 계산
                    long minStart = overlapList.Min(r => r.StartTimeUnixMS);
                    long maxEnd = overlapList.Max(r => r.EndTimeUnixMS);

                    // 첫 번째 구간을 병합 구간으로 업데이트
                    overlapList[0].StartTimeUnixMS = minStart;
                    overlapList[0].EndTimeUnixMS = maxEnd;

                    // 나머지 겹치는 구간 삭제
                    for (int i = 1; i < overlapList.Count; i++)
                        recordInfos.Remove(overlapList[i]);
                }
            }
        }

        private static bool HasOverlapTimeRecordingInfo(PlaybackTimelineRecordInfo first, PlaybackTimelineRecordInfo second, int mergeIntervalSeconds = 0)
        {
            long tolerance = mergeIntervalSeconds * 1000L;

            return first.StartTimeUnixMS <= second.EndTimeUnixMS + tolerance &&
                   second.StartTimeUnixMS <= first.EndTimeUnixMS + tolerance;

            //if (firstRecordInfo.StartTime <= secondRecordInfo.StartTime) 
            //{
            //    if (firstRecordInfo.EndTime.AddMinutes(mergeIntervalSeconds) >= secondRecordInfo.StartTime) return true;
            //} 
            //else 
            //{ 
            //    if (firstRecordInfo.StartTime <= secondRecordInfo.EndTime.AddMinutes(mergeIntervalSeconds))
            //        return true; 
            //}
            //if (firstRecordInfo.EndTime >= secondRecordInfo.EndTime) 
            //{
            //    if (firstRecordInfo.StartTime <= secondRecordInfo.EndTime.AddMinutes(mergeIntervalSeconds)) 
            //        return true;
            //}
            //else
            //{
            //    if (firstRecordInfo.EndTime.AddMinutes(mergeIntervalSeconds) >= secondRecordInfo.StartTime) 
            //        return true; 
            //}
            //return false;
        }

        //시작시간과 끝시간을 벗어난 Item을 제거함 !!
        public void RemoveUnavailableRecordInfo(long startTimeUnixMS, long endTimeUnixMS)
        {
            lock (recordingLock)
            {
                recordInfos.RemoveAll(r => r.EndTimeUnixMS < startTimeUnixMS || r.StartTimeUnixMS > endTimeUnixMS);

                //List<TimeLineControlRecordInfo> removeList = new List<TimeLineControlRecordInfo>();
                //foreach (TimeLineControlRecordInfo current in this.TimeLineControlRecordInfo)
                //{
                //    if (current.EndTime < startTime || current.StartTime > endTime)
                //    {
                //        removeList.Add(current);
                //    }
                //}

                //for (int i = 0; i < removeList.Count; i++)
                //{
                //    this.timeLineControlRecordInfos.Remove(removeList[i]);
                //}
            }
        }

        public int GetRecordInfoCount()
        {
            lock (recordingLock)
            {
                return this.recordInfos.Count;
            }
        }

        public PlaybackTimelineRecordInfo? GetRecordInfo(int index)
        {
            lock (recordingLock)
            {
                return (index >= 0 && index < recordInfos.Count) ? recordInfos[index] : null;
                //if (index < 0 || index >= this.timeLineControlRecordInfos.Count)
                //    return null;

                //int count = 0;
                //foreach (TimeLineControlRecordInfo current in this.timeLineControlRecordInfos)
                //{
                //    if (index == count)
                //        return current;

                //    count++;
                //}

                //return null;
            }
        }

        public int GetEventInfoCount()
        {
            lock (eventLock)
            {
                return this.eventInfos.Count;
            }
        }

        public PlaybackTimelineEventInfo? GetEventInfo(int index)
        {
            lock (eventLock)
            {
                return (index >= 0 && index < eventInfos.Count) ? eventInfos[index] : null;

                //if (index < 0 || index >= this.timeLineControlEventInfos.Count)
                //    return null;

                //int count = 0;
                //foreach (TimeLineControlEventInfo current in this.timeLineControlEventInfos)
                //{
                //    if (index == count)
                //        return current;

                //    count++;
                //}

                //return null;
            }
        }
    }
}
