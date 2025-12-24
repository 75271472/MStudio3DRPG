using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

[RequireComponent(typeof(ItemPickUp))]
public class InventoryController : MonoBehaviour
{
    public InventoryPanel InventoryPanel { get; private set; }
    public HotInventoryPanel HotInventoryPanel { get; private set; }
    public PlayerStatePanel PlayerStatePanel { get; private set; }
    public InventoryData InventoryData { get; private set; }
    public ItemPickUp ItemPickUp { get; private set; }
    public ICharacter Character { get; private set; }

    public const int HotItemQuantitty = 8;
    public const int PlayerStateItemQuantity = 2;

    public event Action<InventoryItemInfo> OnCheckItemEvent;
    // 装备物品事件，供PlayerData监听执行OnEnqupHandler，在执行OnEquipItemEvent将消息分发给外部
    public event Action<EquippableItemInfo> OnEquipEvent, 
    // 此时OnUnloadEvent中传入的itemInfo为空，StateItemUI中的物品已被卸下后
    // 执行该回调
        OnUnloadEvent;

    [SerializeField] private List<InventoryItemInfo> itemInfoList =
        new List<InventoryItemInfo>();

    private int currentDraggedItemIndex = -1;

    // 两个面板都不开启情况下isPanelOpen = false
    // 两个面板有一个开启情况下，isPanelOpen = true
    private bool IsPanelOpen => InventoryPanel.isActiveAndEnabled || 
        PlayerStatePanel.isActiveAndEnabled;

    /// <summary>
    /// 初始化背包
    /// </summary>
    /// <param name="character">背包所有者</param>
    public void InventoryControllerInit(ICharacter character)
    {
        OnEquipEvent = null;
        OnUnloadEvent = null;

        this.Character = character;

        // 确保拾取脚本和仓库控制器处于同一对象上
        ItemPickUp = GetComponent<ItemPickUp>();
        InventoryData = GetComponent<InventoryData>();

        // Data初始化前，确保旧的Panel被删除，新的Panel被创建
        UIManager.Instance.HidePanel<InventoryPanel>();
        UIManager.Instance.HidePanel<HotInventoryPanel>();
        UIManager.Instance.HidePanel<PlayerStatePanel>();

        InventoryPanel = UIManager.Instance.ShowPanel<InventoryPanel>();
        HotInventoryPanel = UIManager.Instance.ShowPanel<HotInventoryPanel>();
        PlayerStatePanel = UIManager.Instance.ShowPanel<PlayerStatePanel>();

        InventoryPanel.HideMe();
        PlayerStatePanel.HideMe();

        PrepareData();
        PrepareUI();

        ItemPickUp.ItemPickUpInit(this);

        InputManager.Instance.OnOpenBagEvent += SwitchPanel;
        QuestManager.Instance.OnStartQusetEvent += OnCheckInventoryHandler;
        
        // 初始化执行完毕后更新UI面板，显示InventoryData中加载的数据
        InventoryData.UpdateInventoryUI();
    }

    private void SwitchPanel()
    {
        if (IsPanelOpen)
        {
            InventoryPanel.HideMe();
            PlayerStatePanel.HideMe();
            // 注意不能再InventoryPanel中控制Player是否暂停
            // InventoryController相当于封装了一层InventoryPanel的Show和Hide
            // 原本UIManager的Show和Hide会生成和销毁面板对象
            // 封装后首先生成面板对象，Show和Hide是显示和隐藏面板对象
            // 因此不能直接再InventoryPanel的ShowMe和HideMe中调用Pause或UnPause
            OnPlayerUnPauseHandler();
        }
        else
        {
            InventoryPanel.ShowMe();
            PlayerStatePanel.ShowMe();

            UpdateInventoryPanel(InventoryData.GetCurrentInventoryState());
            OnPlayerPauseHandler();
        }

        //HotInventoryPanel.ResetSelection();
    }

    private void PrepareData()
    {
        InventoryData.InventoryDataInit();
        InventoryData.OnInventoryUpdatedEvent += UpdateInventoryPanel;

        // 测试代码
        //InventoryData.ClearItem();

        //foreach (var itemInfo in itemInfoList)
        //{
        //    //if (itemInfo.IsEmpty) continue;
        //    InventoryData.AddItem(itemInfo);
        //}

        //InventoryData.SaveItem();
    }

