using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFarm.Inventory
{
public class InventoryUI : MonoBehaviour
 {
    public ItemTooltip itemTooltip;
    
    [Header("拖拽图片")]
    public Image dragItem;
    [Header("玩家背包UI")]
    [SerializeField] private GameObject bagUI;
    private bool bagOpened;
    
    [Header("通用背包")]        
    [SerializeField] private GameObject baseBag;        
    public GameObject shopSlotPrefab;
    public GameObject boxSlotPrefab;
    
    [Header("交易UI")]
    public TradeUI tradeUI;
    public TextMeshProUGUI playerMoneyText;
    
    [SerializeField] private SlotUI [] playerSlots;
    [SerializeField] private List<SlotUI> baseBagSlots;

    private void OnEnable() 
    {
      EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
      EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadedEvent;
      EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
      EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
      EventHandler.ShowTradeUI += OnShowTradeUI;
    }

    private void OnDisable()
    {
      EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
      EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadedEvent;
      EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
      EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
      EventHandler.ShowTradeUI += OnShowTradeUI;
    }

   
    private void Start()
    {
        ///给每一个格子序号
        for (int i =0; i< playerSlots.Length; i++)
        {
            playerSlots[i].slotIndex = i;
        }
        bagOpened = bagUI.activeInHierarchy;
        playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
    }

    ///键盘按下b键打开背包
    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            OpenBagUI();
        }
    }
    
    private void OnShowTradeUI(ItemDetails item, bool isSell)
    {
        tradeUI.gameObject.SetActive(true);
        tradeUI.SetupTradeUI(item, isSell);
    }

    /// <summary>        
    /// 打开通用包裹UI事件        
    /// </summary>        
    /// <param name="slotType"></param>        
    /// <param name="bagData"></param>
     private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)        
     {
        GameObject prefab = slotType switch
        {                
            SlotType.Shop => shopSlotPrefab,                
            SlotType.Box => boxSlotPrefab,               
             _ => null,            
        };

        //生成背包UI            
        baseBag.SetActive(true);

        baseBagSlots = new List<SlotUI>();

        for (int i = 0; i < bagData.itemList.Count; i++)            
        {                
            var slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();                
            slot.slotIndex = i;                
            baseBagSlots.Add(slot);            
        }            
        LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());
        
        if (slotType == SlotType.Shop)
        {
            bagUI.GetComponent<RectTransform>().pivot = new Vector2(-1, 0.5f);
            bagUI.SetActive(true);
            bagOpened = true;
        }
         //更新UI显示            
        OnUpdateInventoryUI(InventoryLocation.Box, bagData.itemList);
     }
    
    /// <summary>
    /// 关闭通用包裹UI事件
    /// </summary>
    /// <param name="slotType"></param>
    /// <param name="bagData"></param>
    private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bagData)
    {
        baseBag.SetActive(false);
        itemTooltip.gameObject.SetActive(false);
        UpdateSlotHightlight(-1);

        foreach (var slot in baseBagSlots)
        {
            Destroy(slot.gameObject);
        }
        baseBagSlots.Clear();

        if (slotType == SlotType.Shop)
        {
            bagUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            bagUI.SetActive(false);
            bagOpened = false;
        }
    }

    
     private void OnBeforeSceneUnloadedEvent()//取消高亮动画
    {
        UpdateSlotHightlight(-1);
    }

    /// <summary>        
    /// 更新指定位置的Slot事件函数        
    /// </summary>        
    /// <param name="location">库存位置</param>        
    /// <param name="list">数据列表</param>
    private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
    {
        switch (location)
        {
            case InventoryLocation.Player:
                for (int i = 0; i < playerSlots.Length; i++)
                {
                    if(list[i].itemAmount>0)
                    {
                        var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                        playerSlots[i].UpdateSlot(item, list[i].itemAmount);
                    }
                    else
                    {
                        playerSlots[i].UpdateEmptySlot();
                    }
                }
                break;
            case InventoryLocation.Box:                    
            for (int i = 0; i < baseBagSlots.Count; i++)                    
            {                        
                if (list[i].itemAmount > 0)                        
                {                            
                    var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);                            
                    baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);                        
                }                        
                else                        
                {                            
                    baseBagSlots[i].UpdateEmptySlot();                        
                }                    
            }                    
            break;    
        }
        playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
    }

    /// <summary>
    ///打开关闭背包UI,Button调用事件
    /// </summary>
    public void OpenBagUI()
    {
        bagOpened = !bagOpened;

        bagUI.SetActive(bagOpened);
    }


    /// <summary>
    /// 更新Slot高亮显示
    /// </summary>
    /// <param name="index">序号</param>
    public void UpdateSlotHightlight(int index)
    {
        foreach (var slot in playerSlots)
        {
            if (slot.isSelected && slot.slotIndex == index)
            {
                slot.slotHightlight.gameObject.SetActive(true);
            }
            else
            {
                slot.isSelected = false;///其他格子取消
                slot.slotHightlight.gameObject.SetActive(false);
            }
        }
    }
  }
}
