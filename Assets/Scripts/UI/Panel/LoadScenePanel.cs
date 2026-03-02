using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadScenePanel : BasePanel
{
    [SerializeField] private Slider slider;
    private CanvasGroup canvasGroup;

    public override void ShowMe()
    {
        Debug.Log("Show Load Scene Panel");
        base.ShowMe();
        UpdateLoadSceneSlider(0);
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public override void HideMe()
    {
        base.HideMe();
        Debug.Log("Hide Load Scene Panel");
    }

    public void FadeIn(float fadeTime)
    {
        StartCoroutine(FadeInCoroutine(fadeTime));
    }

    public void FadeOut(float fadeTime)
    {
        StartCoroutine(FadeOutCoroutine(fadeTime));
    }

    public IEnumerator FadeInCoroutine(float fadeTime)
    {
        float progressTime = 0;
        while (progressTime < fadeTime)
        {
            progressTime += Time.deltaTime;
            canvasGroup.alpha = progressTime / fadeTime;
            yield return null;
        }
    }

    public IEnumerator FadeOutCoroutine(float fadeTime)
    {
        float progressTime = 0;
        while (progressTime < fadeTime)
        {
            progressTime += Time.deltaTime;
            canvasGroup.alpha = 1 - progressTime / fadeTime;
            yield return null;
        }

        UIManager.Instance.HidePanel<LoadScenePanel>();
    }

    public void UpdateLoadSceneSlider(float progress)
    {
        slider.value = progress;
    }
}