    private void PrepareUI()
    {
        PrepareInventoryPanel();
        PrepareHotInventoryPanel();
        PreparePlayerStatePanel();
    }

    private void PrepareInventoryPanel()
    {
        InventoryPanel.OnDescriptionRequestEvent += OnDescriptionRequestHandler;
        InventoryPanel.OnItemActionRequestEvent += OnItemActionRequestHandler;
        InventoryPanel.OnStartDragEvent += OnStartDragHandler;
        InventoryPanel.OnSwapItemEvent += OnSwapItemHandler;
        InventoryPanel.OnItemSelectEvent += OnItemSelectHandler;
        InventoryPanel.OnResetDragEvent += OnResetDragHandler;
        InventoryPanel.OnResetSelectEvent += ResetSelection;

        InventoryPanel.InventoryPanelInit(InventoryData.Size - HotItemQuantitty - 
            PlayerStateItemQuantity);
    }

    private void PrepareHotInventoryPanel()
    {
        HotInventoryPanel.OnDescriptionRequestEvent += OnDescriptionRequestHandler;
        HotInventoryPanel.OnItemActionRequestEvent += OnItemActionRequestHandler;
        HotInventoryPanel.OnStartDragEvent += OnStartDragHandler;
        HotInventoryPanel.OnSwapItemEvent += OnSwapItemHandler;
        HotInventoryPanel.OnItemSelectEvent += OnItemSelectHandler;
        HotInventoryPanel.OnResetDragEvent += OnResetDragHandler;
        HotInventoryPanel.OnResetSelectEvent += ResetSelection;
        HotInventoryPanel.OnItemPerformActionEvent += OnItemPerformActionHandler;
        HotInventoryPanel.OnPlayerPauseEvent += OnPlayerPauseHandler;
        HotInventoryPanel.OnPlayerUnPauseEvent += OnPlayerUnPauseHandler;

        // Init函数中会用到OnResetDragHandler回调，因此先注册事件在初始化
        HotInventoryPanel.HotInventoryInit();
    }

    private void PreparePlayerStatePanel()
    {
        PlayerStatePanel.OnDescriptionRequestEvent += OnDescriptionRequestHandler;
        PlayerStatePanel.OnItemActionRequestEvent += OnItemActionRequestHandler;
        PlayerStatePanel.OnStartDragEvent += OnStartDragHandler;
        PlayerStatePanel.OnSwapItemEvent += OnSwapItemHandler;
        PlayerStatePanel.OnItemSelectEvent += OnItemSelectHandler;
        PlayerStatePanel.OnResetDragEvent += OnResetDragHandler;
        PlayerStatePanel.OnResetSelectEvent += ResetSelection;

        PlayerStatePanel.OnGetCharacterStateDataEvent += 
            Character.CharacterData.GetCharacterStateData;

        //PlayerStatePanel.OnEquipEvent += OnEquipHandler;
        //PlayerStatePanel.OnUnloadEvent += OnUnloadHandler;

        PlayerStatePanel.PlayerStatePanelInit();
    }

    public void OnPlayerPauseHandler()
    {
        //PlayerManager.Instance.PlayerStateMachine.Pause();
    }

    public void OnPlayerUnPauseHandler()
    {
        if (IsPanelOpen) return;
        //PlayerManager.Instance.PlayerStateMachine.UnPause();
    }

    public int AddItem(InventoryItemInfo inventoryItemInfo)
    {
        return InventoryData.AddItem(inventoryItemInfo);
    }

    public int AddItem(InventoryItem inventoryItem)
    {
        return InventoryData.AddItem(inventoryItem);
    }

    public int AddItem(int itemType, int itemId, int quantity)
    {
        InventoryItemInfo inventoryItemInfo = 
            new InventoryItemInfo(itemType, quantity, itemId);
        OnCheckItemEvent?.Invoke(inventoryItemInfo);

        return InventoryData.AddItem(inventoryItemInfo);
    }

    public int AddItem(ItemInfo itemInfo, int quantity, int index)
    {
        return InventoryData.AddItemToSpecialSlot((int)itemInfo.itemType, itemInfo.id, 
            quantity, index);
    }

    public void UpdateItem()
    {
        InventoryData.UpdateItem();
    }

    public void SaveItem()
    {
        InventoryData.SaveItem();
    }

