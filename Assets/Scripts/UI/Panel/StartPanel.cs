using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class StartPanel : BasePanel
{
    [SerializeField] private Button startBtn;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button archiveBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private PlayableDirector startGameDirector;
    [SerializeField] private PlayableDirector turnToArchiveDirector;
    
    [SerializeField] private ArchivePanel archivePanel;

    private CanvasGroup canvasGroup;

    public void Start()
    {
        //// 强制清理所有 AB 包缓存
        //Caching.ClearCache();
        //// 强制清理 Addressables 的 Catalog 缓存
        //UnityEngine.AddressableAssets.Addressables.CleanBundleCache();
        // 热更新加载完毕后，在启动游戏
        // Only update catalogs manually保证仅手动更新勾选
        // 硬更新
        StartCoroutine(HotUpdateManager.Instance.HotUpdateCoroutine(ShowMe));
        // Only update catalogs manually保证仅手动更新没有勾选，
        // 则Addressable从服务器下载数据和游戏加载数据并行
        // 可能下次游戏启动时才能看到资源更新
        // 软更新
        //ShowMe();
    }

    private void RemoveBtnListener()
    {
        startBtn.onClick.RemoveAllListeners();
        archiveBtn.onClick.RemoveAllListeners();
        quitBtn.onClick.RemoveAllListeners();
    }

    public override void ShowMe()
    {
        RemoveBtnListener();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        startBtn.onClick.AddListener(LoadNewGame);
        continueBtn.onClick.AddListener(ContinueGame);
        archiveBtn.onClick.AddListener(TurnToArchive);
        quitBtn.onClick.AddListener(QuitGame);

        // 当前有可用的空存档，就激活StartBtn
        startBtn.gameObject.SetActive(DataManager.Instance.UpdateEmptyArchiveIndex());
        // 当前有ArchiveIndex数据存储，就激活ContinueBtn
        continueBtn.gameObject.SetActive(DataManager.Instance.LoadArchiveIndex());

        SwitchCanvasGroup(true);
    }

    public override void HideMe()
    {
        SwitchCanvasGroup(false);
    }

    private void SwitchCanvasGroup(bool isShow)
    {
        if (canvasGroup == null) return;

        canvasGroup.interactable = isShow;
        canvasGroup.blocksRaycasts = isShow;
    }

    private void LoadNewGame()
    {
        // 这一步是必不可少的，因为Show函数中调用UpdateEmptyArchiveIndex后
        // 有调用了LoadArchiveIndex，会更新DataManager的ArchiveIndex
        // 此时如果直接加载进入新游戏虽然DataManager加载了DefaultPlayerStateInfo
        // 但ArchiveIndex是旧存档的ArchiveIndex
        // 存档检测更新ArchiveIndex
        if (!DataManager.Instance.UpdateEmptyArchiveIndex())
        {
            Debug.LogError("没有可用的空存档");
            return;
        }

        HideMe();

        startGameDirector.stopped += (o) =>
        {
            LoadSceneManager.Instance.LoadSceneAsync(DataManager.FIRSTSCENE,
                () => {
                    PlayerManager.Instance.PlayerTransInit();
                    PlayerManager.Instance.PlayerDialogueTrigger();
                });
        };

        startGameDirector.Play();
    }

    private void ContinueGame()
    {
        if (!DataManager.Instance.LoadArchiveIndex())
        {
            Debug.LogError("无可用存档下标");
            return;
        }

        HideMe();

        DataManager.Instance.LoadPlayerData();

        startGameDirector.stopped += (o) =>
        {
            LoadSceneManager.Instance.LoadSceneAsync(
                DataManager.Instance.PlayerInfo.playerTransInfo.SceneName);
        };

        startGameDirector.Play();
    }

    private void TurnToArchive()
    {
        HideMe();

        archivePanel.AlphaTime = (float)turnToArchiveDirector.duration;
        archivePanel.ShowMe();

        turnToArchiveDirector.Play();
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
