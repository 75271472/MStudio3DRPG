using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviourManager<QuestManager>
{
    private List<QuestInfo> questInfoList;

    // 使用 Dictionary 持有 处于Start和Completed状态的Quest对象
    private Dictionary<int, Quest> activeQuestDict = new Dictionary<int, Quest>();
    // 当前Finished的任务列表
    private Dictionary<int, Quest> finishedQuestDict = new Dictionary<int, Quest>();

    public event Action<Quest> OnStartQusetEvent;
    public event Action<Quest> OnCompletedQuestEvent;
    public event Action<RequireDataPayload> OnRequireUpdateEvent;

    public void QuestManagerInit()
    {
        OnStartQusetEvent = null;
        OnCompletedQuestEvent = null;
        OnRequireUpdateEvent = null;

        activeQuestDict.Clear();
        finishedQuestDict.Clear();

        List<QuestRecord> questRecordList = DataManager.Instance.QuestRecordList;
        foreach (QuestRecord record in questRecordList)
        {
            LoadQuest(record);
        }

        questInfoList = DataManager.Instance.QuestInfoList;

        MonsterManager.Instance.ResetEvent();   
        MonsterManager.Instance.OnMonsterDieEvent += OnKillEnemyHandler;
        PlayerManager.Instance.ResetEvent();
        PlayerManager.Instance.OnUpdateItemEvent += OnCollectItemHandler;
        PlayerManager.Instance.OnRemoveItemEvent += OnRemoveItemHandler;
    }
    
    public override void Init()
    {
        base.Init();
        if (IsNotSubManagerInit) return;

        QuestManagerInit();
    }

    // 提供给 UI 获取当前所有未完成任务的方法
    public List<Quest> GetActiveQuests()
    {
        return activeQuestDict.Values.ToList();
    }

    public List<Quest> GetFinishedQuests()
    {
        return finishedQuestDict.Values.ToList();
    }

    public void UpdateQuests()
    {
        DataManager.Instance.UpdateQuestRecordList(activeQuestDict, finishedQuestDict);
    }

    public void SaveQuests()
    {
        DataManager.Instance.SaveQuestRecordList(activeQuestDict, finishedQuestDict);
    }

    public void SelectQuest(int questId)
    {
        if (questId < 0 || questId >= questInfoList.Count) return;

        CheckAndProcessQuest(questId);
    }

    public EQuestState GetQuestState(int questId)
    {
        if (finishedQuestDict.ContainsKey(questId))
            return EQuestState.Finished;

        if (activeQuestDict.ContainsKey(questId))
            return activeQuestDict[questId].questState;

        // 既没做完，也没开始，那就是还没接
        // 确保你的 EQuestState 枚举里有 NotAccepted 或者 None
        return EQuestState.NotAccepted;
    }

    private void CheckAndProcessQuest(int questId)
    {
        // 传入的Id位于finishedQuestIdList中，则直接返回
        if (finishedQuestDict.ContainsKey(questId)) return;
        
        // 传入处于未完成状态的任务Id
        if (activeQuestDict.ContainsKey(questId))
        {
            Quest activeQuest = activeQuestDict[questId];
            // 当前任务已经完成
            if (activeQuest.questState == EQuestState.Complete)
            {
                FinishQuest(activeQuest);
            }
            // 当前任务还未完成
            else
            {

            }
        }

        // 传入未开始状态的任务Id
        else
        {
            StartQuest(questId);
        }
    }

    private void OnKillEnemyHandler(ICharacter character)
    {
        UpdateRequire(character);
    }

    private void OnCollectItemHandler(InventoryItemInfo inventoryItemInfo)
    {
        print("QuestManager OnUpdateItemHandler Trigger");
        UpdateRequire(null, inventoryItemInfo);
    }

    private void OnRemoveItemHandler(InventoryItemInfo inventoryItemInfo)
    {
        print("QuestManager OnResetItemHandler Trigger");
        inventoryItemInfo.quantity = -inventoryItemInfo.quantity;
        UpdateRequire(null, inventoryItemInfo);
    }

    private void OnQuestCompletedHandler(Quest quest)
    {
        print(quest.questName + " Completed");

        UIManager.Instance.ShowPanel<NoticePanel>().
            UpdateTipTxt($"任务完成：{quest.questName}，可以去领取奖励了");
        OnCompletedQuestEvent?.Invoke(quest);
    }

    // 从硬盘中加载任务数据，并更新内存中的任务数据
    private void LoadQuest(QuestRecord record)
    {
        if (record.questState == EQuestState.NotAccepted) return;

        Quest quest = new Quest(record);
        print("LoadQeust：" + quest.questState);

        if (record.questState == EQuestState.Finished)
        {
            finishedQuestDict.Add(record.id, quest);
            //FinishQuest(quest);
        }
        else if (record.questState == EQuestState.Complete)
        {
            activeQuestDict.Add(record.id, quest);
        }
        else if (record.questState == EQuestState.Start)
        {
            quest.OnQuestCompletedEvent += OnQuestCompletedHandler;
            OnRequireUpdateEvent += quest.UpdateRequire;
            activeQuestDict.Add(record.id, quest);
        }
    }

    private void StartQuest(int questId)
    {
        if (activeQuestDict.ContainsKey(questId)) return;

        QuestInfo questInfo = questInfoList[questId];
        Quest quest = new Quest(questInfo, EQuestState.Start);

        // 注册任务内部事件
        quest.OnQuestCompletedEvent += OnQuestCompletedHandler;
        OnRequireUpdateEvent += quest.UpdateRequire;

        quest.SetStartState();

        activeQuestDict.Add(questId, quest);

        UIManager.Instance.ShowPanel<NoticePanel>().
            UpdateTipTxt($"任务开始：{quest.questName}");
        OnStartQusetEvent?.Invoke(quest);
    }

    private void FinishQuest(Quest quest)
    {
        OnRequireUpdateEvent -= quest.UpdateRequire;

        activeQuestDict.Remove(quest.id);
        finishedQuestDict.Add(quest.id, quest);

        quest.SetFinishState();
        quest.RealizeReward();

        UIManager.Instance.ShowPanel<NoticePanel>().
            UpdateTipTxt($"任务结束：{quest.questName}");
    }

    private void UpdateRequire(ICharacter character = null, 
        InventoryItemInfo inventoryItemInfo = null)
    {
        print("QuestManager UpdateRequire Trigger");

        RequireDataPayload requireDataPayload = new RequireDataPayload(
            character, inventoryItemInfo);

        OnRequireUpdateEvent?.Invoke(requireDataPayload);
    }
}
