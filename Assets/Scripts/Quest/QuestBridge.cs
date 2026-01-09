using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBridge : MonoBehaviour
{
    public void TriggerRequireUpdate(string triggerName)
    {
        QuestManager.Instance.TriggerRequireUpdate(triggerName, true);
    }
}
