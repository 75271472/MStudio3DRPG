using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : CharacterUI
{
    [field: SerializeField] private PlayerInfoPanel PlayerInfoPanel { get; set; }

    public void PlayerInit(ICharacter character, PlayerData characterData)
    {
        base.CharacterUIInit(PlayerManager.Instance, characterData);

        PlayerInfoPanel = UIManager.Instance.ShowPanel<PlayerInfoPanel>();

        PlayerInfoPanel.HealthBarInit(characterData.PlayerStateInfo.health,
            characterData.PlayerStateInfo.maxHealth);
        PlayerInfoPanel.ExpBarInit(characterData.PlayerStateInfo.currentLevel,
            characterData.PlayerStateInfo.currentExp,
            characterData.PlayerStateInfo.baseExp);
    }

    public void CharacterDataEventRegist(PlayerData playerData)
    {
        playerData.OnTakeDamageEvent += UpdateHealthBar;
        playerData.OnRecoveryEvent += UpdateHealthBar;
        playerData.OnLevelUpUpdateHPEvent += UpdateHealthBar;
    }

    public void UpdateExpRegist(PlayerData playerData)
    {
        playerData.OnUpdateExpEvent += PlayerInfoPanel.OnUpdateExpHandler;
    }

    // Inspector窗口中不能世界调用UIManager中带有泛型的函数，因此在PlayerUI中封装一回
    public void ShowGameOverPanel()
    {
        UIManager.Instance.ShowPanel<GameOverPanel>(EUILayer.System);
    }

    private void UpdateHealthBar(int variableHealth, int currentHealth, int maxHealth)
    {
        PlayerInfoPanel.HealthBar.UpdateValueBar(currentHealth, maxHealth);
    }
}
