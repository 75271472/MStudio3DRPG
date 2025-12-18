using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestContentUI : MonoBehaviour
{
    [SerializeField] private Text nameTxt;
    [SerializeField] private Text descriptionTxt;
    [SerializeField] private Text descriptionTitleTxt;
    [SerializeField] private Text progressTitleTxt;
    [SerializeField] private Text rewardTitleTxt;
    [SerializeField] private Transform questProgressContent;
    [SerializeField] private Transform rewardItemContent;

    private List<ProgressUI> progressUIList = new List<ProgressUI>();
    private List<RewardItemUI> rewardItemUIList = new List<RewardItemUI>();

    public void QuestContentUIInit()
    {
        ClearContent();
    }

    public void ClearContent()
    {
        nameTxt.text = string.Empty;
        descriptionTxt.text = string.Empty;
        if (progressUIList != null)
        {
            foreach (var progressUI in progressUIList)
            {
                PoolManager.Instance.PushObj(DataManager.PROGRESSUI, progressUI.gameObject);
            }

            progressUIList.Clear();
        }

        if (rewardItemUIList != null)
        {
            foreach (var rewardItem in rewardItemUIList)
            {
                PoolManager.Instance.PushObj(DataManager.REWARDITEMUI, rewardItem.gameObject);
            }

            rewardItemUIList.Clear();
        }
    }

    public void UpdateNoQuest()
    {
        nameTxt.text = "当前无任务";

        DisableTitleTxt();
    }

    public void UpdateQuestName(string questName)
    {
        nameTxt.text = questName;
    }

    public void UpdateDescription(string description)
    {
        descriptionTitleTxt.gameObject.SetActive(true);
        descriptionTxt.text = description;
    }

    public void UpdateProgress(List<Tuple<string, int, int>> progressList)
    {
        progressTitleTxt.gameObject.SetActive(true);

        foreach (var progressValue in progressList)
        {
            GameObject progressObj = PoolManager.Instance.PullObj(
                DataManager.PROGRESSUI);
            progressObj.transform.SetParent(questProgressContent, false);

            ProgressUI progressUI = progressObj.GetComponent<ProgressUI>();
            progressUI.ResetProgress();
            progressUI.SetProgress(progressValue.Item1,
                progressValue.Item2, progressValue.Item3);

            progressUIList.Add(progressUI);
        }
    }

    public void UpdateRequireContent(List<Tuple<Sprite, int>> itemInfoList)
    {
        rewardTitleTxt.gameObject.SetActive(true);

        foreach (var itemInfo in itemInfoList)
        {
            GameObject rewardItemObj = PoolManager.Instance.PullObj(
                DataManager.REWARDITEMUI);
            rewardItemObj.transform.SetParent(rewardItemContent, false);

            RewardItemUI rewardItemUI = rewardItemObj.GetComponent<RewardItemUI>();
            rewardItemUI.ResetData();
            rewardItemUI.SetData(itemInfo.Item1, itemInfo.Item2);
            
            rewardItemUIList.Add(rewardItemUI);
        }
    }

    private void DisableTitleTxt()
    {
        descriptionTitleTxt.gameObject.SetActive(false);
        progressTitleTxt.gameObject.SetActive(false);
        rewardTitleTxt.gameObject.SetActive(false);
    }
}
