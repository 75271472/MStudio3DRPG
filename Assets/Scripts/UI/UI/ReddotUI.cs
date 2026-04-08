using UnityEngine;
using UnityEngine.UI;

public class ReddotUI : MonoBehaviour
{
    [SerializeField] private Text reddotTxtValue;

    private int value;

    public int GetValue() => value;

    public void UpdateValue(int newValue)
    {
        value = newValue;

        if (reddotTxtValue != null && value > 0)
        {
            // 处理大于99的情况
            reddotTxtValue.text = value > 99 ? "99+" : value.ToString();
        }
    }
}
