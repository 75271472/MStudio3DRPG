using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputActionUI : MonoBehaviour
{
    [field: SerializeField] private Text actionTxt;
    [field: SerializeField] private Button actionBtn;

    public void SetAction(string actionName, UnityAction action)
    {
        actionTxt.text = actionName;
        actionBtn.onClick.AddListener(action);
    }

    public void ResetAction()
    {
        actionTxt.text = string.Empty;
        actionBtn.onClick.RemoveAllListeners();
    }
}
