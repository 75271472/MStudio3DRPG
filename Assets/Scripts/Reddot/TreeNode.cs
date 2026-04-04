using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

public class TreeNode
{
    // 子节点
    private Dictionary<RangeString, TreeNode> children;
    // 节点值改变回调
    private Action<int> changeCallback;
    // 完成路径
    private string fullPath;

    // 节点名
    public string Name { get; private set; }
    // 父节点
    public TreeNode Parent { get; private set; }
    // 获取红点节点路径
    public string FullPath
    {
        get
        {
            if (string .IsNullOrEmpty(fullPath))
            {
                // 没有父节点或父节点为根节点
                if (Parent == null || Parent == ReddotManager.Instance.Root)
                {
                    fullPath = Name;
                }
                // 否则
                else
                {
                    // 父节点path / name，递归调用FullPath获取完整路径
                    fullPath = Parent.FullPath + ReddotManager.Instance.SplitChar + Name;
                }
            }

            return fullPath;
        }
    }
    // 节点值
    public int Value { get; private set; }
    // 获取子节点
    public Dictionary<RangeString, TreeNode>.ValueCollection Children
    {
        get => children?.Values;
    }
    // 递归获取子节点个数
    public int ChildrenCount
    {
        get
        {
            if (children == null) return 0;
            int sum = children.Count;
            // 递归获得以当前节点为根节点的红点树中的节点个数
            foreach (TreeNode node in children.Values)
            {
                sum += node.ChildrenCount;
            }

            return sum;
        }
    }

    public TreeNode(string name)
    {
        Name = name;
        Value = 0;
        changeCallback = null;
    }

    public TreeNode(string name, TreeNode parent) : this(name)
    {
        Parent = parent;
    }

    // 添加监听函数
    public void AddListener(Action<int> callback)
    {
        changeCallback += callback;
    }

    // 移除监听函数
    public void RemoveListener(Action<int> callback)
    {
        changeCallback -= callback;
    }

    // 移除所有监听函数
    public void RemoveAllListener()
    {
        changeCallback = null;
    }

    // 改变节点值（使用传入的新值，只能在叶子节点上调用）
    public void ChangeValue(int newValue)
    {
        // 如果当前不是叶子节点
        if (children != null && children.Count != 0)
            throw new Exception("不允许直接改变非叶子节点的值：" + FullPath);

        InternalChangeValue(newValue);
    }

    // 改变节点值（根据子节点值计算新值，只对非叶子节点有效）
    public void ChangeValue()
    {
        int sum = 0;
        if (children != null && children.Count != 0)
        {
            foreach (KeyValuePair<RangeString, TreeNode> child in children)
            {
                sum += child.Value.Value;
            }
        }

        InternalChangeValue(sum);
    }

    /// <summary>
    /// 获取子节点，如果不存在则添加
    /// </summary>
    public TreeNode GetOrAddChild(RangeString key)
    {
        TreeNode child = GetChild(key);
        if (child == null)
        {
            child = AddChild(key);
        }
        return child;
    }

    /// <summary>
    /// 获取子节点
    /// </summary>
    public TreeNode GetChild(RangeString key)
    {
        if (children == null)
        {
            return null;
        }

        children.TryGetValue(key, out TreeNode child);
        return child;
    }

    /// <summary>
    /// 添加子节点
    /// </summary>
    public TreeNode AddChild(RangeString key)
    {
        if (children == null)
        {
            children = new Dictionary<RangeString, TreeNode>();
        }
        else if (children.ContainsKey(key))
        {
            throw new Exception("子节点添加失败，不允许重复添加：" + FullPath);
        }

        TreeNode child = new TreeNode(key.ToString(), this);
        children.Add(key, child);
        ReddotManager.Instance.NodeNumChangeCallback?.Invoke();
        return child;
    }

    /// <summary>
    /// 移除子节点
    /// </summary>
    public bool RemoveChild(RangeString key)
    {
        if (children == null || children.Count == 0)
        {
            return false;
        }

        TreeNode child = GetChild(key);

        if (child != null)
        {
            //子节点被删除，将当前节点标记为脏节点，需要进行一次父节点刷新
            ReddotManager.Instance.MarkDirtyNode(this);

            children.Remove(key);
            // 触发节点数量改变的回调
            ReddotManager.Instance.NodeNumChangeCallback?.Invoke();

            return true;
        }

        return false;
    }

    /// <summary>
    /// 移除所有子节点
    /// </summary>
    public void RemoveAllChild()
    {
        if (children == null || children.Count == 0)
        {
            return;
        }

        children.Clear();
        ReddotManager.Instance.MarkDirtyNode(this);
        ReddotManager.Instance.NodeNumChangeCallback?.Invoke();
    }

    /// <summary>
    /// 改变节点值
    /// </summary>
    private void InternalChangeValue(int newValue)
    { 
        if (Value == newValue)
        {
            return;
        }

        Value = newValue;
        // 触发值改变回调
        changeCallback?.Invoke(newValue);
        ReddotManager.Instance.NodeValueChangeCallback?.Invoke(this, Value);

        //标记父节点为脏节点
        ReddotManager.Instance.MarkDirtyNode(Parent);
    }
}
