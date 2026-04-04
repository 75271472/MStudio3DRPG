using System;
using UnityEngine;

public class InventoryReddotHandler
{
    private const string HOTBAR_PREFIX = "Main/Hotbar/";
    private const string INVENTORY_PREFIX = "Main/BagTask/Inventory/";

    private InventoryController boundController;

    public void Init()
    {
        BindController(PlayerManager.Instance.PlayerData.InventoryController);
    }

    public void BindController(InventoryController inventoryController)
    {
        if (boundController != null)
        {
            boundController.OnItemAddedEvent -= OnItemAdded;
        }

        boundController = inventoryController;
        if (boundController != null)
        {
            boundController.OnItemAddedEvent += OnItemAdded;
        }
    }

    public void OnDestroy()
    {
        if (boundController != null)
        {
            boundController.OnItemAddedEvent -= OnItemAdded;
        }
    }

    private void OnItemAdded(IndexInfo indexInfo)
    {
        if (indexInfo.panelType == EInventoryPanel.Hotbar)
        {
            ReddotManager.Instance.ChangeValue(HOTBAR_PREFIX + indexInfo.index, 1);
        }
        else if (indexInfo.panelType == EInventoryPanel.Inventory)
        {
            ReddotManager.Instance.ChangeValue(INVENTORY_PREFIX + indexInfo.index, 1);
        }
    }

    public void MarkItemRead(IndexInfo indexInfo)
    {
        if (indexInfo.panelType == EInventoryPanel.Hotbar)
        {
            ReddotManager.Instance.ChangeValue(HOTBAR_PREFIX + indexInfo.index, 0);
        }
        else if (indexInfo.panelType == EInventoryPanel.Inventory)
        {
            ReddotManager.Instance.ChangeValue(INVENTORY_PREFIX + indexInfo.index, 0);
        }
    }
}
