using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// note: ���ڴ���һ���洢 NPC �ճ̰������ݵ� ScriptableObject������ Unity �༭����Ϊ���ṩ��һ������Ĵ�����ʽ
[CreateAssetMenu(fileName = "ScheduleDataList_SO", menuName = "NPC Schedule/ScheduleDataList")]
public class ScheduleDataList_SO : ScriptableObject
{
    public List<ScheduleDetails> scheduleList;
}
