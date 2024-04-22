using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MFarm.Save;

// note: 存档类3；UI类3；MVC的V，和界面直接关联；在控件上注册了SaveLoadManager的LoadGameData()，但没注册Save()相关
public class SaveSlotUI : MonoBehaviour
{
    public Text dataTime, dataScene;
    private Button currentButton;
    private DataSlot currentData;

    private int Index => transform.GetSiblingIndex();

    private void Awake()
    {
        currentButton = GetComponent<Button>();
        // note: 应该这样说，UI控件的onClick，通常会让UI自己Update()，然后很可能从中调Manager的逻辑，比如取数据，最后更新UI表现
        // note: 但LoadGameData()没有更新UI表现，毕竟是存档逻辑
        currentButton.onClick.AddListener(LoadGameData);
    }

    private void OnEnable()
    {
        SetupSlotUI();
    }

    // note: 这里是在OnEnable()中调用，但如果是其他普通UI，也可能在TogglePanel()中调用，TogglePanel()再绑在某界面Icon的onClick或按键中
    private void SetupSlotUI()
    {
        // note: 存档界面，即使玩家还没点击，3个slot实例已经各自读取一份dataslot数据了
        currentData = SaveLoadManager.Instance.dataSlots[Index];

        if (currentData != null)
        {
            dataTime.text = currentData.DataTime;
            dataScene.text = currentData.DataScene;
        }
        else
        {
            dataTime.text = "这个世界还没开始";
            dataScene.text = "梦还没开始";
        }
    }

    private void LoadGameData()
    {
        // note: 读已有存档
        if (currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        else // note: 新游戏
        {
            Debug.Log("新游戏");
            // note: 执行和存档无关的逻辑；广播出去
            EventHandler.CallStartNewGameEvent(Index);
        }
    }
}
