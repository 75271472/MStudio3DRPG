using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ReddotTreeView : TreeView
{
    private ReddotTreeViewItem root;
    private int id;

    public ReddotTreeView(TreeViewState state) : base(state)
    {
        Reload();

        useScrollView = true;

        ReddotManager.Instance.NodeNumChangeCallback += Reload;
        ReddotManager.Instance.NodeValueChangeCallback += Repaint;
    }

    protected override TreeViewItem BuildRoot()
    {
        id = 0;

        root = PreOrder(ReddotManager.Instance.Root);
        root.depth = -1;

        SetupDepthsFromParentsAndChildren(root);

        return root;
    }

    private ReddotTreeViewItem PreOrder(TreeNode root)
    {
        if (root == null)
        {
            return null;
        }

        ReddotTreeViewItem item = new ReddotTreeViewItem(id++, root);

        if (root.ChildrenCount > 0)
        {
            foreach (TreeNode child in root.Children)
            {
                item.AddChild(PreOrder(child));
            }
        }

        return item;
    }

    private void Repaint(TreeNode node, int value)
    {
        Repaint();
    }

    public void OnDestory()
    {
        ReddotManager.Instance.NodeNumChangeCallback -= Reload;
        ReddotManager.Instance.NodeValueChangeCallback -= Repaint;
    }
}
