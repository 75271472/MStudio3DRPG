using System;
using UnityEngine;

public class Main : MonoBehaviour
{
    [Header("UI面板引用")]
    [SerializeField] private StartPanel startPanel;
    [SerializeField] private ABUpdatePanel updatePanel;

    private void Start()
    {
        Debug.Log("【Main】游戏入口启动，开始热更新检测...");

        // 注册事件
        HotUpdateManager.Instance.OnFoundUpdateEvent += OnFoundUpdate;
        HotUpdateManager.Instance.OnUpdateProgressEvent += OnUpdateProgress;
        HotUpdateManager.Instance.OnUpdateErrorEvent += OnUpdateError;
        HotUpdateManager.Instance.OnHotUpdateComplete += OnHotUpdateCompleted;

        // 显示正在检测
        if (updatePanel != null)
        {
            updatePanel.ShowMe();
            updatePanel.SetTipTxt("正在连接服务器获取版本信息...");
        }

        // 启动热更检测
        HotUpdateManager.Instance.StartHotUpdate();
    }

    private void OnFoundUpdate(float totalSizeMB)
    {
        if (updatePanel == null) return;

        updatePanel.SetTipTxt($"发现新版本资源，共计 {totalSizeMB:F2} KB，是否下载？");
        updatePanel.ShowBtnGroupForSelectUpdate();

        updatePanel.OnUpdateSelectEvent = (isUpdate) =>
        {
            if (isUpdate)
            {
                updatePanel.SetTipTxt("准备下载中...");
                updatePanel.ShowProgress();
                HotUpdateManager.Instance.ConfirmDownload();
            }
            else
            {
                // 选择取消更新，退出游戏
                Application.Quit();
            }
        };
    }

    private void OnUpdateProgress(float progress)
    {
        if (updatePanel == null) return;

        updatePanel.UpdateProgress(progress);
        updatePanel.SetTipTxt($"正在下载更新... {(progress * 100):F1}%");
    }

    private void OnUpdateError()
    {
        if (updatePanel == null) return;

        updatePanel.SetTipTxt("网络连接异常，是否重试获取？");
        updatePanel.ShowBtnGroupForRetry();

        updatePanel.OnRetrySelectEvent = (isRetry) =>
        {
            if (isRetry)
            {
                updatePanel.SetTipTxt("正在重新连接服务器...");
                // 重新触发完整热更检测流程
                HotUpdateManager.Instance.StartHotUpdate();
            }
            else
            {
                Application.Quit();
            }
        };
    }

    private void OnHotUpdateCompleted()
    {
        Debug.Log("【Main】接收到热更新完成信号，准备显示初始UI面板。");

        if (updatePanel != null)
        {
            updatePanel.SetTipTxt("资源准备完毕！");
            updatePanel.ShowBtnGroupForComplete();
            updatePanel.UpdateProgress(1f);

            updatePanel.OnCompletedEvent = () =>
            {
                updatePanel.HideMe();
                TryShowStartPanel();
            };
        }
        else
        {
            TryShowStartPanel();
        }
    }

    private void TryShowStartPanel()
    {
        if (startPanel != null)
        {
            startPanel.ShowMe();
        }
        else
        {
            StartPanel panel = FindObjectOfType<StartPanel>(true);
            if (panel != null)
                panel.ShowMe();
            else
                Debug.LogWarning("未找到 StartPanel，无法展现初始界面！");
        }
    }

    private void OnDestroy()
    {
        if (HotUpdateManager.Instance != null)
        {
            HotUpdateManager.Instance.OnFoundUpdateEvent -= OnFoundUpdate;
            HotUpdateManager.Instance.OnUpdateProgressEvent -= OnUpdateProgress;
            HotUpdateManager.Instance.OnUpdateErrorEvent -= OnUpdateError;
            HotUpdateManager.Instance.OnHotUpdateComplete -= OnHotUpdateCompleted;
        }
    } // Ensure cleanup when scene closes
}
