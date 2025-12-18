using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class StateItemUI : BaseInventoryItemUI
{
    [field: SerializeField] public EEquippableItem EquippableItemType;

    [SerializeField] private Sprite defaultSprite;

    //public event Action<EEquippableItem, StateItemUI> OnEquipEvent, OnUnloadEvent;

    public override void ItemUIInit()
    {
        base.ItemUIInit();
        quantityTxt.transform.parent.gameObject.SetActive(false);
    }

    public override void ResetData()
    {
        // 当前没有物品不执行ResetData，避免外部调用ReestAllItems频繁触发OnUnloadEvent
        if (isEmpty) return;
        itemImg.sprite = defaultSprite;

        isEmpty = true;
        //OnUnloadEvent?.Invoke(EquippableItemType, this);
    }

    public override void SetData(Sprite sprite, int quantity)
    {
        itemImg.sprite = sprite;

        isEmpty = false;
        //OnEquipEvent?.Invoke(EquippableItemType, this);
    }
}
