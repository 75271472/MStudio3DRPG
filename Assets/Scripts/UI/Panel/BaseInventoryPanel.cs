using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EInventoryPanel
{
    None,
    Inventory,
    Hotbar,
    PlayerState
}

public class IndexInfo
{
    public EInventoryPanel panelType;
    public int index;

    public IndexInfo(EInventoryPanel panelType, int indexInPanel)
    {
        this.panelType = panelType;
        this.index = indexInPanel;
    }
}

public abstract class BaseInventoryPanel : BasePanel
{
    protected ItemDescription itemDescription;
    protected MouseFollower mouseFollower;
    protected List<BaseInventoryItemUI> itemUIList = new List<BaseInventoryItemUI>();
    //protected int currentDraggedItemIndex = -1;

    public abstract EInventoryPanel PanelType { get;}
    public event Action OnResetDragEvent, OnResetSelectEvent;
    public event Action<IndexInfo> OnDescriptionRequestEvent, OnItemActionRequestEvent,
        OnStartDragEvent, OnItemSelectEvent;
    public event Action<IndexInfo> OnSwapItemEvent;
    //public event Func<int, int> OnGetIndexEvent;

    protected void BaseInventoryInit()
    {
        if (itemDescription == null)
        {
            itemDescription = UIManager.Instance.GetPanel<ItemDescription>();
            if (itemDescription == null)
            {
                itemDescription = UIManager.Instance.ShowPanel<ItemDescription>(
                    EUILayer.Top);
            }
        }

        itemDescription.HideMe();

        if (mouseFollower == null)
        {
            mouseFollower = UIManager.Instance.GetPanel<MouseFollower>();
            if (mouseFollower == null)
            {
                mouseFollower = UIManager.Instance.ShowPanel<MouseFollower>(
                    EUILayer.Top);
            }
        }

        mouseFollower.HideMe();
    }

    public void UpdateData(int index, Sprite itemImg, int itemQuantity)
    {
        //print("UpdateData: " + index);
        if (index < 0 || index >= itemUIList.Count) return;

        itemUIList[index].SetData(itemImg, itemQuantity);
    }

    public virtual void ResetSelection()
    {
        //print(name + "ResetSelection");

        itemDescription?.ResetDescription();
        DeselectAllItems();
    }

    public virtual void SelectItem(int index)
    {
        itemUIList[index].Select();
    }

    public void ResetAllItems()
    {
        DeselectAllItems();

        foreach (var itemUI in itemUIList)
        {
            itemUI.ResetData();
        }
    }

    public void UpdateDescription(int index, string name, string description)
    {
        itemDescription.SetDescription(name, description);
    }

    public void CreateDraggedItem(Sprite sprite, int quantity)
    {
        mouseFollower.ShowMe();
        mouseFollower.SetData(sprite, quantity);
    }

    public void ShowItemAction(int itemIndex)
    {
        InputActionPanel panel = UIManager.Instance.GetPanel<InputActionPanel>();
        if (panel == null)
            panel = UIManager.Instance.ShowPanel<InputActionPanel>();

        SetItemActionPosition(panel, itemIndex);
    }

    public void HideItemAction()
    {
        UIManager.Instance.HidePanel<InputActionPanel>();
    }

    public void AddAction(string actionName, UnityAction action)
    {
        UIManager.Instance.GetPanel<InputActionPanel>().AddInput(actionName, action);
    }

    public IndexInfo GetIndexInfo(int index)
    {
        return new IndexInfo(PanelType, index);
    }

    protected void DeselectAllItems()
    {
        foreach (var itemUI in itemUIList)
        {
            itemUI.Deselect();
        }

        UIManager.Instance.HidePanel<InputActionPanel>();
    }

    protected void ResetDraggedItem()
    {
        mouseFollower?.HideMe();
        //currentDraggedItemIndex = -1;
        OnResetDragEvent?.Invoke();
    }

