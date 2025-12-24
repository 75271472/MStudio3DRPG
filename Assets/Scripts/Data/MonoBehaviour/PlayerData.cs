using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData : CharacterData
{
    private EquippableItemInfo defaultWeaponItemInfo;
    private EquippableItemInfo playerWeaponItemInfo;

    public PlayerInfo PlayerInfo { get; private set; }
    public PlayerStateInfo PlayerStateInfo => PlayerInfo.playerStateInfo;
    public PlayerMoveInfo PlayerMoveInfo => PlayerInfo.playerMoveInfo;
    public CharacterTransInfo PlayerTransInfo => PlayerInfo.playerTransInfo;
    public EquippableItemInfo PlayerShieldInfo { get; private set; }

    public InventoryController InventoryController { get; private set; }
    public QuestController QuestController { get; private set; }

    public event Action<InventoryItemInfo> OnPickUpEvent;
    public event Action<int, int, int> OnUpdateExpEvent;
    public event Action<EquippableItemInfo> OnEquipItemEvent, OnUnloadItemEvent;

    public EquippableItemInfo PlayerWeaponInfo
    {
        get
        {
            if (defaultWeaponItemInfo == null)
            {
                defaultWeaponItemInfo = PlayerInfo.defaultWeaponInfo.GetItemInfo() as
                    EquippableItemInfo;
            }

            if (playerWeaponItemInfo == null)
            {
                return defaultWeaponItemInfo;
            }
            return playerWeaponItemInfo;
        }

        set
        {
            if (defaultWeaponItemInfo == null)
            {
                defaultWeaponItemInfo = PlayerInfo.defaultWeaponInfo.GetItemInfo() as
                    EquippableItemInfo;
            }

            playerWeaponItemInfo = value;
        }
    }

    public override void CharacterDataInit(ICharacter character)
    {
        OnEquipItemEvent = null;
        OnUnloadItemEvent = null;
        OnUpdateExpEvent = null;

        PlayerInfo = DataManager.Instance.PlayerInfo;

        if (!PlayerTransInfo.IsEmpty)
        {
            PlayerManager.Instance.PlayerTransInit(PlayerTransInfo);
        }
        AttackInfo = PlayerInfo.playerAttackInfo;

        InventoryController = GetComponent<InventoryController>();
        QuestController = GetComponent<QuestController>();

        InventoryController.InventoryControllerInit(character);
        InventoryController.OnEquipEvent += OnEquipHandler;
        InventoryController.OnUnloadEvent += OnUnloadHandler;
        InventoryController.OnCheckItemEvent += OnPickUpHandler;

        QuestController.QuestControllerInit();

        //OnEquipItemEvent += (weaponInfo) => print("OnEquipEvent");
        //OnUnloadItemEvent += (weaponInfo) => print("OnUnloadEvent");
        OnEquipItemEvent += (weaponInfo) => UpdateAttackInfo();
        OnEquipItemEvent += (weaponInfo) => AttackComboList.
            UpdateAttackRangeFactor(PlayerWeaponInfo.attackRangeFactor);

        InputManager.Instance.OnSaveEvent += OnSaveHandler;

        base.CharacterDataInit(PlayerManager.Instance);
    }

    // 重置攻击武器，
    public void OnResetEquipEventInvoke()
    {
        // 卸下当前武器，让广播默认武器
        OnUnloadHandler(PlayerWeaponInfo);
    }

    public void OnEquipEventInvoke()
    {
        InventoryController.UpdateEquipEvent();
    }

    public void UpdateAttackInfo()
    {
        if (playerWeaponItemInfo == null)
            AttackInfo = PlayerWeaponInfo.weaponAttackInfo;
        else
            AttackInfo = defaultWeaponItemInfo.weaponAttackInfo +
                PlayerWeaponInfo.weaponAttackInfo;
    }

    public override CharacterStateData GetCharacterStateData()
    {
        return new CharacterStateData(PlayerStateInfo.health, PlayerStateInfo.maxHealth,
            PlayerStateInfo.currentLevel, PlayerStateInfo.maxLevel,
            PlayerStateInfo.currentExp, PlayerStateInfo.baseExp,
            PlayerStateInfo.levelBuff, AttackInfo.damage, AttackInfo.criticalRate);
    }

    public override void OnTakeDamageHandler(int damage, GameObject attacker)
    {
        base.TakeDamage(PlayerStateInfo, damage, attacker);
    }

    public override void OnRecoveryHandler(int recovery)
    {
        base.Recovery(PlayerStateInfo, recovery);
    }

    public void OnUpdateDataInMemoryHandler()
    {
        InventoryController.UpdateItem();
        QuestManager.Instance.UpdateQuests();
    }

    private void OnEquipHandler(EquippableItemInfo itemInfo)
    {
        switch (itemInfo.equippableItemType)
        {
            case EEquippableItem.Sword: PlayerWeaponInfo = itemInfo; break;
            case EEquippableItem.Shield: PlayerShieldInfo = itemInfo; break;
        }

        OnEquipItemEvent?.Invoke(itemInfo);
    }

    private void OnUnloadHandler(EquippableItemInfo itemInfo)
    {
        switch (itemInfo.equippableItemType)
        {
            case EEquippableItem.Sword:
                // 先将PlayerWeaponInfo置空，此时PlayerWeaponInfo = DefaultWeaponInfo
                PlayerWeaponInfo = null;
                // 调用OnEquipItemEvent，将DefaultWeaponInfo装备上
                // 所以实际上不会调用WeaponHandler的OnUnloadItemHandler
                OnEquipItemEvent?.Invoke(PlayerWeaponInfo);
                break;
            case EEquippableItem.Shield: 
                // 先触发回调将PlayerShieldInfo传入
                OnUnloadItemEvent?.Invoke(PlayerShieldInfo);
                // 再将PlayerShieldInfo置空
                PlayerShieldInfo = null;
                break;
        }

        //OnUnloadItemEvent?.Invoke(itemInfo);
    }

    private void OnPickUpHandler(InventoryItemInfo inventoryItemInfo)
    {
        OnPickUpEvent?.Invoke(inventoryItemInfo);
    }

    private void OnSaveHandler()
    {
        PlayerTransInfo.UpdateCharacterTransInfo(SceneManager.GetActiveScene().name, 
            transform.position, transform.rotation);

        InventoryController.SaveItem();
        QuestManager.Instance.SaveQuests();
        DataManager.Instance.SavePlayerInfo(() =>
        {
            UIManager.Instance.ShowPanel<NoticePanel>().UpdateTipTxt("保存成功");
        }, () =>
        {
            UIManager.Instance.ShowPanel<NoticePanel>().UpdateTipTxt("保存失败");
        });
    }

    public void UpdateExp(int point)
    {
        print("UpdateExp" + point);
        PlayerStateInfo.UpdateExp(point);
        OnUpdateExpEvent?.Invoke(PlayerStateInfo.currentLevel, PlayerStateInfo.currentExp,
            PlayerStateInfo.baseExp);
    }
}
