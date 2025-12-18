using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPanel : BasePanel
{
    [SerializeField] private Transform content;

    private List<InfoTxtUI> infoTxtUIList = new List<InfoTxtUI>();

    public override void ShowMe()
    {
        base.ShowMe();

        gameObject.SetActive(true);
    }

    public override void HideMe()
    {
        base.HideMe();

        gameObject.SetActive(false);
    }

    public void AddInfo(string info)
    {
        GameObject infoTxtObj = PoolManager.Instance.PullObj(DataManager.INFOTEXTUI);

        InfoTxtUI infoTxtUI = infoTxtObj.GetComponent<InfoTxtUI>();
        infoTxtUI.ResetInfo();
        infoTxtUI.UpdateInfo(info, RemoveInfo);

        infoTxtUIList.Add(infoTxtUI);
    }

    public void RemoveInfo(InfoTxtUI infoTxtUI) 
    {
        PoolManager.Instance.PushObj(DataManager.INFOTEXTUI, infoTxtUI.gameObject);

        infoTxtUIList.Remove(infoTxtUI);
    }

    public void ResetInfoList()
    {
        for (int i = infoTxtUIList.Count - 1; i >= 0; i--)
        {
            RemoveInfo(infoTxtUIList[i]);
        }

        infoTxtUIList.Clear();
    }
}
