using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// note: 用于创建一个存储 NPC 日程安排数据的 ScriptableObject，并在 Unity 编辑器中为其提供了一个方便的创建方式
[CreateAssetMenu(fileName = "ScheduleDataList_SO", menuName = "NPC Schedule/ScheduleDataList")]
public class ScheduleDataList_SO : ScriptableObject
{
    public List<ScheduleDetails> scheduleList;
}
