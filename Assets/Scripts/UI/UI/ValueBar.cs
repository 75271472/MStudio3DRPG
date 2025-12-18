using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueBar : MonoBehaviour
{
    [field: SerializeField] public float buffTime;
    // 该值大于零，表示更新血条时修改血条透明度
    [field: SerializeField] public float alphaTime;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image effectBar;
    [SerializeField] private Image bar;

    public void ValueBarInit(int value, int maxValue)
    {
        if (alphaTime > 0 && canvasGroup != null)
            canvasGroup.alpha = 0;

        UpdateValueWithOutCoroutine(value, maxValue);
    }

    public void UpdateValueWithOutCoroutine(int value, int maxValue)
    {
        bar.fillAmount = Mathf.Clamp01((float)value / maxValue);
        effectBar.fillAmount = bar.fillAmount;
    }

    public void UpdateValueBar(int value, int maxValue, 
        Action<GameObject> alphaAction = null)
    {
        StopAllCoroutines();
        StartCoroutine(UpdateValueBarCoroutine(value, maxValue));

        if (alphaTime <= 0 || canvasGroup == null) return;
        StartCoroutine(UpdateAlphaCoroutine(alphaAction));
    }

    private IEnumerator UpdateValueBarCoroutine(int value, int maxValue)
    {
        float barFillAmount = Mathf.Clamp01((float)value / maxValue);
        float difference = bar.fillAmount - barFillAmount;

        if (difference == 0) yield break;

        float elapsedTime = 0;

        // 小于零说明bar要增长
        if (difference < 0)
        {
            effectBar.fillAmount = barFillAmount;

            while (elapsedTime < buffTime)
            {
                elapsedTime += Time.deltaTime;
                bar.fillAmount = Mathf.Lerp(
                    effectBar.fillAmount + difference,
                    effectBar.fillAmount, elapsedTime / buffTime);

                yield return null;
            }

            bar.fillAmount = effectBar.fillAmount;
        }
        // 大于零说明bar要减少
        else
        {
            bar.fillAmount = barFillAmount;

            while (elapsedTime < buffTime)
            {
                elapsedTime += Time.deltaTime;
                effectBar.fillAmount = Mathf.Lerp(
                    bar.fillAmount + difference,
                    bar.fillAmount, elapsedTime / buffTime);

                yield return null;
            }

            effectBar.fillAmount = bar.fillAmount;
        }
    }

    private IEnumerator UpdateAlphaCoroutine(
        Action<GameObject> alphaAction = null)
    {
        canvasGroup.alpha = 1;

        float elapsedTime = 0;

        while (elapsedTime < alphaTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / alphaTime);

            yield return null;
        }

        canvasGroup.alpha = 0;
        alphaAction?.Invoke(gameObject);
    }

    public void UpdatePos(Vector3 pos) => this.transform.position = pos;
}
