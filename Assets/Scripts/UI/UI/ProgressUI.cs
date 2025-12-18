using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    [SerializeField] private Text nameTxt;
    [SerializeField] private Text progressTxt;

    public void ResetProgress()
    {
        progressTxt.text = string.Empty;
    }

    public void SetProgress(string progressName, int valueA, int valueB)
    {
        nameTxt.text = progressName;
        progressTxt.text = $"{valueA}/{valueB}";
    }
}
