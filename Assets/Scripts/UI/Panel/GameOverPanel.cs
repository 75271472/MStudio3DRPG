using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private CanvasGroup thankCanvasGroup;
    [SerializeField] private CanvasGroup btnCanvasGroup;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button backMenuBtn;
    [SerializeField] private float fadeTime;

    private void Start()
    {
        ShowMe();
    }

    public override void ShowMe()
    {
        base.ShowMe();

        ResetCanvas();

        continueBtn.onClick.AddListener(() =>
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutPanelCoroutine());
        });

        backMenuBtn.onClick.AddListener(() =>
        {
            // 先删除所有面板在进行场景加载
            // 因为场景加载时也会创建面板，先进行场景加载在删除面板会吧场景加载面板也删除
            LoadSceneManager.Instance.LoadSceneAsync(DataManager.STARTSCENE);
        });

        StopAllCoroutines();
        StartCoroutine(FadeInPanelCoroutine());
    }

    private IEnumerator FadeInPanelCoroutine()
    {
        // 淡入面板
        yield return FadeInCoroutine(panelCanvasGroup);
        // 淡入淡出gameovertext
        yield return FadeInCoroutine(gameOverCanvasGroup);
        yield return FadeOutCoroutine(gameOverCanvasGroup);
        // 淡入淡出thanktext
        yield return FadeInCoroutine(thankCanvasGroup);
        yield return FadeOutCoroutine(thankCanvasGroup);
        // 淡入btngroup
        yield return FadeInCoroutine(btnCanvasGroup);
    }

    private IEnumerator FadeOutPanelCoroutine()
    {
        yield return FadeOutCoroutine(btnCanvasGroup);
        UIManager.Instance.HidePanel<GameOverPanel>();
    }

    private IEnumerator FadeInCoroutine(CanvasGroup canvasGroup)
    {
        float progressTime = 0;
        while (progressTime < fadeTime)
        {
            progressTime += Time.deltaTime;
            canvasGroup.alpha = progressTime / fadeTime;
            yield return null;
        }
    }

    private IEnumerator FadeOutCoroutine(CanvasGroup canvasGroup)
    {
        float progressTime = 0;
        while (progressTime < fadeTime)
        {
            progressTime += Time.deltaTime;
            canvasGroup.alpha = 1 - progressTime / fadeTime;
            yield return null;
        }
    }

    private void ResetCanvas()
    {
        gameOverCanvasGroup.alpha = 0;
        thankCanvasGroup.alpha = 0;
        btnCanvasGroup.alpha = 0;
        panelCanvasGroup.alpha = 0;
    }
}
