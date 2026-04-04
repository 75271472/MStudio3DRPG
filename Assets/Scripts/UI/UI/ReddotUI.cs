using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class ReddotUI : MonoBehaviour
{
    [SerializeField] private Text reddotTxtValue;

    private int value;

    public int GetValue() => value;

    public void UpdateValue(int newValue)
    {
        value = newValue;

        // 值为0时隐藏整个红点，大于0时显示
        gameObject.SetActive(value > 0);

        if (reddotTxtValue != null && value > 0)
        {
            // 处理大于99的情况
            reddotTxtValue.text = value > 99 ? "99+" : value.ToString();
        }
    }
}
