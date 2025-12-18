using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollower : BasePanel
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private InventoryItemUI itemUI;

    public override void ShowMe()
    {
        MouseFollowerInit();

        MouseFollowerSwitch(true);
    }

    public override void HideMe()
    {
        MouseFollowerSwitch(false);
    }

    public void MouseFollowerInit()
    {
        canvas = transform.root.GetComponent<Canvas>();

        itemUI = GetComponentInChildren<InventoryItemUI>();
        itemUI.Deselect();

        //MouseFollowerSwitch(false);
    }

    public void SetData(Sprite sprite, int quality)
    {
        itemUI.SetData(sprite, quality);
    }

    private void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, Input.mousePosition,
            canvas.worldCamera, out var pointOnCanvas
        );

        //transform.position = canvas.transform.TransformPoint(pointOnCanvas);
        (transform as RectTransform).anchoredPosition = pointOnCanvas;
    }

    public void MouseFollowerSwitch(bool isActive)
    {
        gameObject.SetActive( isActive );
    }
}
