using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryData : MonoBehaviour
{
    public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdatedEvent;

    // 前八个物品为快捷栏物品，后两个物品为PlayerState物品，剩余物品为背包中的物品
    private List<InventoryItem> inventoryItemList;
    // 背包中的各自数量，包括有物品的格子和没有物品的格子
    public int Size => inventoryItemList.Count;
    // 不可添加的物品数量（从仓库结尾开始，表现为不可向PlayerStatePanel中添加物品）
    // UnAddableSize物品格的物品只可通过Swap或AddToSpecial添加
    public const int UnAddableSize = InventoryController.PlayerStateItemQuantity;

    public void InventoryDataInit()
    {
        InventoryInfo inventoryInfo = DataManager.Instance.InventoryInfo;
        inventoryItemList = new List<InventoryItem>(inventoryInfo.size);
        List<InventoryItemInfo> inventoryItemInfoList = 
            inventoryInfo.inventoryItemList;

        // 初始化背包
        for (int i = 0; i < inventoryInfo.size; i++) 
        {
            AddEmptySlot();
        }

        // 向背包中添加物品
        for (int i = 0; i < inventoryItemInfoList.Count; i++)
        {
            AddItemToSpecialSlot(inventoryItemInfoList[i]);
        }
    }

    public void UpdateItem()
    {
        DataManager.Instance.UpdateInventoryItemInfoList(Size, 
            GetCurrentInventoryState());
    }

    public void SaveItem()
    {
        DataManager.Instance.SaveInventoryItemList(Size, 
            GetCurrentInventoryState());
    }

    public void AddEmptySlot()
    {
        //print("AddEmptySlot");
        inventoryItemList.Add(InventoryItem.GetEmptyItem());
    }

    public int AddItem(InventoryItemInfo inventoryItemInfo)
    {
        return AddItem(inventoryItemInfo.itemInfoType, inventoryItemInfo.itemId, 
            inventoryItemInfo.quantity);
    }

    public int AddItem(InventoryItem inventoryItem)
    {
        if (inventoryItem.IsEmpty) return inventoryItem.quantity;

        return AddItem((int)inventoryItem.itemInfo.itemType, 
            inventoryItem.itemInfo.id, inventoryItem.quantity);
    }

    public int AddItemToSpecialSlot(InventoryItemInfo itemInfo)
    {
        return AddItemToSpecialSlot(itemInfo.itemInfoType, itemInfo.itemId, 
            itemInfo.quantity, itemInfo.index);
    }

    public int AddItemToSpecialSlot(int itemInfoType, int itemId, int quantity, 
        int index)
    {
        if (index < 0 || index >= inventoryItemList.Count) return quantity;
        if (itemId == -1 || quantity <= 0) return quantity;

        ItemInfo itemInfo =
            DataManager.Instance.GetItemInfo(itemInfoType, itemId);

        quantity = AddItemToSpecifiedSlot(itemInfo, quantity, index);
        // 如果还有剩余物品，则向其他格子中添加
        //if (quantity > 0)
        //    quantity = AddItem(itemInfoType, itemId, quantity);

        // 更新UI
        UpdateInventoryUI();
        return quantity;
    }

    /// <summary>
    /// 向Inventory中添加物品
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="quantity"></param>
    /// <returns>剩余物品数量</returns>
    public int AddItem(int itemInfoType, int itemId, int quantity)
    {
        if (itemId == -1 || quantity <= 0) return quantity;

        //print("AddItem " + itemInfoType + " " + itemId + " " + quantity);
        ItemInfo itemInfo = 
            DataManager.Instance.GetItemInfo(itemInfoType, itemId);

        if (!itemInfo.isStackable)
        {
            // 不可堆叠物品，直接将物品放入空格子中
            quantity = AddItemToEmptySlot(itemInfo, quantity);
        }
        else
        {
            // 可堆叠物品，将物品放入不空格子中进行叠加
            quantity = AddItemToNotEmptySlot(itemInfo, quantity);
        }

        // 更新UI
        UpdateInventoryUI();
        return quantity;
    }

    // 背包内没有空格子，但可能还能在可堆叠物品格上堆东西
    public bool IsInventoryExistEmptySlot()
    {
        return inventoryItemList.Exists(inventoryItem => inventoryItem.IsEmpty);
    }

    /// <summary>
    /// 向指定下标的格子中添加物品
    /// </summary>
    /// <returns></returns>
    private int AddItemToSpecifiedSlot(ItemInfo itemInfo, int quantity, int index)
    {
        // 指定格子为空格子
        if (inventoryItemList[index].IsEmpty)
        {
            // 获取添加数量和最大堆叠数之间的较小值
            int fillQuantity = Mathf.Min(quantity, itemInfo.maxStackSize);

            inventoryItemList[index] =
                new InventoryItem(itemInfo, fillQuantity);
            quantity -= fillQuantity;
        }
        // 指定格子中已有物品
        else
        {
            if (inventoryItemList[index].itemInfo.itemType != itemInfo.itemType ||
                inventoryItemList[index].itemInfo.id != itemInfo.id) 
                return quantity;

            int fillQuantity = itemInfo.maxStackSize - 
                inventoryItemList[index].quantity;
            int addAmount = Math.Min(fillQuantity, quantity);

            inventoryItemList[index] = inventoryItemList[index].ChangeQuantity(
                inventoryItemList[index].quantity + addAmount);
            quantity -= addAmount;
        }

        return quantity;
    }

    /// <summary>
    /// 向空格子中填入物品
    /// </summary>
    /// <param name="itemInfo"></param>
    /// <param name="itemId"></param>
    /// <param name="quantity">要填入的物品总数量</param>
    /// <returns>剩余物品数量</returns>
    private int AddItemToEmptySlot(ItemInfo itemInfo, int quantity)
    {
        int fillQuantity = 0;

        // 遍历所有格子，直到遍历完成或要放入的物品数量为0，不可添加物品格除外
        for (int i = 0; i < inventoryItemList.Count - UnAddableSize && 
            quantity > 0; i++)
        {
            if (!inventoryItemList[i].IsEmpty) continue;

            // 获取添加数量和最大堆叠数之间的较小值
            fillQuantity = Mathf.Min(quantity, itemInfo.maxStackSize);

            inventoryItemList[i] =
                new InventoryItem(itemInfo, fillQuantity);
            quantity -= fillQuantity;
        }

        return quantity;
    }

    /// <summary>
    /// 向已有物品的格子上堆加物品
    /// </summary>
    /// <param name="itemInfo"></param>
    /// <param name="itemId"></param>
    /// <param name="quantity"></param>
    /// <returns>剩余物品数</returns>
    private int AddItemToNotEmptySlot(ItemInfo itemInfo, int quantity)
    {
        int fillQuantity = 0;
        int addAmount = 0;

        // 首先向相同物品Id的格子中填充，不可添加物品格除外
        for (int i = 0; i < inventoryItemList.Count - UnAddableSize && 
            quantity > 0; i++)
        {
            if (inventoryItemList[i].IsEmpty || 
                inventoryItemList[i].itemInfo.itemType != itemInfo.itemType ||
                inventoryItemList[i].itemInfo.id != itemInfo.id) continue;
            // 计算当前格子可容纳物品数量
            fillQuantity = itemInfo.maxStackSize - inventoryItemList[i].quantity;
            addAmount = Math.Min(fillQuantity, quantity);

            inventoryItemList[i] = inventoryItemList[i].ChangeQuantity(
                inventoryItemList[i].quantity + addAmount);
            quantity -= addAmount;
        }

        if (quantity != 0)
            quantity = AddItemToEmptySlot(itemInfo, quantity);

        return quantity;
    }

    public void SwapItem(int indexA, int indexB)
    {
        InventoryItem itemA = inventoryItemList[indexA];
        inventoryItemList[indexA] = inventoryItemList[indexB];
        inventoryItemList[indexB] = itemA;

        UpdateInventoryUI();
    }

    public void ClearItem()
    {
        for (int i = 0; i < inventoryItemList.Count; i++)
        {
            if (inventoryItemList[i].IsEmpty) continue;
            inventoryItemList[i] = InventoryItem.GetEmptyItem();
        }
    }

    public Dictionary<int, InventoryItem> GetCurrentInventoryState()
    {
        Dictionary<int, InventoryItem> stateDict = 
            new Dictionary<int, InventoryItem>();

        for (int i = 0; i < inventoryItemList.Count; i++)
        {
            if (inventoryItemList[i].IsEmpty) continue;

            stateDict.Add(i, inventoryItemList[i]);
        }

        return stateDict;
    }

    public InventoryItem GetInventoryItem(int index)
    {
        if (index < 0 || index >= inventoryItemList.Count) 
            return InventoryItem.GetEmptyItem();

        return inventoryItemList[index];
    }

    public void RemoveItem(int index, int amount)
    {
        if (index < 0 || index >= inventoryItemList.Count) return;
        if (inventoryItemList[index].IsEmpty) return;

        int reminder = inventoryItemList[index].quantity - amount;
        if (reminder <= 0)
            inventoryItemList[index] = InventoryItem.GetEmptyItem();
        else
            inventoryItemList[index] = 
                inventoryItemList[index].ChangeQuantity(reminder);

        UpdateInventoryUI();
    }

    public int GetEmptyItem()
    {
        for (int i = 0; i < inventoryItemList.Count; i++)
        {
            if (inventoryItemList[i].IsEmpty)
                return i;
        }

        return -1;
    }

    public void UpdateInventoryUI()
    {
        OnInventoryUpdatedEvent?.Invoke(GetCurrentInventoryState());
    }
}

