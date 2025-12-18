using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryBK : MonoBehaviour, IPointerClickHandler
{
    public event Action OnPointerClickEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerClickEvent?.Invoke();
        print("OnPointClick");
    }
}
