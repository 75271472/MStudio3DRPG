using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NoticePanel : BasePanel
{
    [SerializeField] private float alphaTime;
    [SerializeField] private Text tipTxt;
    private CanvasGroup canvasGroup;

    public override void ShowMe()
    {
        base.ShowMe();

        ClearTipTxt();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
    }

    public void ClearTipTxt()
    {
        tipTxt.text = "";
    }

    public void UpdateTipTxt(string tipStr)
    {
        if (string.IsNullOrEmpty(tipStr)) return;

        tipTxt.text = tipStr;

        StopAllCoroutines();
        StartCoroutine(UpdateAlphaCoroutine(
            UIManager.Instance.HidePanel<NoticePanel>));
    }

    private IEnumerator UpdateAlphaCoroutine(Action alphaAction)
    {
        canvasGroup.alpha = 1;
        float elapsedTime = 0;

        while (elapsedTime <= alphaTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime /  alphaTime);
            yield return null;
        }

        canvasGroup.alpha = 0;
        alphaAction?.Invoke();
    }
}
