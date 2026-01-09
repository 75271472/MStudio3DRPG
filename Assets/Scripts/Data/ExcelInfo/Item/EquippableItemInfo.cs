using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// 装备类型枚举
public enum EEquippableItem
{
    Jewelry,    // 饰品
    Sword,      // 剑
    Shield      // 盾牌
}

// 当前装备状态枚举
public enum EEquippableState
{
    Equipped,   // 已装备
    UnEquipped  // 未装备
}

public class EquippableItemInfo : ItemInfo, IDestroyableItem
{
    //public string ActionName => "Equip";
    public EEquippableItem equippableItemType;
    public EEquippableState equippableState;
    public float attackRangeFactor;
    public string weaponPrefabPath;
    public int defense;
    public int attackInfoId;
    public AttackInfo weaponAttackInfo;

    //public bool PerformAction(PerformActionInfo performActionInfo)
    //{
    //    //character.CharacterStateMachine.WeaponHandler.SetWeapon(character, this);
    //    (performActionInfo.character.CharacterData as PlayerData).EquipItem(
    //        this, performActionInfo.itemIndex);
    //    return true;
    //}

    public override string GetDescription()
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(description);
        if (weaponAttackInfo != null)
        {
            stringBuilder.AppendLine(weaponAttackInfo.ToString());
        }

        return stringBuilder.ToString();
    }
}

