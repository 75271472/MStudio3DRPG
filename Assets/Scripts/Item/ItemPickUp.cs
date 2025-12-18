using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    private InventoryController inventoryController;
    private const string InputActionStr = "PickUp";

    public void ItemPickUpInit(InventoryController inventoryController)
    {
        this.inventoryController = inventoryController;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Item>(out var item))
        {
            //print("Pick up: " + item.ItemInfo.name);

            UIManager.Instance.ShowPanel<InputActionPanel>().AddInput(
                InputActionStr, () =>
                {
                    item.ChangeQuantity(inventoryController.AddItem(
                    item.ItemType, item.ItemId, item.Quantity));
                    UIManager.Instance.HidePanel<InputActionPanel>();
                });
            UIManager.Instance.GetPanel<InputActionPanel>().SetPosNextToPlayer();
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Item>(out var item))
        {
            UIManager.Instance.HidePanel<InputActionPanel>();
        }
    }
}