    protected void OnResetSelectHandler()
    {
        OnResetSelectEvent?.Invoke();
    }

    protected void OnItemSelectedHandler(BaseInventoryItemUI itemUI)
    {
        //print("OnLeftClicked " + itemUI.name);
        int itemIndex = itemUIList.IndexOf(itemUI);

        if (itemIndex == -1) return;

        // 在Controller中通过InventoryData，判断所选项是否为空数据
        // 决定是否能够选中
        OnItemSelectEvent?.Invoke(GetIndexInfo(itemIndex));
        // OnItemSelectEvent会执行ResetSelection重置选中
        // 因此在OnItemSelectEvent执行后执行OnShowDescriptionHandler
        OnShowDescriptionHandler(itemUI);
    }

    protected void OnItemAcionShowHandler(BaseInventoryItemUI itemUI)
    {
        int itemIndex = itemUIList.IndexOf(itemUI);
        if (itemIndex == -1) return;

        OnItemActionRequestEvent?.Invoke(GetIndexInfo(itemIndex));
    }

    protected void OnItemBeginDragHandler(BaseInventoryItemUI itemUI)
    {
        int itemIndex = itemUIList.IndexOf(itemUI);
        if (itemIndex == -1) return;

        //currentDraggedItemIndex = itemIndex;
        //print("OnItemBeginDrag" + itemUI.name);

        //OnItemSelectedHandler(itemUI);
        OnStartDragEvent?.Invoke(GetIndexInfo(itemIndex));
    }

    protected void OnItemEndDragHandler(BaseInventoryItemUI itemUI)
    {
        //print("OnItemEndDrag");
        ResetDraggedItem();
    }

    protected void OnItemSwapHandler(BaseInventoryItemUI itemUI)
    {
        //print("OnItemDrop");
        int itemIndex = itemUIList.IndexOf(itemUI);
        if (itemIndex == -1) return;

        // 物体数据交换逻辑
        OnSwapItemEvent?.Invoke(GetIndexInfo(itemIndex));
        // OnItemSwapHandler执行之前会执行OnItemBeginDragHandler，
        // 在OnItemBeginDragHandler中会执行OnItemSelectedHandler，
        // 从而在InventoryManager中执行OnItemSelectHandler，执行ResetSelection
        // 重置所有选中并关闭描述界面
        // 因此在交换操作完成，也就是执行OnItemSwapHandler时，重新选中被交换物体
        //OnItemSelectedHandler(itemUI);
        // 由于OnDrop会在OnEndDrag之前显示
        // 因此OnItemEndDragHandler会在OnSwapHandler之后才会执行ResetDraggedItem
        // 将currentDraggedItemIndex置为-1
        // 而OnShowDescriptionHandler会对currentDraggedItemIndex是否等于-1进行检查
        // 因此这里直接调用OnShowDescriptionHandler无效
        // 因为此时currentDraggedItemIndex未被重置
        //OnShowDescriptionHandler(itemUI);
        // 因此这里直接触发回调，绕过currentDraggedItemIndex != -1检查
        // 实现交换物品后仍然显示物品描述
        //OnDescriptionRequestEvent?.Invoke(itemIndex);
        //itemDescription.ItemDescriptionSwitch(true);
    }

    protected void OnShowDescriptionHandler(BaseInventoryItemUI itemUI)
    {
        int itemIndex = itemUIList.IndexOf(itemUI);
        // 所选物品不存在，或者当前正在拖动物品，不显示物品描述
        if (itemIndex == -1) return;

        OnDescriptionRequestEvent?.Invoke(GetIndexInfo(itemIndex));
        // OnDescriptionRequestEvent回调会调用itemDescription.SetDescription
        // 在该方法中激活itemDescription对象
        //itemDescription.ItemDescriptionSwitch(true);

        SetDescriptionPosition(itemIndex);
    }

    protected abstract void SetDescriptionPosition(int index);

    protected abstract void SetItemActionPosition(
        InputActionPanel panel, int index);
}
