using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RequireUIPayload
{
    public string questName;
    public string description;
    public List<Tuple<string, int, int>> questProgressList;
    public List<Tuple<Sprite, int>> rewardItemInfoList;

    public RequireUIPayload(Quest quest)
    {
        this.questName = quest.questName;
        this.description = quest.description;
        this.questProgressList = new List<Tuple<string, int, int>>();
        this.rewardItemInfoList = new List<Tuple<Sprite, int>>();

        foreach (var questRequire in quest.questRequireList)
        {
            questProgressList.Add(new Tuple<string, int, int>(
                questRequire.questRequireInfo.name,
                questRequire.currentAmount, 
                questRequire.questRequireInfo.requireAmount));
        }

        foreach (var reward in quest.rewardItemInfoList)
        {
            ItemInfo itemInfo = reward.GetItemInfo();

            rewardItemInfoList.Add(new Tuple<Sprite, int>(
                itemInfo.img, reward.quantity));
        }
    }
}

public class QuestController : MonoBehaviour
{
    //public QuestData QuestData { get; private set; }
    public QuestPanel QuestPanel { get; private set; }
    public bool IsPanelOpen => QuestPanel != null && QuestPanel.isActiveAndEnabled;

    private List<Quest> activeQuestList = new List<Quest>();
    private List<Quest> finishedQuestList = new List<Quest>();

    public void QuestControllerInit()
    {
        //QuestData = GetComponent<QuestData>();

        //QuestManager.Instance.OnStartQusetEvent += OnAddQuestHandler;

        //PrepareData();
        PrepareUI();

        InputManager.Instance.OnOpenQuestEvent += SwitchPanel;
    }

    private void SwitchPanel()
    {
        print("Switch Panel");

        if (IsPanelOpen)
        {
            QuestPanel.HideMe();
        }
        else
        {
            QuestPanel.ShowMe();
            //QuestData.UpdateQuestNameList();
            UpdateQuestList();
            // 默认查看第一个任务的详细信息
            OnQuestShowHandler(0, false);
        }
    }

    //private void PrepareData()
    //{
    //    QuestData.OnGetQuestNameEvent += UpdateQuestList;
    //}

    private void PrepareUI()
    {
        QuestPanel = UIManager.Instance.ShowPanel<QuestPanel>();
        QuestPanel.HideMe();

        QuestPanel.OnQuestBtnClickEvent += OnQuestShowHandler;

        QuestPanel.QuestPanelInit();
    }

    private void UpdateQuestList()
    {
        activeQuestList = QuestManager.Instance.GetActiveQuests();
        finishedQuestList = QuestManager.Instance.GetFinishedQuests();

        List<string> activeNameList = activeQuestList.Select(q => q.questName).
            ToList();
        List<string> finishedNameList = finishedQuestList.Select(q => q.questName).
            ToList();

        UpdateQuestListUI(activeNameList, finishedNameList);
    }

    private void UpdateQuestListUI(List<string> activeNameList, 
        List<string> finishedNameList)
    {
        QuestPanel.UpdateActiveQuestBtnList(activeNameList);
        QuestPanel.UpdateFinishedQuestBtnList(finishedNameList);
    }

    //private void OnAddQuestHandler(Quest quest)
    //{
    //    QuestData.AddQuest(quest);
    //}

    private void OnQuestShowHandler(int index, bool isFinished)
    {
        if (index < 0 ||
            isFinished && index >= finishedQuestList.Count || 
            !isFinished && index >= activeQuestList.Count)
        {
            NoQuestShow();
            return;
        }

        Quest quest = null;
        if (isFinished)
        {
            quest = finishedQuestList[index];
        }
        else 
        {
            quest = activeQuestList[index];
        }
            
        QuestPanel.UpdateQuestContent(new RequireUIPayload(quest));
    }

    private void NoQuestShow()
    {
        QuestPanel.UpdateNoQuest();
    }
}
