using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOptionUI : MonoBehaviour
{
    public event Action<int> OnOptionSelectEvent;

    private List<OptionBtnUI> optionBtnList = new List<OptionBtnUI>();

    public void UpdateOptionList(List<string> optionStringList)
    {
        ResteOptionList();

        if (optionStringList != null)
        {
            foreach (string optionString in optionStringList)
            {
                GameObject optionObj = PoolManager.Instance.
                    PullObj(DataManager.OPTIONBUTTONUI);
                optionObj.transform.SetParent(this.transform);

                OptionBtnUI optionBtnUI = optionObj.GetComponent<OptionBtnUI>();
                optionBtnUI.ResetOptionBtn();
                optionBtnUI.SetOptionBtn(optionString);
                optionBtnUI.OnBtnClickEvent += OnOptionBtnClickHandler;

                optionBtnList.Add(optionBtnUI);
            }
        }

        ExtensionTool.UpdateUI(this.transform);
        ExtensionTool.UpdateUI(transform.parent);
    }

    public void ResetEvent()
    {
        OnOptionSelectEvent = null;
    }

    public void ResteOptionList()
    {
        foreach (OptionBtnUI optionBtn in optionBtnList)
        {
            optionBtn.OnBtnClickEvent -= OnOptionBtnClickHandler;
            PoolManager.Instance.PushObj(DataManager.OPTIONBUTTONUI, 
                optionBtn.gameObject);
        }

        optionBtnList.Clear();
    }

    private void OnOptionBtnClickHandler(OptionBtnUI optionBtn)
    {
        int index = optionBtnList.IndexOf(optionBtn);
        if (index < 0 || index >= optionBtnList.Count) return;

        OnOptionSelectEvent?.Invoke(index);
    }
}
