using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionBtnUI : MonoBehaviour
{
    public event Action<OptionBtnUI> OnBtnClickEvent;

    private Text contentTxt;
    private Button optionBtn;

    public void ResetOptionBtn()
    {
        if (contentTxt == null)
        {
            contentTxt = GetComponent<Text>();
        }
        if (optionBtn == null)
        {
            optionBtn = GetComponent<Button>();
        }
        
        optionBtn.onClick.RemoveAllListeners();
        optionBtn.onClick.AddListener(() => OnBtnClickEvent?.Invoke(this));

        //print("Reset Option Btn");
    }

    public void SetOptionBtn(string optionText)
    {
        contentTxt.text = optionText;
    }
}