    // 为PlayerState中所有已装备的物品执行OnEquipEvent
    public void UpdateEquipEvent()
    {
        IndexInfo indexInfo = new IndexInfo(EInventoryPanel.PlayerState, -1);
        InventoryItem item;

        indexInfo.index = PlayerStatePanel.GetEquippableItemIndex(
            EEquippableItem.Jewelry);
        if (indexInfo.index != -1)
        {
            item = InventoryData.GetInventoryItem(GetIndex(indexInfo));
            if (!item.IsEmpty)
            {
                OnEquipEvent?.Invoke(item.itemInfo as EquippableItemInfo);
            }            
        }

        indexInfo.index = PlayerStatePanel.GetEquippableItemIndex(
            EEquippableItem.Sword);
        if (indexInfo.index != -1)
        {
            item = InventoryData.GetInventoryItem(GetIndex(indexInfo));
            if (!item.IsEmpty)
            {
                OnEquipEvent?.Invoke(item.itemInfo as EquippableItemInfo);
            }
        }

        indexInfo.index = PlayerStatePanel.GetEquippableItemIndex(
            EEquippableItem.Shield);
        if (indexInfo.index != -1)
        {
            item = InventoryData.GetInventoryItem(GetIndex(indexInfo));
            if (!item.IsEmpty)
            {
                OnEquipEvent?.Invoke(item.itemInfo as EquippableItemInfo);
            }
        }
    }

    public bool EquipItem(EquippableItemInfo itemInfo, int itemIndex)
    {
        // 从PlayerStatePanel中查找该类型装备的物品栏下标（在Panel中的下标）
        int itemIndexInPanel = PlayerStatePanel.GetEquippableItemIndex(itemInfo.
            equippableItemType);
        if (itemIndexInPanel == -1) return false;

        // 要装备到的物品栏在Data中的下标
        currentDraggedItemIndex = itemIndex;
        // 交换两物品
        OnSwapItemHandler(new IndexInfo(EInventoryPanel.PlayerState, itemIndexInPanel));

        return true;
    }

    /// <summary>
    /// 将PlayerState中的物品卸到背包中
    /// </summary>
    /// <param name="itemInfo">要卸下的物品</param>
    /// <param name="itemIndex">卸下物品的Data下标</param>
    /// <returns></returns>
    public bool UnloadItem(EquippableItemInfo itemInfo, int itemIndex)
    {
        int itemIndexInData = InventoryData.GetEmptyItem();
        // 物品栏已满
        if (itemIndexInData == -1) return false;

        currentDraggedItemIndex = itemIndex;
        OnSwapItemHandler(new IndexInfo(GetPanelTypeByIndex(itemIndexInData), 
            GetPanelIndex(itemIndexInData)));

        return true;
    }

    public EquippableItemInfo GetEquippableItemInfo(EEquippableItem equippableItemType)
    {
        int itemIndexInPanel = PlayerStatePanel.
            GetEquippableItemIndex(equippableItemType);
        if (itemIndexInPanel == -1) return null;

        int itemIndex = GetIndex(new IndexInfo(EInventoryPanel.PlayerState,
            itemIndexInPanel));

        return (InventoryData.GetInventoryItem(itemIndex).itemInfo) 
            as EquippableItemInfo;
    }

    public EInventoryPanel GetPanelTypeByIndex(int indexInData)
    {
        if (indexInData < 0 || indexInData >= InventoryData.Size) 
            return EInventoryPanel.None;
        if (indexInData < HotItemQuantitty) 
            return EInventoryPanel.Hotbar;
        if (indexInData < InventoryData.Size - PlayerStateItemQuantity) 
            return EInventoryPanel.Inventory;

        return EInventoryPanel.PlayerState;
    }

    private BaseInventoryPanel GetPanel(IndexInfo indexInfo)
    {
        return indexInfo.panelType switch
        {
            EInventoryPanel.Inventory => InventoryPanel,
            EInventoryPanel.Hotbar => HotInventoryPanel,
            EInventoryPanel.PlayerState => PlayerStatePanel,
            _ => null
        };
    }

    private BaseInventoryPanel GetPanelByIndex(int index)
    {
        if (index < 0 || index >= InventoryData.Size) return null;
        if (index < HotItemQuantitty) return HotInventoryPanel;
        if (index < InventoryData.Size - PlayerStateItemQuantity) return InventoryPanel;
        return PlayerStatePanel;
    }

