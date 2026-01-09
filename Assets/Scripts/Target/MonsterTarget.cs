using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTarget : MonoBehaviour
{
    [field: SerializeField] public bool IsVisible { get; private set; } = true;

    [SerializeField] private List<MonsterTargeter> targeterList = 
        new List<MonsterTargeter>();

    public void SetVisible()
    {
        IsVisible = true;

        Clear();
    }
    
    public void SetUnvisible()
    {
        IsVisible = false;

        Clear();
    }

    public void AddTargeter(MonsterTargeter targeter)
    {
        targeterList.Add(targeter);
    }

    public void RemoveTargeter(MonsterTargeter targeter)
    {
        targeterList.Remove(targeter);
    }

    // 执行逻辑: 遍历targeter，在targeterList中移除targeter
    public void Clear()
    {
        // 因为在遍历过程中Targeter会调用target.RemoveTargeter修改targeterList列表
        // 因此要从后往前移除
        for (int i = targeterList.Count - 1; i >= 0; i--)
        {
            // 游戏退出并重新加载游戏时，此时如果targeterList中有targeter对象
            // 会变为null，需要使用 ? 防止空引用
            targeterList[i]?.RemoveTarget(gameObject);
        }

        targeterList.Clear();
    }
}
