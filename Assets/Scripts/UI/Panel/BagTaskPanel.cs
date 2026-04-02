using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EBagTaskType
{
    None,
    Bag,
    Task
}

public class BagTaskPanel : BasePanel
{
    [field: SerializeField] public Button BagBtn { get; set; }
    [field: SerializeField] public Button TaskBtn { get; set; }

    // 当前显示的子界面类型
    public EBagTaskType CurrentType { get; private set; } = EBagTaskType.None;

    public override void ShowMe()
    {
        base.ShowMe();

        // 初始化按钮事件
        BagBtn.onClick.RemoveAllListeners();
        BagBtn.onClick.AddListener(ShowBag);

        TaskBtn.onClick.RemoveAllListeners();
        TaskBtn.onClick.AddListener(ShowTask);
    }

    #region 按键触发转发逻辑

    /// <summary>
    /// 背包快捷键联动逻辑 (Tab)
    /// </summary>
    public void ToggleBag()
    {
        // 如果当前已经打开了背包，则重复按键应关闭整个面板
        if (CurrentType == EBagTaskType.Bag)
        {
            HideBagTaskPanel();
        }
        else
        {
            ShowBag();
        }
    }

    /// <summary>
    /// 任务快捷键联动逻辑 (J)
    /// </summary>
    public void ToggleTask()
    {
        // 如果当前已经打开了任务，则重复按键应关闭整个面板
        if (CurrentType == EBagTaskType.Task)
        {
            HideBagTaskPanel();
        }
        else
        {
            ShowTask();
        }
    }

    #endregion

    #region 子界面显示切换

    private void ShowBag()
    {
        // 如果已经在背包页签，则无反应
        if (CurrentType == EBagTaskType.Bag) return;

        var playerData = PlayerManager.Instance.PlayerData;

        // 1. 关闭正在显示的任务面板
        if (playerData.QuestController.IsPanelOpen)
            playerData.QuestController.SwitchPanel();

        // 2. 开启背包面板
        if (!playerData.InventoryController.IsPanelOpen)
            playerData.InventoryController.SwitchPanel();

        CurrentType = EBagTaskType.Bag;
    }

    private void ShowTask()
    {
        // 如果已经在任务页签，则无反应
        if (CurrentType == EBagTaskType.Task) return;

        var playerData = PlayerManager.Instance.PlayerData;

        // 1. 关闭正在显示的背包面板
        if (playerData.InventoryController.IsPanelOpen)
            playerData.InventoryController.SwitchPanel();

        // 2. 开启任务面板
        if (!playerData.QuestController.IsPanelOpen)
            playerData.QuestController.SwitchPanel();

        CurrentType = EBagTaskType.Task;
    }

    /// <summary>
    /// 关闭整合面板及所有开启的子面板
    /// </summary>
    private void HideBagTaskPanel()
    {
        var playerData = PlayerManager.Instance.PlayerData;

        if (playerData.InventoryController.IsPanelOpen)
            playerData.InventoryController.SwitchPanel();
        if (playerData.QuestController.IsPanelOpen)
            playerData.QuestController.SwitchPanel();

        // 重置状态
        CurrentType = EBagTaskType.None;
        UIManager.Instance.HidePanel<BagTaskPanel>();
    }

    #endregion
}
