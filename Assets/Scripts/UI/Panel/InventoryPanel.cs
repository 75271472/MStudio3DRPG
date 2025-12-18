using System.Collections;
using UnityEngine;

public class InventoryPanel : BaseInventoryPanel
{
    [SerializeField] private RectTransform contentTrans;

    private InventoryBK inventoryBK;

    public override EInventoryPanel PanelType => EInventoryPanel.Inventory;

    public override void ShowMe()
    {
        if (inventoryBK == null)
        {
            inventoryBK = GetComponentInChildren<InventoryBK>();
            inventoryBK.OnPointerClickEvent += OnResetSelectHandler;
        }

        gameObject.SetActive(true);
        ResetSelection();
    }

    public override void HideMe()
    {
        UIManager.Instance.HidePanel<InputActionPanel>();
        gameObject.SetActive(false);
        ResetDraggedItem();
    }

    public void InventoryPanelInit(int inventorySize)
    {
        BaseInventoryInit();

        for (int i = 0; i < inventorySize; i++)
        {
            GameObject itemUIObj = PoolManager.Instance.PullObj(
                DataManager.INVENTORYITEMUI);
            itemUIObj.transform.SetParent(contentTrans, false);
            InventoryItemUI itemUI = itemUIObj.GetComponent<InventoryItemUI>();

            itemUI.OnLeftClickedEvent += OnItemSelectedHandler;
            itemUI.OnRightClickedEvent += OnItemAcionShowHandler;
            itemUI.OnItemBeginDragEvent += OnItemBeginDragHandler;
            itemUI.OnItemEndDragEvent += OnItemEndDragHandler;
            itemUI.OnItemDroppedEvent += OnItemSwapHandler;
            //itemUI.OnPointerEnterEvent += OnShowDescriptionHandler;
            //itemUI.OnPointerExitEvent += OnHideDescriptionHandler;

            itemUI.ItemUIInit();
            itemUIList.Add(itemUI);
        }
    }

    protected override void SetDescriptionPosition(int index)
    {
        Transform itemUITrans = itemUIList[index].transform;
        Vector3 targetPos = itemUITrans.position;
        //targetPos.x -= (itemUITrans as RectTransform).rect.width / 2 + 
        //    (itemDescription.transform as RectTransform).rect.width / 2;
        targetPos.x -= (itemUITrans as RectTransform).rect.width / 2;
        itemDescription.SetPosition(targetPos, ItemDescription.EPos.Left);
    }

    protected override void SetItemActionPosition(InputActionPanel panel, 
        int index)
    {
        // 这里不知道为什么要等两帧，但等两帧才能将面板显示在正确位置
        //yield return null;
        //yield return null;

        Transform itemUITrans = itemUIList[index].transform;
        Vector3 targetPos = itemUITrans.position;
        //targetPos.x -= (itemUITrans as RectTransform).rect.width / 2 + 
        //    (panel.inputActionBG as RectTransform).rect.width / 2;
        targetPos.x -= (itemUITrans as RectTransform).rect.width / 2;
        panel.SetPosition(targetPos, InputActionPanel.EPos.Left);
    }
}
