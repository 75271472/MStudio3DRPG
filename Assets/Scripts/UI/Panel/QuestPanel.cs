using System;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

public class QuestPanel : BasePanel
{
    [SerializeField] private AccordionUI activeQuestAccordionUI;
    [SerializeField] private AccordionUI finishedQuestAccordionUI;
    private QuestContentUI questContentUI;

    public event Action<int, bool> OnQuestBtnClickEvent;

    public override void ShowMe()
    {
        base.ShowMe();

        activeQuestAccordionUI?.ClearBtnList();
        finishedQuestAccordionUI?.ClearBtnList();
        questContentUI?.ClearContent();
        gameObject.SetActive(true);
    }

    public override void HideMe()
    {
        base.HideMe();

        gameObject.SetActive(false);
    }

    public void QuestPanelInit()
    {
        questContentUI = GetComponentInChildren<QuestContentUI>();

        activeQuestAccordionUI.OnQuestBtnClickEvent += (index) => 
            OnQuestBtnClickHandler(index, false);
        finishedQuestAccordionUI.OnQuestBtnClickEvent += (index) => 
            OnQuestBtnClickHandler(index, true);

        activeQuestAccordionUI.AccordionUIInit();
        finishedQuestAccordionUI.AccordionUIInit();
        questContentUI.QuestContentUIInit();
    }

    public void UpdateActiveQuestBtnList(List<Quest> questList)
    {
        activeQuestAccordionUI.SetExpandState(false);
        activeQuestAccordionUI.UpdateQuestBtnList(questList);
    }

    public void UpdateFinishedQuestBtnList(List<Quest> questList)
    {
        finishedQuestAccordionUI.SetExpandState(false);
        finishedQuestAccordionUI.UpdateQuestBtnList(questList);
    }

    public void UpdateQuestContent(RequireUIPayload quest)
    {
        questContentUI.ClearContent();

        questContentUI.UpdateQuestName(quest.questName);
        questContentUI.UpdateDescription(quest.description);
        questContentUI.UpdateProgress(quest.questProgressList);
        questContentUI.UpdateRequireContent(quest.rewardItemInfoList);
    }

    public void UpdateNoQuest()
    {
        questContentUI.UpdateNoQuest();
    }

    private void OnQuestBtnClickHandler(int index, bool isFinished)
    {
        OnQuestBtnClickEvent?.Invoke(index, isFinished);
    }
}
