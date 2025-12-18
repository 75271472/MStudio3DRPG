using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ArchiveUI : MonoBehaviour
{
    [SerializeField] private Transform sceneTrans;
    [SerializeField] private Transform levelTrans;
    [SerializeField] private Transform hpTrans;
    [SerializeField] private Transform expTrans;

    [SerializeField] private Text sceneTxt;
    [SerializeField] private Text levelTxt;
    [SerializeField] private Text hpTxt;
    [SerializeField] private Text expTxt;

    [SerializeField] private Button choseBtn;
    [SerializeField] private Button deleteBtn;

    public UnityAction<int> OnChoseBtnClickEvent;
    public UnityAction<int> OnDeleteBtnClickEvent;

    public int ArchiveIndex { get; private set; }

    // 向OnChoseBtnClickEvent中添加事件要在ArchiveUIInit之前添加
    // 否则会被重置掉
    public void ArchiveUIInit(int archiveIndex)
    {
        ArchiveIndex = archiveIndex;

        choseBtn.onClick.RemoveAllListeners();
        deleteBtn.onClick.RemoveAllListeners();

        choseBtn.onClick.AddListener(() => OnChoseBtnClickEvent(archiveIndex));
        deleteBtn.onClick.AddListener(() => OnDeleteBtnClickEvent(archiveIndex));

        ResetArchive();
    }

    public void RemoveAllUnityAction()
    {
        OnChoseBtnClickEvent = null;
        OnDeleteBtnClickEvent = null;
    }

    public void ResetArchive()
    {
        SwitchArchiveTxt(false);
        deleteBtn.gameObject.SetActive(false);
    }

    public void UpdateArchive(CharacterTransInfo playerTransInfo,
        PlayerStateInfo playerStateInfo)
    {
        if (playerStateInfo == null || playerTransInfo == null)
        {
            ResetArchive();
            return;
        }

        SwitchArchiveTxt(true);

        deleteBtn.gameObject.SetActive(true);

        sceneTxt.text = $"{playerTransInfo.SceneName}";
        levelTxt.text = $"{playerStateInfo.currentLevel}";
        hpTxt.text = $"{playerStateInfo.health}";
        expTxt.text = $"{playerStateInfo.currentExp}";
    }

    private void SwitchArchiveTxt(bool isActive)
    {
        sceneTrans.gameObject.SetActive(isActive);
        levelTrans.gameObject.SetActive(isActive);
        hpTrans.gameObject.SetActive(isActive);
        expTrans.gameObject.SetActive(isActive);
    }
}
