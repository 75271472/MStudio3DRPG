using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public QuestConditionItem condition;

    private void Start()
    {
        QuestManager.Instance.OnStartQusetEvent += CheckCondition;
        QuestManager.Instance.OnCompletedQuestEvent += CheckCondition;
        QuestManager.Instance.OnFinishedQuestEvent += CheckCondition;
    }

    public void CheckCondition(Quest quest)
    {
        if (condition.questId != quest.id || condition.questState != quest.questState)
            return;

        condition.OnInteractEvent?.Invoke();
    }
}
