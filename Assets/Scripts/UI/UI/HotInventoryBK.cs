using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HotInventoryBK : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action OnPointerEnterEvent, OnPointerExitEvent; 

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke();
    }
}
