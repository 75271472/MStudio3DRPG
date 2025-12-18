using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class StateTxtUI : MonoBehaviour
{
    [field: SerializeField] public string Title { get; private set; }
    [SerializeField] public Text stateTxt;

    public void UpdateTxt(int valueA, int valueB)
    {
        stateTxt.text = $"{Title}: {valueA}/{valueB}";
    }

    public void UpdateTxt(int value)
    {
        stateTxt.text = $"{Title}: {value}";
    }

    public void UpdateTxt(float value)
    {
        // 保留两位数字
        stateTxt.text = $"{Title}: {(float)Math.Round(value, 2)}";
    }
}
