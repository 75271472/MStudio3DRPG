
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

    public event Action<InventoryItemInfo> OnUpdateItemEvent, OnRemoveItemEvent;
    public event Action<ICharacter> OnCharacterDieEvent;

    public void PlayerManagerInit()
    {
        PlayerStateMachine = GetComponent<PlayerStateMachine>();
        PlayerData = GetComponent<PlayerData>();
        PlayerUI = GetComponent<PlayerUI>();

        PlayerData.CharacterDataInit(this);
        PlayerStateMachine.CharacterStateMachineInit(this);
        PlayerUI.PlayerInit(this, PlayerData);

        PlayerData.OnUpdateItemEvent += (inventoryItemInfo) => {
            print("PlayerManager OnUpdateItemEvent Trigger");
            OnUpdateItemEvent?.Invoke(inventoryItemInfo);
        };
            
        PlayerData.OnRemoveItemEvent += (inventoryItemInfo) => {
            print("PlayerManager OnResetItemEvent Trigger");
            OnRemoveItemEvent?.Invoke(inventoryItemInfo);
        };
        PlayerData.HealthEventRegist(PlayerStateMachine.Health);
        PlayerStateMachine.DieDataRegist(PlayerData);
        PlayerStateMachine.SwitchEquipItemRegist(PlayerData);
        PlayerUI.CharacterDataEventRegist(PlayerData);
        PlayerUI.UpdateExpRegist(PlayerData);

        // 装备武器前先保障装备了默认武器
        PlayerData.OnResetEquipEventInvoke();
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

    public void PlayerDialogueTrigger()
    {
        PlayerData.PlayerDialogueTrigger.DialogueTrigger();
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

    public void ResetEvent()
    {
        OnCharacterDieEvent = null;
        OnRemoveItemEvent = null;
        OnUpdateItemEvent = null;
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
