using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    [SerializeField] private Text tipTxt;
    [SerializeField] private Button sureBtn;
    [SerializeField] private Button cancelBtn;

    public override void ShowMe()
    {
        tipTxt.text = string.Empty;

        sureBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.RemoveAllListeners();

        sureBtn.onClick.AddListener(UIManager.Instance.HidePanel<TipPanel>);
        cancelBtn.onClick.AddListener(UIManager.Instance.HidePanel<TipPanel>);
    }

    public void UpdateTipTxt(string tipStr, Action sureAction = null, 
        Action cancelAction = null)
    {
        tipTxt.text = tipStr;

        sureBtn.onClick.AddListener(() => sureAction?.Invoke());
        cancelBtn.onClick.AddListener(() => cancelAction?.Invoke());
    }
}
