using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private List<WeaponLogic> weaponLogicList;
    [SerializeField] private Transform weaponTrans;
    [SerializeField] private Transform shieldTrans;

    private GameObject weaponObj;
    private GameObject shieldObj;
    //private ICharacter handler;

    public void UpdateWeaponLogicList(ICharacter handler)
    {
        weaponLogicList = null;
        weaponLogicList = GetComponentsInChildren<WeaponLogic>(true).ToList();

        foreach (var weaponLogic in weaponLogicList)
        {
            weaponLogic.WeaponLogicInit(handler);
            weaponLogic.gameObject.SetActive(false);
        }
    }

    public void ClearWeaponLogicList()
    {
        weaponLogicList.Clear();
    }

    // 重置手持武器，防止切换场景时，由于WeaponHandler为DontDestroyOnLoad
    // 其WeaponItemInfo和WeaponObj不会重置
    // 导致执行SetWeapon时会执行OnUnLoadWeaponEvent，在背包中凭空增加一个手持武器
    public void ResetWeapon()
    {
        if (weaponObj != null)
        {
            DestroyImmediate(weaponObj);
        }

        weaponObj = null;
        ClearWeaponLogicList();
    }

    public void SetWeapon(ICharacter character, EquippableItemInfo weaponItemInfo)
    {
        ResetWeapon();

        if (weaponItemInfo == null) return; 

        weaponObj = Instantiate(Resources.Load<GameObject>(
            weaponItemInfo.weaponPrefabPath));
        weaponObj.transform.SetParent(weaponTrans, false);

        UpdateWeaponLogicList(character);
    }

    public void ResetShield()
    {
        if (shieldObj != null)
        {
            DestroyImmediate(shieldObj);
        }

        shieldObj = null;
    }

    public void SetShield(EquippableItemInfo shieldItemInfo)
    {
        ResetShield();

        if (shieldItemInfo == null) return;

        shieldObj = Instantiate(Resources.Load<GameObject>(
            shieldItemInfo.weaponPrefabPath));
        shieldObj.transform.SetParent(shieldTrans, false);
    }

    public void OnEquipWeaponHandler(ICharacter character,
        EquippableItemInfo equippableItemInfo)
    {
        if (equippableItemInfo == null) return;

        switch (equippableItemInfo.equippableItemType)
        {
            case EEquippableItem.Sword: 
                SetWeapon(character, equippableItemInfo);
                break;
            case EEquippableItem.Shield: 
                SetShield(equippableItemInfo);
                break;
        }
    }

    public void OnUnloadItemHandler(EquippableItemInfo equippableItemInfo)
    {
        if (equippableItemInfo == null) return;

        switch (equippableItemInfo.equippableItemType)
        {
            case EEquippableItem.Sword: ResetWeapon(); break;
            case EEquippableItem.Shield: ResetShield(); break;
        }
    }

    public void SetDamage(AttackSO attackSO, GameObject targetObj)
    {
        foreach (var logic in FindWeaponLogicByAttackName(attackSO.actionName))
        {
            logic?.SetAttack(attackSO, targetObj);
        }
    }

    public void EnableWeaponLogic(string attackName)
    {
        foreach (var logic in FindWeaponLogicByAttackName(attackName))
        {
            logic?.SetEnable();
        }
    }

    public void DisableWeaponLogic(string attackName)
    {
        foreach (var logic in FindWeaponLogicByAttackName(attackName))
        {
            logic?.SetDisable();
        }
    }

    public void DisableAllWeaponLogic()
    {
        foreach (var weaponLogic in weaponLogicList)
        {
            weaponLogic.gameObject.SetActive(false);
        }
    }

    private List<WeaponLogic> FindWeaponLogicByAttackName(string attackName)
    {
        return weaponLogicList.FindAll(logic => 
        logic.AttackName == attackName || logic.AttackName == "AttackAll");
    }
}
