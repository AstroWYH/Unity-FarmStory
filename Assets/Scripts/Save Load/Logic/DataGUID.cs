using UnityEngine;

// note: 存档类6；提供唯一资产ID；编辑器模式下也执行
[ExecuteAlways]
public class DataGUID : MonoBehaviour
{
    public string guid;

    private void Awake()
    {
        if (guid == string.Empty)
        {
            guid = System.Guid.NewGuid().ToString();
        }
    }
}