using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// note: NPC类1；单例，挂在Persistent身上。1）读取所有NPC线路存盘数据，并内存化，并提供对外访问接口
// note: 2）提供编辑器可编辑器的NPC位置信息，去初始化NPCMovement
public class NPCManager : Singleton<NPCManager>
{
    // note: 所有npc的线路，比如有3个npc有3条路，存盘
    // note: 序列化的数据，可直接在编辑器写好，类似Json存盘
    public SceneRouteDataList_SO sceneRouteDate;
    // note: 3个npc的初始位置
    public List<NPCPosition> npcPositionList;

    // note: 所有npc的线路，比如有3个npc有3条路，内存表现
    private Dictionary<string, SceneRoute> sceneRouteDict = new Dictionary<string, SceneRoute>();

    protected override void Awake()
    {
        base.Awake();

        InitSceneRouteDict();
    }

    private void OnEnable()
    {
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        foreach (var character in npcPositionList)
        {
            // note: 没想到NPC的初始位置是通过NPCManager给到NPCMovement的
            // note: 所以初始信息都可以挂到Manager上？还是说直接挂到NPC身上的NPCMovement更合适？感觉后者也行
            character.npc.position = character.position;
            // note: 这个操作可以学一下的，直接把场景中NPC的transform存进来，就可以得到这个NPC的其他组件了
            character.npc.GetComponent<NPCMovement>().currentScene = character.startScene;
        }
    }

    /// <summary>
    /// 初始化路径字典
    /// </summary>
    // note: 从序列化数据读到内存表现
    private void InitSceneRouteDict()
    {
        if (sceneRouteDate.sceneRouteList.Count > 0)
        {
            foreach (SceneRoute route in sceneRouteDate.sceneRouteList)
            {
                var key = route.fromSceneName + route.gotoSceneName;

                if (sceneRouteDict.ContainsKey(key))
                    continue;

                sceneRouteDict.Add(key, route);
            }
        }
    }

    /// <summary>
    /// 获得两个场景间的路径
    /// </summary>
    /// <param name="fromSceneName">起始场景</param>
    /// <param name="gotoSceneName">目标场景</param>
    /// <returns></returns>
    // note: 提供外部使用
    public SceneRoute GetSceneRoute(string fromSceneName, string gotoSceneName)
    {
        return sceneRouteDict[fromSceneName + gotoSceneName];
    }
}
