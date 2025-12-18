using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ArchivePanel : BasePanel
{
    public float AlphaTime { get; set; } = 3f;

    [SerializeField] private Button quitBtn;
    [SerializeField] private PlayableDirector backToMainPanelDirector;
    [SerializeField] private PlayableDirector continueGameDirector;
    [SerializeField] private StartPanel startPanel;

    [SerializeField] private List<ArchiveUI> archiveUIList = new List<ArchiveUI>();

    private CanvasGroup canvasGroup;

    private void Start()
    {
        RemoveBtnListener();

        canvasGroup = GetComponent<CanvasGroup>();

        SwitchCanvasGroup(false);

        quitBtn.onClick.AddListener(BackToMainPanel);

        for (int i = 0; i < archiveUIList.Count; i++)
        {
            archiveUIList[i].RemoveAllUnityAction();
            archiveUIList[i].OnChoseBtnClickEvent += OnChoseBtnClickHandler;
            archiveUIList[i].OnDeleteBtnClickEvent += OnDeleteBtnClickHandler;
            archiveUIList[i].ArchiveUIInit(i + 1);
        }
    }

    private void RemoveBtnListener()
    {
        quitBtn.onClick.RemoveAllListeners();
    }

    private void BackToMainPanel()
    {
        HideMe();

        backToMainPanelDirector.stopped += (o) =>
        {
            startPanel.ShowMe();
        };

        backToMainPanelDirector.Play();
    }

    public override void ShowMe()
    {
        // 设置CanvasGroup为隐藏状态
        // 当完全显示时才能进行射线检测和事件互动
        SwitchCanvasGroup(false);

        StopAllCoroutines();
        StartCoroutine(UpdateAlphaCoroutine(true, () =>
        {
            SwitchCanvasGroup(true);
        }));

        UpdateArchiveUI();
    }

    public override void HideMe()
    {
        // 设置CanvasGroup为隐藏状态
        SwitchCanvasGroup(false);

        StopAllCoroutines();
        StartCoroutine(UpdateAlphaCoroutine(false));
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

    /// <summary>
    /// 调整CanvasGroup状态
    /// </summary>
    /// <param name="isShow"></param>
    private void SwitchCanvasGroup(bool isShow)
    {
        canvasGroup.alpha = isShow ? 1 : 0;
        canvasGroup.interactable = isShow;
        canvasGroup.blocksRaycasts = isShow;
    }

    private void UpdateArchiveUI()
    {
        foreach (var archiveUI in archiveUIList)
        {
            archiveUI.ResetArchive();

            PlayerInfo playerInfo = DataManager.Instance.
                GetPlayerInfoByIndex(archiveUI.ArchiveIndex);

            if (playerInfo == null) continue;

            archiveUI.UpdateArchive(
                playerInfo.playerTransInfo, playerInfo.playerStateInfo
            );
        };
    }

    private void OnChoseBtnClickHandler(int archiveIndex)
    {
        HideMe();

        // 如果选择存档为空存档
        if (DataManager.Instance.UpdateEmptyArchiveIndex(archiveIndex))
        {
            LoadNewGame();
            return;
        }

        DataManager.Instance.LoadPlayerData(archiveIndex);

        continueGameDirector.stopped += (o) =>
        {
            LoadSceneManager.Instance.LoadSceneAsync(
                DataManager.Instance.PlayerInfo.playerTransInfo.SceneName);
        };

        continueGameDirector.Play();
    }

    private void OnDeleteBtnClickHandler(int archiveIndex)
    {
        UIManager.Instance.ShowPanel<TipPanel>().UpdateTipTxt("是否删除存档？",
            () => {
                DataManager.Instance.DeletePlayerDataInPersistentData(archiveIndex);

                UpdateArchiveUI();
            });
    }

    private void LoadNewGame()
    {
        continueGameDirector.stopped += (o) =>
        {
            LoadSceneManager.Instance.LoadSceneAsync(DataManager.FIRSTSCENE,
            () => PlayerManager.Instance.PlayerTransInit());
        };

        continueGameDirector.Play();
    }
}
