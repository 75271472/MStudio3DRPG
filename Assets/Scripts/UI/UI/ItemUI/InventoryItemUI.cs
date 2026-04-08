using System;
using UnityEngine.EventSystems;

public class InventoryItemUI : BaseInventoryItemUI, IPointerEnterHandler, 
    IPointerExitHandler
{
    public event Action<InventoryItemUI> OnPointerEnterEvent, OnPointerExitEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isEmpty) return;

        OnPointerEnterEvent?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke(this);
    }
}
