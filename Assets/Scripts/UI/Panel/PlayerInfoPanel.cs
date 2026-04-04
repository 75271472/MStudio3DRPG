using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanel : BasePanel
{
    [field: SerializeField] public ValueBar HealthBar { get; set; }
    [field: SerializeField] public ValueBar ExpBar { get; set; }
    [field: SerializeField] public Text ExpTxt { get; set; }
    [field: SerializeField] public Text LevelTxt { get; set; }
    [field: SerializeField] public Button BagTaskBtn { get; set; }
    [SerializeField] private ReddotUIController bagTaskReddot;

    protected override void Awake()
    {
        base.Awake();
        if (bagTaskReddot != null)
        {
            bagTaskReddot.SetReddotPath("Main/BagTask");
        }
    }

    public override void ShowMe()
    {
        base.ShowMe();
        BagTaskBtn.onClick.RemoveAllListeners();
        BagTaskBtn.onClick.AddListener(() => UIManager.Instance.ShowPanel<BagTaskPanel>().
            ToggleBag());
    }

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

    protected void ButtonOnClicked()
    {
        UIManager.Instance.ShowPanel<BagTaskPanel>().ToggleBag();
    }
}
