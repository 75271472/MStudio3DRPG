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
    protected IResource imgRes;

    public virtual void ItemUIInit()
    {
        ResetData();
    }

    public virtual void ResetData()
    {
        itemImg.gameObject.SetActive(false);
        itemImg.sprite = null;
        isEmpty = true;

        if (imgRes != null)
        {
            ResourcesManager.Instance.Unload(imgRes);
            imgRes = null;
        }
    }

    public virtual void SetData(string imgPath, int quantity)
    {
        if (imgRes != null)
        {
            ResourcesManager.Instance.Unload(imgRes);
            imgRes = null;
        }

        if (!string.IsNullOrEmpty(imgPath))
        {
            string fullUrl = ResourcesManager.Instance.GetFullUrl(imgPath);
            imgRes = ResourcesManager.Instance.Load(fullUrl, false);
            itemImg.sprite = imgRes.GetAsset<Sprite>();
        }

        itemImg.gameObject.SetActive(true);
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

    protected virtual void OnDestroy()
    {
        if (imgRes != null)
        {
            ResourcesManager.Instance.Unload(imgRes);
            imgRes = null;
        }
    }
}
