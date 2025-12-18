using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBtnContent : MonoBehaviour
{
    [SerializeField] private Transform contentTrans;
    private List<QuestBtn> questBtnList = new List<QuestBtn>();

    public event Action<int> OnQuestBtnClickEvent;

    public void QuestBtnContentInit()
    {
        ClearBtnList();
    }

    public void ClearBtnList()
    {
        foreach (var questBtn in questBtnList)
        {
            PoolManager.Instance.PushObj(DataManager.QUESTBUTTON, questBtn.gameObject);
        }

        questBtnList.Clear();
    }

    public void UpdateQuestBtnList(List<string> questNameList)
    {
        foreach (var questName in questNameList)
        {
            GameObject questBtnObj = PoolManager.Instance.PullObj(
                DataManager.QUESTBUTTON);
            questBtnObj.transform.SetParent(contentTrans, false);

            QuestBtn questBtn = questBtnObj.GetComponent<QuestBtn>();
            questBtn.ResetQuestBtn();
            questBtn.SetQuestBtn(questName);
            questBtn.OnQuestBtnClickEvent += OnQuestBtnClickHandler;

            questBtnList.Add(questBtn);
        }
    }

    public void OnQuestBtnClickHandler(QuestBtn questBtn)
    {
        int index = questBtnList.IndexOf(questBtn);

        if (index == -1) return;

        OnQuestBtnClickEvent?.Invoke(index);
    }
}
