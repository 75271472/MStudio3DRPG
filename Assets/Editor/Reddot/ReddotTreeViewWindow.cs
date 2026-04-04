using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class ReddotTreeViewWindow : EditorWindow
{
    private static ReddotTreeViewWindow window;
    private ReddotTreeView treeView;
    private SearchField searchField;

    [MenuItem("Window/红点树视图窗口")]
    private static void OpenWindow()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("警告", "运行后才能打开红点树视图窗口", "了解");
            return;
        }

        window = GetWindow<ReddotTreeViewWindow>();
        window.titleContent = new GUIContent("红点树视图窗口");
        window.Show();
    }

    private void OnEnable()
    {
        treeView = new ReddotTreeView(new TreeViewState());

        searchField = new SearchField();
        searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;

        EditorApplication.playModeStateChanged += OnPlayModeStateChange;
    }

    private void OnPlayModeStateChange(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredEditMode:
                break;
            case PlayModeStateChange.ExitingEditMode:
                break;
            case PlayModeStateChange.EnteredPlayMode:
                break;
            case PlayModeStateChange.ExitingPlayMode:
                window.Close();
                break;
        }
    }

    private void OnDestroy()
    {
        treeView.OnDestory();
        EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
    }

    private void OnGUI()
    {
        UpToolbar();
        TreeView();
        BottomToolBar();
    }

    private void UpToolbar()
    {
        treeView.searchString = searchField.OnGUI(new Rect(0, 0, 
            position.width - 40f, 20f), treeView.searchString);
    }

    private void TreeView()
    {
        treeView.OnGUI(new Rect(0, 20f, position.width, position.height - 40f));
    }

    private void BottomToolBar()
    {
        GUILayout.BeginArea(new Rect(20f, position.height - 18f, position.width - 40f, 16f));

        using (new EditorGUILayout.HorizontalScope())
        {
            string style = "miniButton";
            if (GUILayout.Button("展开全部节点", style))
            {
                treeView.ExpandAll();
            }

            if (GUILayout.Button("收起全部节点", style))
            {
                treeView.CollapseAll();
            }
        }

        GUILayout.EndArea();
    }
}