    private int GetIndex(IndexInfo indexInfo)
    {
        return indexInfo.panelType switch
        {
            EInventoryPanel.Hotbar => indexInfo.index,
            EInventoryPanel.Inventory => indexInfo.index + HotItemQuantitty,
            EInventoryPanel.PlayerState => indexInfo.index + 
                InventoryData.Size - PlayerStateItemQuantity,
            _ => -1
        };
    }

    private int GetPanelIndex(int index)
    {
        if (index >= InventoryData.Size - PlayerStateItemQuantity) 
            index -= InventoryData.Size - PlayerStateItemQuantity;
        else if (index >= HotItemQuantitty)
            index -= HotItemQuantitty;
        return index;
    }

    private void ResetAllItems()
    {
        InventoryPanel.ResetAllItems();
        HotInventoryPanel.ResetAllItems();
        PlayerStatePanel.ResetAllItems();
    }

    private void ResetSelection()
    {
        InventoryPanel.ResetSelection();
        HotInventoryPanel.ResetSelection();
        PlayerStatePanel.ResetSelection();
    }

    private void OnResetDragHandler()
    {
        currentDraggedItemIndex = -1;
    }

    private void OnItemSelectHandler(IndexInfo indexInfo)
    {
        InventoryItem inventoryItem = 
            InventoryData.GetInventoryItem(GetIndex(indexInfo));

        ResetSelection();
        //GetPanel(indexInfo).ResetSelection();

        if (inventoryItem.IsEmpty) return;

        GetPanel(indexInfo).SelectItem(indexInfo.index);
    }

    private void OnDescriptionRequestHandler(IndexInfo indexInfo)
    {
        InventoryItem inventoryItem = 
            InventoryData.GetInventoryItem(GetIndex(indexInfo));

        if (inventoryItem.IsEmpty) return;

        GetPanel(indexInfo).UpdateDescription(indexInfo.index, 
            inventoryItem.itemInfo.name, inventoryItem.itemInfo.GetDescription());
    }

    /// <summary>
    /// 显示面板再执行PerformAction
    /// </summary>
    /// <param name="indexInfo"></param>
    private void OnItemActionRequestHandler(IndexInfo indexInfo)
    {
        InventoryItem inventoryItem = 
            InventoryData.GetInventoryItem(GetIndex(indexInfo));
        if (inventoryItem.IsEmpty) return;

        GetPanel(indexInfo).ResetSelection();
        // 先隐藏面板，去掉面板中的已有事件
        GetPanel(indexInfo).HideItemAction();

        // 物品为可装备物品 单独处理
        if (inventoryItem.itemInfo is EquippableItemInfo equippableItemInfo)
        {
            // 装备不位于PlayerState
            // 只有当装备位于PlayerState才可Drop
            if (indexInfo.panelType != EInventoryPanel.PlayerState)
            {
                GetPanel(indexInfo).ShowItemAction(indexInfo.index);
                GetPanel(indexInfo).AddAction("Equip", () =>
                    EquipItem(equippableItemInfo, GetIndex(indexInfo)));

                // 物品为可移除物品
                if (inventoryItem.itemInfo is IDestroyableItem itemDestroy)
                {
                    GetPanel(indexInfo).ShowItemAction(indexInfo.index);
                    GetPanel(indexInfo).AddAction("Drop", () =>
                        DropItem(GetIndex(indexInfo), inventoryItem.quantity));
                }
            }
            // 装备位于PlayerState
            else
            {
                GetPanel(indexInfo).ShowItemAction(indexInfo.index);
                GetPanel(indexInfo).AddAction("Unload", () =>
                    UnloadItem(equippableItemInfo, GetIndex(indexInfo)));
            }
        }
        else
        {
            // 物品为可执行操作的物品
            if (inventoryItem.itemInfo is IItemAction itemAction)
            {
                GetPanel(indexInfo).ShowItemAction(indexInfo.index);
                GetPanel(indexInfo).AddAction(itemAction.ActionName,
                    () => PerformAction(GetIndex(indexInfo)));
            }
            // 物品为可移除物品
            if (inventoryItem.itemInfo is IDestroyableItem itemDestroy)
            {
                GetPanel(indexInfo).ShowItemAction(indexInfo.index);
                GetPanel(indexInfo).AddAction("Drop", () =>
                    DropItem(GetIndex(indexInfo), inventoryItem.quantity));
            }
        }
    }

