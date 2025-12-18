using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Item : MonoBehaviour
{
    [field: SerializeField] public int ItemType { get; private set; }
    [field: SerializeField] public int Quantity { get; private set; }
    [field: SerializeField] public int ItemId { get; private set; }
    public ItemInfo ItemInfo { get; private set; }

    private void Start()
    {
        ItemInfo = DataManager.Instance.GetItemInfo(ItemType, ItemId);
    }

    public void ChangeQuantity(int remainQuantity)
    {
        if ( remainQuantity == 0 )
        {
            ItemDestroy();
            return;
        }

        Quantity = remainQuantity;
    }

    public void ItemDestroy()
    {
        GameObject.Destroy(gameObject);
    }
}

/// <summary>
/// 可销毁物品接口，表示该物品是可被销毁的
/// </summary>
public interface IDestroyableItem
{

}

/// <summary>
/// 可装备物品
/// </summary>
public interface IEquippableItem
{

}

/// <summary>
/// 可卸下物品
/// </summary>
public interface IDetachableItem
{

}

public class PerformActionInfo
{
    public ICharacter character;
    public int itemIndex;

    public PerformActionInfo(ICharacter character, int itemIndex)
    {
        this.character = character;
        this.itemIndex = itemIndex;
    }
}

/// <summary>
/// 物品事件接口，表示该物品可用执行的一个动作
/// </summary>
public interface IItemAction
{
    public string ActionName { get; }
    public bool PerformAction(PerformActionInfo performActionInfo);
}
