using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum EItemInfo
{
    // 可使用物品类
    Editable,
    // 可装备物品类
    Equippable,
    // 正在装备物品类

}

// 物品数据类
public abstract class ItemInfo
{
    public int id;
    public EItemInfo itemType;
    public bool isStackable;
    public string name;
    public string description;
    public string imgPath;
    public int maxStackSize;
    public Sprite img;

    public virtual string GetDescription() => description;
}

