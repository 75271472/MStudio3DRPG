using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotInventoryPanel : BaseInventoryPanel
{
    private HotInventoryBK hotInventoryBK; 
    public override EInventoryPanel PanelType => EInventoryPanel.Hotbar;
    public event Action<IndexInfo> OnItemPerformActionEvent;
    public event Action OnPlayerPauseEvent, OnPlayerUnPauseEvent;

    private int selectItemIndex = -1;

    public override void ShowMe()
    {
        gameObject.SetActive(true);
        ResetSelection();
    }

    public override void HideMe()
    {
        UIManager.Instance.HidePanel<InputActionPanel>();
        gameObject.SetActive(false);
        ResetDraggedItem();
    }

    public void HotInventoryInit()
    {
        BaseInventoryInit();

        itemUIList = new List<BaseInventoryItemUI>(
            GetComponentsInChildren<HotItemUI>());

        foreach (var itemUI in itemUIList)
        {
            itemUI.OnLeftClickedEvent += OnItemSelectedHandler;
            itemUI.OnRightClickedEvent += OnItemAcionShowHandler;
            itemUI.OnItemBeginDragEvent += OnItemBeginDragHandler;
            itemUI.OnItemEndDragEvent += OnItemEndDragHandler;
            itemUI.OnItemDroppedEvent += OnItemSwapHandler;

            itemUI.ItemUIInit();
        }

        if (hotInventoryBK == null)
        {
            hotInventoryBK = GetComponentInChildren<HotInventoryBK>();
        }

        //hotInventoryBK.OnPointerEnterEvent += OnPointerEnterHandler;
        //hotInventoryBK.OnPointerExitEvent += OnPointerExitHandler;

        InputManager.Instance.OnHotItemEvent += OnHotItemHandler;
    }

    private void OnPointerEnterHandler()
    {
        OnPlayerPauseEvent?.Invoke();
    }

    private void OnPointerExitHandler()
    {
        OnPlayerUnPauseEvent?.Invoke();
    }

    private void OnHotItemHandler(string controlName)
    {
        int index = int.Parse(controlName);

        if (selectItemIndex == -1 || index - 1 != selectItemIndex)
        {
            OnItemSelectedHandler(itemUIList[index - 1]);
        }
        else
        {
            OnItemPerformActionEvent?.Invoke(GetIndexInfo(selectItemIndex));
            ResetSelection();
        }
    }

    public override void SelectItem(int index)
    {
        base.SelectItem(index);
        selectItemIndex = index;
    }

    public override void ResetSelection()
    {
        base.ResetSelection();
        selectItemIndex = -1;
    }

    protected override void SetDescriptionPosition(int index)
    {
        Transform itemUITrans = itemUIList[index].transform;
        Vector3 targetPos = itemUITrans.position;
        //targetPos.y += (itemUITrans as RectTransform).rect.height / 2 +
        //    (itemDescription.transform as RectTransform).rect.height / 2;
        targetPos.y += (itemUITrans as RectTransform).rect.height / 2;
        itemDescription.SetPosition(targetPos, ItemDescription.EPos.Top);

        //print("AfterSetPos");
    }

    protected override void SetItemActionPosition(InputActionPanel panel, 
        int index)
    {
        Transform itemUITrans = itemUIList[index].transform;
        Vector3 targetPos = itemUITrans.position;
        //targetPos.y += (itemUITrans as RectTransform).rect.height / 2 +
        //    (panel.inputActionBG as RectTransform).rect.height / 2;
        targetPos.y += (itemUITrans as RectTransform).rect.height / 2;
        panel.SetPosition(targetPos, InputActionPanel.EPos.Top);
    }
}

