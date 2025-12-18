using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescription : BasePanel
{
    public enum EPos
    {
        Left,
        Right,
        Top,
        Bottom,
    }

    //[SerializeField] private Canvas canvas;
    [SerializeField] private Text titleTxt;
    [SerializeField] private Text descriptionTxt;

    //private RectTransform rectTransform;

    public override void ShowMe()
    {
        ItemDescriptionInit();
        ResetDescription();

        ItemDescriptionSwitch(true);
    }

    public override void HideMe()
    {
        ItemDescriptionSwitch(false);
    }

    public void ItemDescriptionInit()
    {
        //canvas = transform.root.GetComponent<Canvas>();
        //rectTransform = transform as RectTransform;

        ResetDescription();
    }

    public void SetDescription(string itemName, string itemDescription)
    {
        ItemDescriptionSwitch(true);
        titleTxt.text = itemName;
        descriptionTxt.text = itemDescription;

        // 3. 【关键修改】启动协程，在帧末强制刷新布局
        // 必须在设置完文字之后调用
        if (gameObject.activeInHierarchy)
        {
            StopAllCoroutines(); // 防止快速切换导致协程冲突
            StartCoroutine(UpdateLayoutCoroutine());
        }
    }

    public void ResetDescription()
    {
        ItemDescriptionSwitch(false);
        titleTxt.text = string.Empty;
        descriptionTxt.text = string.Empty;
    }

    public void ItemDescriptionSwitch(bool isActive)
    {
        //print("ItemDescriptionSwitch" +  isActive);
        gameObject.SetActive(isActive);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="pos">面板显示在物体的哪个方向</param>
    public void SetPosition(Vector3 position, EPos pos)
    {
        SetPivot(pos);
        position.z = 0;
        transform.position = position;
    }

    private void SetPivot(EPos pos)
    {
        RectTransform rt = transform as RectTransform;
        Vector2? newPivot = pos switch
        {
            EPos.Left => new Vector2(1, 0.5f),
            EPos.Right => new Vector2(0, 0.5f),
            EPos.Top => new Vector2(0.5f, 0),
            EPos.Bottom => new Vector2(0.5f, 1),
            _ => null
        };

        if (!newPivot.HasValue) return;

        rt.pivot = newPivot.Value;
    }

    // --- 新增的布局刷新协程 ---
    private IEnumerator UpdateLayoutCoroutine()
    {
        // 等待当前帧结束，确保 Text 组件已经拿到了新的字符串并计算了基本的网格信息
        yield return new WaitForEndOfFrame();

        // 强制刷新自身布局 (ContentSizeFitter 会在这里计算正确大小)
        ExtensionTool.UpdateUI(transform);
    }
}
