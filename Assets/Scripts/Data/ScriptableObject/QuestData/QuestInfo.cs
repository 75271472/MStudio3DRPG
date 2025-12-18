using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public enum ERequireType
{
    KillEnemy,      // 击杀怪物类型
    CollectItem,    // 收集物品类型
}

public enum EQuestState
{
    NotAccepted,
    Start,
    Complete,
    Finished
}

public class RequireDataPayload
{
    public ICharacter character;
    public InventoryItemInfo inventoryItemInfo;

    public RequireDataPayload(ICharacter character = null,
        InventoryItemInfo inventoryItemInfo = null)
    {
        this.character = character;
        this.inventoryItemInfo = inventoryItemInfo;
    }
}

#region Excel表中记录数据

[System.Serializable]
public class QuestRequireInfo
{
    public string name;
    public int requireAmount;
    public ERequireType requireType;

    public QuestRequireInfo() { }

    public QuestRequireInfo(string name, int requireAmount)
    {
        this.name = name;
        this.requireAmount = requireAmount;
    }
}

[System.Serializable]
public class QuestInfo
{
    public int id;
    public string questName;
    public string description;

    public List<QuestRequireInfo> questRequireInfos;
    public List<InventoryItemInfo> rewardItemInfos;
}
#endregion
#region 内存中记录数据
[System.Serializable]
public class QuestRequire
{
    public QuestRequireInfo questRequireInfo;
    public int currentAmount;
    public bool isComplated;

    public event Action<QuestRequire> OnRequireCompletedEvent;

    public QuestRequire() { }

    public QuestRequire(QuestRequireInfo questRequireInfo, int currentAmount = 0) 
    {
        this.questRequireInfo = questRequireInfo;
        this.currentAmount = currentAmount;
        isComplated = false;
    }

    // 深拷贝
    public QuestRequire Clone()
    {
        QuestRequire newRequire = new QuestRequire(this.questRequireInfo);
        newRequire.currentAmount = this.currentAmount;
        newRequire.isComplated = this.isComplated;
        return newRequire;
    }

    public void UpdateKillEnemyRequire(string monsterName)
    {
        if (questRequireInfo.requireType != ERequireType.KillEnemy) return;
        if (monsterName != questRequireInfo.name) return;

        currentAmount = Mathf.Min(currentAmount + 1, questRequireInfo.requireAmount);

        CheckCompleted();
    }

    public void UpdateCollectItemRequire(string itemName, int quantity)
    {
        if (questRequireInfo.requireType != ERequireType.CollectItem) return;
        if (itemName != questRequireInfo.name) return;

        Debug.Log(questRequireInfo.name);

        currentAmount = Mathf.Min(currentAmount + quantity, 
            questRequireInfo.requireAmount);

        CheckCompleted();
    }

    private void CheckCompleted()
    {
        if (currentAmount < questRequireInfo.requireAmount) return;

        isComplated = true;
        OnRequireCompletedEvent?.Invoke(this);

        OnRequireCompletedEvent = null;
    }
}

[System.Serializable]
public class Quest
{
    public int id;
    public string questName;
    public string description;
    public EQuestState questState;
    public List<QuestRequire> questRequireList;
    public List<InventoryItemInfo> rewardItemInfoList;

    public event Action<Quest> OnQuestCompletedEvent;

    public Quest(QuestInfo questInfo, EQuestState questState = EQuestState.NotAccepted)
    {
        this.id = questInfo.id;
        this.questName = questInfo.questName;
        this.description = questInfo.description;

        this.rewardItemInfoList = questInfo.rewardItemInfos;
        questRequireList = new List<QuestRequire>();
        foreach (var questRequireInfo in questInfo.questRequireInfos)
        {
            QuestRequire questRequire = new QuestRequire(questRequireInfo);
            questRequire.OnRequireCompletedEvent += OnRequireComlatedHandler;
            questRequireList.Add(questRequire);
        }
    }

    public Quest(QuestRecord questRecord)
    {
        QuestInfo questInfo = DataManager.Instance.QuestInfoList[questRecord.id];

        this.id = questInfo.id;
        this.questName = questInfo.questName;
        this.description = questInfo.description;
        this.rewardItemInfoList = questInfo.rewardItemInfos;
        this.questState = questRecord.questState;

        this.questRequireList = new List<QuestRequire>();
        foreach (var require in questRecord.questRequireList)
        {
            QuestRequire newRequire = require.Clone();
            newRequire.OnRequireCompletedEvent += OnRequireComlatedHandler;
            questRequireList.Add(require);
        }
    }

    public void UpdateRequire(RequireDataPayload payload)
    {
        Debug.Log(questName);

        foreach (var require in questRequireList)
        {
            switch (require.questRequireInfo.requireType)
            {
                case ERequireType.KillEnemy:
                    if (payload.character == null) break;
                    string monsterName = (payload.character.CharacterData as 
                        MonsterData).MonsterName;
                    require.UpdateKillEnemyRequire(monsterName);
                    break;
                case ERequireType.CollectItem:
                    if (payload.inventoryItemInfo == null) break;
                    string itemName = payload.inventoryItemInfo.GetItemInfo().name;
                    int quantity = payload.inventoryItemInfo.quantity;
                    require.UpdateCollectItemRequire(itemName, quantity);
                    break;
            }
        }
    }

    public void SetStartState() => questState = EQuestState.Start;

    public void SetFinishState() => questState = EQuestState.Finished;

    public void RealizeReward()
    {
        Debug.Log("RealizeReward");

        foreach (var rewardItem in rewardItemInfoList)
        {
            PlayerManager.Instance.PlayerData.InventoryController.AddItem(rewardItem);
            Debug.Log("AddItem");
        }
    }

    private void OnRequireComlatedHandler(QuestRequire questRequire)
    {
        int index = questRequireList.IndexOf(questRequire);
        if (index == -1) return;

        if (questRequireList.Any(questRequire => questRequire.isComplated == false))
            return;

        questState = EQuestState.Complete;
        OnQuestCompletedEvent?.Invoke(this);
        OnQuestCompletedEvent = null;
    }
}
#endregion
#region 存储到硬盘上的数据
public class QuestRecord
{
    public int id;
    public EQuestState questState;
    public List<QuestRequire> questRequireList;

    public QuestRecord() { }

    public QuestRecord(QuestInfo questInfo)
    {
        this.id = questInfo.id;
        this.questState = EQuestState.NotAccepted;
        questRequireList = new List<QuestRequire>();
        foreach (var requireInfo in questInfo.questRequireInfos)
        {
            questRequireList.Add(new QuestRequire(requireInfo));
        }
    }

    public QuestRecord(Quest quest)
    {
        this.id = quest.id;
        this.questState = quest.questState;
        questRequireList = new List<QuestRequire>();
        foreach (var require in quest.questRequireList)
        {
            questRequireList.Add(require.Clone());
        }
    }
}

#endregion