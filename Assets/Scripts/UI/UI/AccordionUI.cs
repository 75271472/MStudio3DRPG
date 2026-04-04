using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccordionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button dropdownBtn;
    [SerializeField] private Transform dropdownBtnTrans;
    [SerializeField] private GameObject itemContainerObj;
    [SerializeField] private Transform containerTrans;

    [SerializeField] private List<QuestBtn> questBtnList = new List<QuestBtn>();
    private bool isExpanded;

    public event Action<int> OnQuestBtnClickEvent;

    public void AccordionUIInit()
    {
        SetExpandState(false);
        ClearBtnList();

        dropdownBtn.onClick.RemoveAllListeners();
        dropdownBtn.onClick.AddListener(() => SetExpandState(!isExpanded));
    }

    public void SetExpandState(bool isExpanded)
    {
        this.isExpanded = isExpanded;

        if (itemContainerObj != null)
        {
            itemContainerObj.SetActive(isExpanded);
        }

        if (dropdownBtnTrans != null)
        {
            dropdownBtnTrans.localRotation = isExpanded ? Quaternion.Euler(0, 0, 180) :
                Quaternion.identity;
        }
        // 刷新UI布局
        ExtensionTool.UpdateUI(transform.parent);
    }

    public void ClearBtnList()
    {
        foreach (var questBtn in questBtnList)
        {
            questBtn.ResetEvent();
            PoolManager.Instance.PushObj(DataManager.QUESTBUTTON, questBtn.gameObject);
        }

        //print("ClearBtnList");
        questBtnList.Clear();
    }

    public void UpdateQuestBtnList(List<Quest> questList)
    {
        foreach (var quest in questList)
        {
            GameObject questBtnObj = PoolManager.Instance.PullObj(
                DataManager.QUESTBUTTON);
            questBtnObj.transform.SetParent(containerTrans, false);

            QuestBtn questBtn = questBtnObj.GetComponent<QuestBtn>();
            questBtn.ResetQuestBtn();
            questBtn.SetQuestBtn(quest.questName);
            questBtn.InitReddot(quest.id); // Add Reddot initialization
            questBtn.OnQuestBtnClickEvent += OnQuestBtnClickHandler;

            questBtnList.Add(questBtn);
        }
    }

    private void OnQuestBtnClickHandler(QuestBtn questBtn)
    {
        int index = questBtnList.IndexOf(questBtn);

        if (index == -1) return;

        OnQuestBtnClickEvent?.Invoke(index);
    }
}
