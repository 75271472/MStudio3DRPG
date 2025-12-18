
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviourManager<PlayerManager>, ICharacter
{
    public PlayerStateMachine PlayerStateMachine { get; private set; }
    public PlayerData PlayerData { get; private set; }
    public PlayerUI PlayerUI { get; private set; }

    public CharacterStateMachine CharacterStateMachine => PlayerStateMachine;
    public CharacterData CharacterData => PlayerData;
    public CharacterUI CharacterUI => PlayerUI;
    public GameObject CharacterGameObject => gameObject;

    public event Action<InventoryItemInfo> OnPickUpEvent;
    public event Action<ICharacter> OnCharacterDieEvent;

    public void PlayerManagerInit()
    {
        PlayerStateMachine = GetComponent<PlayerStateMachine>();
        PlayerData = GetComponent<PlayerData>();
        PlayerUI = GetComponent<PlayerUI>();

        PlayerData.CharacterDataInit(this);
        PlayerStateMachine.CharacterStateMachineInit(this);
        PlayerUI.PlayerInit(this, PlayerData);

        // 不在这里进行切换武器的事件注册，而是在PlayerStateMachine.Init中
        // 在执行WeaponHandler.SetWeapon之前进行事件注册
        // 因为SetWeapon执行就会触发事件，而那时还没有注册事件
        //PlayerData.SwitchWeaponRegist(PlayerStateMachine.WeaponHandler);
        PlayerData.OnPickUpEvent += (inventoryItemInfo) => 
            OnPickUpEvent?.Invoke(inventoryItemInfo);
        PlayerData.HealthEventRegist(PlayerStateMachine.Health);
        PlayerStateMachine.DieDataRegist(PlayerData);
        PlayerStateMachine.SwitchEquipItemRegist(PlayerData);
        PlayerUI.CharacterDataEventRegist(PlayerData);
        PlayerUI.UpdateExpRegist(PlayerData);

        // 所有时间注册结束后执行一次PlayerData.OnEquipEvent
        // 更新WeaponHandler和AttackComboList
        PlayerData.OnEquipEventInvoke();

        // 加载场景前，将数据保存到DataManager中
        LoadSceneManager.Instance.OnPrepareLoadSceneEvent += 
            PlayerData.OnUpdateDataInMemoryHandler;
        PortalManager.Instance.OnEnterSameScenePortalEvent += 
            PlayerStateMachine.Pause;
        PortalManager.Instance.OnExitSameScenePortalEvent += 
            PlayerStateMachine.UnPause;

        //print("PlayerManagerInit");
    }

    public void PlayerTransInit()
    {
        PlayerInitTrans initTrans = FindObjectOfType<PlayerInitTrans>();
        if (initTrans == null) return;

        UpdatePlayerTrans(initTrans.Pos, initTrans.Rot);
    }

    public void PlayerTransInit(CharacterTransInfo playerTrans)
    {
        if (playerTrans == null) return;

        // 不调用Updateplayer
        //UpdatePlayerTrans(playerTrans.position, playerTrans.rotation);
        gameObject.transform.SetPositionAndRotation(playerTrans.GetPosition(), 
            playerTrans.GetRotation());
    }

    private void UpdatePlayerTrans(Vector3 position, Quaternion rotation)
    {
        PlayerStateMachine.Pause();
        gameObject.transform.SetPositionAndRotation(position, rotation);
        PlayerStateMachine.UnPause();
    }

    public override void Init()
    {
        base.Init();
        if (IsNotSubManagerInit) return;

        PlayerManagerInit();
    }
}
