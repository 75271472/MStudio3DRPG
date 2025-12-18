using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiePanel : BasePanel
{
    [SerializeField] private Button backToStartSceneBtn;
    [SerializeField] private Button continueBtn;
    [SerializeField] private float AlphaTime = 3f;

    private CanvasGroup canvasGroup;

    public override void ShowMe()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        continueBtn.onClick.RemoveAllListeners();
        backToStartSceneBtn.onClick.RemoveAllListeners();

        continueBtn.onClick.AddListener(() =>
        {
            int archiveIndex = DataManager.Instance.ArchiveIndex;

            // 本存档之前没有保存过，
            if (DataManager.Instance.UpdateEmptyArchiveIndex(archiveIndex))
            {
                // 重新进行场景加载
                LoadSceneManager.Instance.LoadSceneAsync(DataManager.FIRSTSCENE,
                    () => PlayerManager.Instance.PlayerTransInit());
                return;
            }

            // 之前保存过
            DataManager.Instance.LoadPlayerData(archiveIndex);

            LoadSceneManager.Instance.LoadSceneAsync(
                DataManager.Instance.PlayerInfo.playerTransInfo.SceneName);
        });

        backToStartSceneBtn.onClick.AddListener(() =>
        {
            // 先删除所有面板在进行场景加载
            // 因为场景加载时也会创建面板，先进行场景加载在删除面板会吧场景加载面板也删除
            LoadSceneManager.Instance.LoadSceneAsync(DataManager.STARTSCENE);
        });

        StopAllCoroutines();
        StartCoroutine(UpdateAlphaCoroutine(true));
    }

    private IEnumerator UpdateAlphaCoroutine(bool isShow, Action alphaAction = null)
    {
        canvasGroup.alpha = isShow ? 0 : 1;
        float originAlpha = isShow ? 0 : 1;
        float targetAlpha = isShow ? 1 : 0;
        float elapsedTime = 0;

        while (elapsedTime <= AlphaTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(originAlpha, targetAlpha,
                elapsedTime / AlphaTime);

            yield return null;
        }

        alphaAction?.Invoke();
    }
}
