using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoTxtUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text infoTxt;

    public float showTime = 3f;

    public void ResetInfo()
    {
        infoTxt.text = string.Empty;
        StopAllCoroutines();
    }

    public void UpdateInfo(string info, Action<InfoTxtUI> callback)
    {
        infoTxt.text = info;

        StopAllCoroutines();
        StartCoroutine(UpdateGroupAlpha(callback));
    }

    private IEnumerator UpdateGroupAlpha(Action<InfoTxtUI> callback)
    {
        float time = 0;

        while (time < showTime)
        {
            canvasGroup.alpha = 1 - time / showTime;
            time += Time.deltaTime;

            yield return null;
        }

        callback?.Invoke(this);
    }
}
