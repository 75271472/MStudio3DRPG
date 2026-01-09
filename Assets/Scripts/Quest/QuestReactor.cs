using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class QuestReactor : MonoBehaviour, IInteractable
{
    [SerializeField] private List<GameObject> activeObjectList;
    [SerializeField] private List<GameObject> deactiveObjectList;

    private void Start()
    {
        ResetObjectActive();
        SetObjectActive();

        QuestManager.Instance.OnCompletedQuestEvent += SetObjectActive;
    }

    private void OnDestroy()
    {
        if (!QuestManager.Instance) return;
        QuestManager.Instance.OnCompletedQuestEvent -= SetObjectActive;
    }

    private void ResetObjectActive()
    {
        //foreach (var obj in deactiveObjectList)
        //    obj?.SetActive(true);
        foreach (var obj in activeObjectList)
            obj?.SetActive(false);
    }

    private void SetObjectActive(Quest quest = null)
    {
        if (!CanInteract()) return;

        foreach (var obj in deactiveObjectList)
            obj?.SetActive(false);
        foreach (var obj in activeObjectList)
            obj?.SetActive(true);
    }

    public bool CanInteract()
    {
        if (gameObject.TryGetComponent<QuestCondition>(out var condition))
            return condition.CheckCondition();
        return false;
    }
}