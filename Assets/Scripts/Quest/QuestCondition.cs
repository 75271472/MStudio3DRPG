using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 条件限制枚举
public enum ECondition
{
    During,     // 处于某任务的某状态允许执行
    Before,     // 在某任务的某状态之后允许执行
    After,      // 在某任务的某状态之前允许执行
}

public interface IInteractable
{
    public bool CanInteract();
}

// 备选状态
[System.Serializable]
public class BackQuestCondition
{
    public UnityEvent OnBackInteractEvent;
    public int questId;
    public EQuestState questState;
    public ECondition conditionType;
}

public class QuestCondition : MonoBehaviour
{
    public int questId;
    public EQuestState questState;
    public ECondition conditionType;

    public List<BackQuestCondition> BackQuestConditionList;
    
    public bool CheckCondition()
    {
        bool isMeet = false;

        switch (conditionType)
        {
            case ECondition.Before:
                isMeet = QuestManager.Instance.GetQuestState(questId) < questState;
                break;
            case ECondition.After:
                isMeet = QuestManager.Instance.GetQuestState(questId) >= questState;
                break;
            case ECondition.During:
                isMeet = questState == QuestManager.Instance.GetQuestState(questId);
                break;
        }

        if (!isMeet) CheckBackCondition();
        return isMeet;
    }

    private void CheckBackCondition()
    {
        bool isMeet = false;

        foreach (var condition in BackQuestConditionList)
        {
            switch (condition.conditionType)
            {
                case ECondition.Before:
                     isMeet = QuestManager.Instance.GetQuestState(condition.questId) < 
                        condition.questState;
                    break;
                case ECondition.After:
                    isMeet = QuestManager.Instance.GetQuestState(condition.questId) >= 
                        condition.questState;
                    break;
                case ECondition.During:
                    isMeet = (condition.questState == QuestManager.Instance.
                        GetQuestState(condition.questId));
                    break;
            }

            if (isMeet)
            {
                condition.OnBackInteractEvent?.Invoke();
                break;
            }
        }
    }
}
