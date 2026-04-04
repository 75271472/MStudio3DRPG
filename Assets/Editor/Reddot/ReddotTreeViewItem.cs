using UnityEditor.IMGUI.Controls;

public class ReddotTreeViewItem : TreeViewItem
{
    private TreeNode node;
    /// <summary>
    /// 节点路径
    /// </summary>
    public string Path { get; private set; }
    /// <summary>
    /// 节点值
    /// </summary>
    public int Value { get; private set; }

    public override string displayName =>
        $"{node.Name} - 节点值: {node.Value} - 子节点数: {node.ChildrenCount}";

    public ReddotTreeViewItem(int id, TreeNode node)
    {
        base.id = id;

        this.node = node;
        Path = node.FullPath;
        Value = node.Value;
    }
}
