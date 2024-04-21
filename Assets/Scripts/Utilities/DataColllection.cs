
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemDetails///详情
{
    public int itemID;///物品id查询
    public string itemName;///物品名字itemName
    public ItemType itemType;///物品类型
    public Sprite itemIcon;///物品图标
    public Sprite itemOnWorldSprite;///物品在世界地图产生的图片
    public string itemDescription;///详情
    public int itemUseRadius;///物品可使用范围

    ///物品状态
    public bool canPickedup;///拾取
    public bool canDropped;///丢弃
    public bool canCarried;///举着
    public int itemPrice;///价值
    [Range(0, 1)]
    public float sellPercentage;///售卖的折扣
}

[System.Serializable]
public struct InventoryItem
{
    public int itemID;
    public int itemAmount;
}


[System.Serializable]
public class AnimatorType
{    
    public PartType partType;    
    public PartName partName;    
    public AnimatorOverrideController animatorOverride;
}

[System.Serializable]
public class SerializableVector3//可被序列化的坐标存储
{
    public float x, y, z;

    public SerializableVector3(Vector3 pos)//序列化
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
}

[System.Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;//坐标
}

[System.Serializable]
public class SceneFurniture
{
    public int itemID;
    public SerializableVector3 position;
    public int boxIndex;
}

[System.Serializable]
public class TileProperty
{
    public Vector2Int tileCoordinate;
    public bool boolTypeValue;
    public GridType gridType;
  
}

[System.Serializable]
public class TileDetails
{
    public int girdX, gridY;
    public bool canDig;
    public bool canDropItem;
    public bool canPlaceFurniture;
    public bool isNPCObstacle;
    public int daysSinceDug = -1;//被挖天数
    public int daysSinceWatered = -1;//被浇水天数
    public int seedItemID = -1;//种子id
    public int growthDays = -1;//成长天数
    public int daysSinceLastHarvest = -1;//反复收割
}

[System.Serializable]
public class NPCPosition
{
    public Transform npc;
    public string startScene;
    public Vector3 position;
}

//场景路径
[System.Serializable]
public class SceneRoute
{
    public string fromSceneName;
    public string gotoSceneName;
    public List<ScenePath> scenePathList;
}

[System.Serializable]
public class ScenePath
{
    public string sceneName;
    public Vector2Int fromGridCell;
    public Vector2Int gotoGridCell;
}