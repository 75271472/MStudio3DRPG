using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestBtn : MonoBehaviour
{
    [SerializeField] private Button questBtn;
    [SerializeField] private Text questTxt;
    [SerializeField] private ReddotUIController reddotController;

    private int questId;

    public event Action<QuestBtn> OnQuestBtnClickEvent;

    public void ResetQuestBtn()
    {
        questBtn.onClick.RemoveAllListeners();
        questBtn.onClick.AddListener(() => 
        {
            if (ReddotHandlerManager.Instance != null && ReddotHandlerManager.Instance.QuestReddotHandler != null)
            {
                ReddotHandlerManager.Instance.QuestReddotHandler.MarkQuestRead(questId);
            }
            OnQuestBtnClickEvent?.Invoke(this);
        });
    }

    public void ResetEvent()
    {
        OnQuestBtnClickEvent = null;
    }

    public void SetQuestBtn(string questName)
    {
        questTxt.text = questName;
    }

    public void InitReddot(int questId)
    {
        this.questId = questId;
        if (reddotController != null)
        {
            reddotController.SetReddotPath($"Main/BagTask/Quest/{questId}");
        }
    }
}
