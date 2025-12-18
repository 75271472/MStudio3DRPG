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
        base.ShowMe();

        canvasGroup = GetComponent<CanvasGroup>();
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
    }

    public void UpdateLoadSceneSlider(float progress)
    {
        slider.value = progress;
    }
}
