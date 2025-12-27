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
    Trigger,        // 触发类型
}

public enum EQuestState
{
    NotAccepted,
    Start,
    Complete,
    Finished
}

public class TriggerRequire
{
    public const string triggerStr1 = "调查密林深处的异象";
    public const string triggerStr2 = "探明哥布林的秘密计划";

    public string triggerString;
    public bool isFinish;

    public TriggerRequire() { }

    public TriggerRequire(string triggerString, bool isFinish)
    {
        this.triggerString = triggerString;
        this.isFinish = isFinish;
    }
}

public class RequireDataPayload
{
    public ICharacter character;
    public InventoryItemInfo inventoryItemInfo;
    public TriggerRequire triggerRequire;

    public RequireDataPayload(ICharacter character = null,
        InventoryItemInfo inventoryItemInfo = null, 
        TriggerRequire triggerRequire = null)
    {
        this.character = character;
        this.inventoryItemInfo = inventoryItemInfo;
        this.triggerRequire = triggerRequire;
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

    public event Action<QuestRequire> OnUpdateRequireEvent;

    public QuestRequire() { }

    public QuestRequire(QuestRequireInfo questRequireInfo, int currentAmount = 0) 
    {
        this.questRequireInfo = questRequireInfo;
        this.currentAmount = currentAmount;
        isComplated = currentAmount >= questRequireInfo.requireAmount;
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

        Debug.Log("UpdateKillEnemyRequire: " + questRequireInfo.name);

        currentAmount++;

        isComplated = currentAmount >= questRequireInfo.requireAmount;
        OnUpdateRequireEvent?.Invoke(this);
    }

    public void UpdateCollectItemRequire(string itemName, int quantity)
    {
        if (questRequireInfo.requireType != ERequireType.CollectItem) return;
        if (itemName != questRequireInfo.name) return;

        Debug.Log("UpdateItemRequire: " + questRequireInfo.name);

        // 根据传入的数据重置进度
        currentAmount = Mathf.Max(currentAmount + quantity, 0);

        isComplated = currentAmount >= questRequireInfo.requireAmount;
        // 当进度更新时触发
        OnUpdateRequireEvent?.Invoke(this);
    }

    public void UpdateTriggerRequire(string triggerName, bool isTrigger = true)
    {
        if (questRequireInfo.requireType != ERequireType.Trigger) return;
        if (triggerName != questRequireInfo.name) return;

        Debug.Log("UpdateTriggerRequire: " + triggerName);

        currentAmount = isTrigger ? 1 : 0;
        isComplated = isTrigger;
        OnUpdateRequireEvent?.Invoke(this);
    }

    public void ResetEvent()
    {
        OnUpdateRequireEvent = null;
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
            questRequire.OnUpdateRequireEvent += OnUpdateRequireHandler;
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
            newRequire.OnUpdateRequireEvent += OnUpdateRequireHandler;
            this.questRequireList.Add(newRequire);
        }
    }

    public void UpdateRequire(RequireDataPayload payload)
    {
        Debug.Log(questName);

        foreach (var require in questRequireList)
        {
            //if (require.isComplated) continue;
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
                case ERequireType.Trigger:
                    if (payload.triggerRequire == null) break;
                    string triggerName = payload.triggerRequire.triggerString;
                    require.UpdateTriggerRequire(triggerName, 
                        payload.triggerRequire.isFinish);
                    break;
            }
        }
    }

    public void SetStartState() => questState = EQuestState.Start;

    public void SetFinishState()
    {
        questState = EQuestState.Finished;

        questRequireList.ForEach((quesetReqiure) => quesetReqiure.ResetEvent());
        OnQuestCompletedEvent = null;
    }

    public void RealizeReward()
    {
        Debug.Log("RealizeReward");

        foreach (var rewardItem in rewardItemInfoList)
        {
            PlayerManager.Instance.PlayerData.InventoryController.AddItem(rewardItem);
            Debug.Log("AddItem");
        }
    }

    public void SubstituteItems()
    {
        Debug.Log("SubstituteItems");

        foreach (var require in questRequireList)
        {
            if (require.questRequireInfo.requireType != ERequireType.CollectItem)
                continue;

            PlayerManager.Instance.PlayerData.InventoryController.
                DropItem(require.questRequireInfo.name,
                require.questRequireInfo.requireAmount);
        }
    }

    private void OnUpdateRequireHandler(QuestRequire questRequire)
    {
        int index = questRequireList.IndexOf(questRequire);
        if (index == -1) return;

        if (questRequireList.Any(questRequire => questRequire.isComplated == false))
        {
            questState = EQuestState.Start;
            return;
        }
        
        questState = EQuestState.Complete;
        OnQuestCompletedEvent?.Invoke(this);
        //OnQuestCompletedEvent = null;
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