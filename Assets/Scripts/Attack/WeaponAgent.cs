using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAgent : MonoBehaviour
{
    private EquippableItemInfo weaponItemInfo;

    public void SetWeapon(EquippableItemInfo weapoinItemInfo)
    {
        this.weaponItemInfo = weapoinItemInfo;
    }
}
