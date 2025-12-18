using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputActionPanel : BasePanel
{
    public enum EPos
    {
        Left,
        Right,
        Top,
        Bottom,
    }

    // 因为PlayerPostion不是位于Player的视觉中央，因此要对PlayerPosition进行偏移
    private Vector2 playerOffsetVect = new Vector2(-0.2f, 0);
    // 通过偏移后的PlayerPosition在进行偏移，获得面板的显示位置
    private Vector3 panelOffsetVect = new Vector3(-200, 50, 0);
    [field: SerializeField] public Transform inputActionBG { get; private set; } 
    private Dictionary<string, InputActionUI> actionDict = 
        new Dictionary<string, InputActionUI>();

    public override void ShowMe()
    {
        base.ShowMe();

        ClearInput();
    }

    public override void HideMe()
    {
        base.HideMe();

        ClearInput();
    }

    public void SetPosition(Vector3 position, EPos pos)
    {
        SetPivot(pos);

        position.z = 0;
        inputActionBG.position = position;
    }

    public void SetPosNextToPlayer()
    {
        Transform cameraTrans = Camera.main.transform;

        Vector3 playerPos = PlayerManager.Instance.gameObject.transform.position + 
            // 偏移是相对摄像机的观察方向的
            cameraTrans.right * playerOffsetVect.x + cameraTrans.up * playerOffsetVect.y;
        Vector3 targetPos = Camera.main.WorldToScreenPoint(playerPos) + 
            panelOffsetVect;

        SetPosition(targetPos, EPos.Left);
    }

    public void AddInput(string actionName, UnityAction action)
    {
        if (actionDict.ContainsKey(actionName)) return;

        GameObject inputActionObj = PoolManager.Instance.PullObj(
            DataManager.INPUTACTIONING);
        inputActionObj.transform.SetParent(inputActionBG, false);
        InputActionUI inputActionUI = inputActionObj.GetComponent<InputActionUI>();
        
        inputActionUI.ResetAction();
        inputActionUI.SetAction(actionName, action);

        actionDict.Add(actionName, inputActionUI);

        // 3.【关键修改】启动协程，在帧末强制刷新布局
        // 必须在设置完文字之后调用
        if (gameObject.activeInHierarchy)
        {
            StopAllCoroutines(); // 防止快速切换导致协程冲突
            StartCoroutine(UpdateLayoutCoroutine());
        }
    }

    public void ClearInput()
    {
        foreach (var actionUI in actionDict.Values)
        {
            PoolManager.Instance.PushObj(DataManager.INPUTACTIONING, 
                actionUI.gameObject);
        }

        actionDict.Clear();
    }

    private void SetPivot(EPos pos)
    {
        RectTransform rt = inputActionBG as RectTransform;
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