    private void OnItemPerformActionHandler(IndexInfo indexInfo)
    {
        GetPanel(indexInfo).ResetSelection();

        PerformAction(GetIndex(indexInfo));
    }

    private void OnCheckInventoryHandler(Quest quest)
    {
        Dictionary<int, InventoryItem> inventoryItemDict = 
            InventoryData.GetCurrentInventoryState();

        foreach (var pair in inventoryItemDict)
        {
            OnCheckItemEvent?.Invoke(new InventoryItemInfo(pair.Value.itemInfo, 
                pair.Value.quantity));
        }
    }

    private void PerformAction(int index)
    {
        InventoryItem inventoryItem = InventoryData.GetInventoryItem(index);
        if (inventoryItem.IsEmpty) return;

        // 先将要使用的物品从背包中移除
        if (inventoryItem.itemInfo is IDestroyableItem itemDestroy)
        {
            InventoryData.RemoveItem(index, 1);
        }
        // 在使用物品
        if (inventoryItem.itemInfo is IItemAction itemAction)
        {
            itemAction.PerformAction(new PerformActionInfo(Character, index));
        }
    }

    private void DropItem(int index, int quantity)
    {
        print("DropItem");

        InventoryData.RemoveItem(index, quantity);
        ResetSelection();
    }

    private void OnStartDragHandler(IndexInfo indexInfo)
    {
        // PlayerState中的物品不能通过Drag卸下，只能点击Drop卸下
        if (indexInfo.panelType == EInventoryPanel.PlayerState) return;

        InventoryItem inventoryItem = 
            InventoryData.GetInventoryItem(GetIndex(indexInfo));
        if (inventoryItem.IsEmpty) return;

        currentDraggedItemIndex = GetIndex(indexInfo);
        //print("CurrentDraggedItemIndex: " + currentDraggedItemIndex);

        BaseInventoryPanel panel = GetPanel(indexInfo);
        panel.ResetSelection();
        panel.CreateDraggedItem(inventoryItem.itemInfo.img,
            inventoryItem.quantity);
    }

    private void OnSwapItemHandler(IndexInfo indexInfo)
    {
        if (currentDraggedItemIndex == -1) return;

        if (indexInfo.panelType == EInventoryPanel.PlayerState)
        {
            InventoryItem inventoryItem =
                InventoryData.GetInventoryItem(currentDraggedItemIndex);

            // 如果要交换的物品类型不是可装备类型，不继续执行
            if (inventoryItem.itemInfo is EquippableItemInfo itemInfo)
            {
                // 如果要交换的物品类型不是 PlayerStatePanel中对应物品栏要求的物品类型
                // 不继续执行
                if (itemInfo.equippableItemType != PlayerStatePanel.
                    GetEquippableItemType(indexInfo.index)) return;

                // print($"EquipItem DragItem: {currentDraggedItemIndex} TargetItem: {GetIndex(indexInfo)}");
                OnEquipEvent?.Invoke(itemInfo);
            }
            else return;
        }

        IndexInfo currentDragIndexInfo = new IndexInfo(
            GetPanelTypeByIndex(currentDraggedItemIndex), currentDraggedItemIndex);
        if (currentDragIndexInfo.panelType == EInventoryPanel.PlayerState)
        {
            InventoryItem inventoryItem =
                InventoryData.GetInventoryItem(currentDraggedItemIndex);

            if (inventoryItem.itemInfo is EquippableItemInfo itemInfo)
            {
                print($"UnloadItem DragItem: {currentDraggedItemIndex} TargetItem: {GetIndex(indexInfo)}");
                OnUnloadEvent?.Invoke(itemInfo);
            }
        }

        InventoryData.SwapItem(currentDraggedItemIndex, GetIndex(indexInfo));
        GetPanelByIndex(currentDraggedItemIndex).ResetSelection();
    }

    private void UpdateInventoryPanel(Dictionary<int, InventoryItem> stateDict)
    {
        //print("UpdateInventoryPanel");
        ResetAllItems();

        foreach (var pair in stateDict)
        {
            GetPanelByIndex(pair.Key).UpdateData(GetPanelIndex(pair.Key),
                pair.Value.itemInfo.img, pair.Value.quantity);
        }
    }
}
