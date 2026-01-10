using UnityEngine;
using UnityEngine.UI;

public class PausePanel : BasePanel
{
    [SerializeField] private Button backToStartSceneBtn;
    [SerializeField] private Button saveBtn;
    [SerializeField] private Button continueBtn;

    public override void ShowMe()
    {
        continueBtn.onClick.RemoveAllListeners();
        backToStartSceneBtn.onClick.RemoveAllListeners();

        PauseGame();

        continueBtn.onClick.AddListener(() => {
            UnPauseGame();
            UIManager.Instance.HidePanel<PausePanel>();
        });

        saveBtn.onClick.AddListener(() =>
        {
            PlayerManager.Instance.PlayerData.SaveGame();
        });

        backToStartSceneBtn.onClick.AddListener(() =>
        {
            // 取消时间静止，loadScenePanel的FadeInCoroutine中回累加Time.detatime
            UnPauseGame();
            // 先删除所有面板在进行场景加载
            // 因为场景加载时也会创建面板，先进行场景加载在删除面板会吧场景加载面板也删除
            LoadSceneManager.Instance.LoadSceneAsync(DataManager.STARTSCENE);
        });
    }

    private void PauseGame()
    {
        PlayerManager.Instance.PlayerStateMachine.Agent.isStopped = true;
        Time.timeScale = 0f;
        for (int i = 0; MonsterManager.Instance.GetMonster(i) != null; i++)
        {
            MonsterController monster = MonsterManager.Instance.GetMonster(i);
            monster.MonsterStateMachine.Agent.isStopped = true;
        }
    }

    private void UnPauseGame()
    {
        Time.timeScale = 1f;
        PlayerManager.Instance.PlayerStateMachine.Agent.isStopped = false;
        for (int i = 0; MonsterManager.Instance.GetMonster(i) != null; i++)
        {
            MonsterController monster = MonsterManager.Instance.GetMonster(i);
            monster.MonsterStateMachine.Agent.isStopped = false;
        }
    }
}
