using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanel : BasePanel
{
    [field: SerializeField] public ValueBar HealthBar { get; set; }
    [field: SerializeField] public ValueBar ExpBar { get; set; }
    [field: SerializeField] public Text ExpTxt { get; set; }
    [field: SerializeField] public Text LevelTxt { get; set; }

    public void HealthBarInit(int currentHealth, int maxHealth)
    {
        HealthBar.ValueBarInit(currentHealth, maxHealth);
    }

    public void ExpBarInit(int currentLevel, int currentExp, int baseExp)
    {
        ExpBar.ValueBarInit(currentExp, baseExp);
        UpdateExpTxt(currentExp, baseExp);
        UpdateLevelTxt(currentLevel);
    }

    public void OnUpdateExpHandler(int currentLevel, int currentExp, int baseExp)
    {
        ExpBar.UpdateValueBar(currentExp, baseExp);
        UpdateExpTxt(currentExp, baseExp);
        UpdateLevelTxt(currentLevel);
    }

    public void UpdateExpTxt(int currentExp, int baseExp)
    {
        ExpTxt.text = $"{currentExp}/{baseExp}";
    }

    public void UpdateLevelTxt(int currentLevel)
    {
        LevelTxt.text = currentLevel.ToString("00");
    }
}