[Serializable]
// Presentdata中存储的单个物体数据
public class InventoryItemInfo
{
    public int itemInfoType;
    public int quantity;
    public int itemId;
    public int index;
    public bool IsEmpty => itemId == -1;

    public InventoryItemInfo() { }

    public InventoryItemInfo(InventoryItem inventoryItem, int index)
    {
        if (inventoryItem.IsEmpty)
        {
            itemId = -1;
            return;
        }

        itemInfoType = (int)inventoryItem.itemInfo.itemType;
        quantity = inventoryItem.quantity;
        itemId = inventoryItem.itemInfo.id;
        this.index = index;
    }

    public InventoryItemInfo(ItemInfo itemInfo, int quantity)
    {
        itemInfoType = (int)itemInfo.itemType;
        this.quantity = quantity;
        itemId = itemInfo.id;
    }

    public InventoryItemInfo(int itemInfoType, int quantity, int itemId)
    {
        if (itemId == -1)
        {
            itemId = -1;
            return;
        }

        this.itemInfoType = itemInfoType;
        this.quantity = quantity;
        this.itemId = itemId;
    }

    public ItemInfo GetItemInfo()
    {
        return DataManager.Instance.GetItemInfo(itemInfoType, itemId);
    }
}

// 背包中的单个物体数据结构体
public struct InventoryItem
{
    //public int itemInfoType;
    public int quantity;
    //public int itemId;
    public ItemInfo itemInfo;
    public bool IsEmpty => itemInfo == null;

