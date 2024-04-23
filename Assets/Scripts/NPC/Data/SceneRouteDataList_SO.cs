using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SceneRouteDataList_SO", menuName = "NPC Schedule/SceneRouteDataList")]
public class SceneRouteDataList_SO : ScriptableObject
{
    // note: 所有npc的线路，比如有3个npc有3条路，存盘
    public List<SceneRoute> sceneRouteList;
}