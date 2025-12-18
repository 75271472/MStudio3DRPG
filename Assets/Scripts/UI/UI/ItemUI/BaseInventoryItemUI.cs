using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseInventoryItemUI : BaseItemUI, IBeginDragHandler,
    IEndDragHandler, IDropHandler, IDragHandler
{
    [SerializeField] protected Image borderImg;

    public new event Action<BaseInventoryItemUI> OnLeftClickedEvent, 
        OnRightClickedEvent;
    public event Action<BaseInventoryItemUI> OnItemDroppedEvent, 
        OnItemBeginDragEvent, OnItemEndDragEvent;

    public override void ItemUIInit()
    {
        base.ItemUIInit();

        Deselect();
    }
    public void Deselect()
    {
        borderImg.enabled = false;
    }

    public void Select()
    {
        borderImg.enabled = true;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClickedEvent?.Invoke(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClickedEvent?.Invoke(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isEmpty) return;
        OnItemBeginDragEvent?.Invoke(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnItemEndDragEvent?.Invoke(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnItemDroppedEvent?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData) { }
}
