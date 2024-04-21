using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapDate_SO", menuName = "Map/MapDate")]
public class MapData_SO : ScriptableObject
{
    [Header("地图信息")]
    public int gridWidth;
    public int gridHeight;

    [Header("地图左原点")] 
    public int originX;
    public int originY;
        
        
    [SceneName] public string sceneName;
    public List<TileProperty> tileProperties;
}