    // 从Persisentdata加载InventoryItemInfo实例化背包中的物体结构体
    public InventoryItem(InventoryItemInfo inventoryItemInfo)
    {
        if (inventoryItemInfo.IsEmpty)
        {
            this = GetEmptyItem();
        }
        else
        {
            this = new InventoryItem(inventoryItemInfo.itemInfoType,
                inventoryItemInfo.itemId, inventoryItemInfo.quantity);
        }
    }

    public InventoryItem(ItemInfo itemInfo, int quantity)
    {
        if (itemInfo == null)
        {
            this = GetEmptyItem();
        }
        else
        {
            this.quantity = quantity;
            this.itemInfo = itemInfo;
        }
    }

    public InventoryItem(int itemType, int itemId, int quantity)
    {
        if (itemId < 0)
        {
            this = GetEmptyItem();
            return;
        }

        itemInfo = DataManager.Instance.GetItemInfo(itemType, itemId);
        this.quantity = Mathf.Min(quantity, itemInfo.maxStackSize);
    }

    public InventoryItem ChangeQuantity(int newQuantity)
    {
        return new InventoryItem 
        {
            quantity = newQuantity,
            itemInfo = this.itemInfo
        };
    }

    public static InventoryItem GetEmptyItem()
    {
        return new InventoryItem
        {
            quantity = 0,
            itemInfo = null
        };
    }
}
