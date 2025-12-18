using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseItemUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] protected Image itemImg;
    [SerializeField] protected Text quantityTxt;

    public event Action<BaseItemUI> OnLeftClickedEvent, OnRightClickedEvent;
    
    protected bool isEmpty = false;

    public virtual void ItemUIInit()
    {
        ResetData();
    }

    public virtual void ResetData()
    {
        itemImg.gameObject.SetActive(false);
        isEmpty = true;
    }

    public virtual void SetData(Sprite sprite, int quantity)
    {
        itemImg.gameObject.SetActive(true);
        itemImg.sprite = sprite;
        quantityTxt.text = quantity.ToString();
        isEmpty = false;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
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
}
