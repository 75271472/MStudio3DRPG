using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestBtn : MonoBehaviour
{
    [SerializeField] private Button questBtn;
    [SerializeField] private Text questTxt;

    public event Action<QuestBtn> OnQuestBtnClickEvent;

    public void ResetQuestBtn()
    {
        questBtn.onClick.RemoveAllListeners();
        questBtn.onClick.AddListener(() => OnQuestBtnClickEvent?.Invoke(this));
    }

    public void ResetEvent()
    {
        OnQuestBtnClickEvent = null;
    }

    public void SetQuestBtn(string questName)
    {
        questTxt.text = questName;
    }
}
