using System.Collections.Generic;
using System.Resources;
using UnityEditor;
using UnityEngine;

public class AssetBundleMonitorWindow : EditorWindow
{
    private Vector2 scrollPosition;

    // 用于记录树状视图中各个 Bundle 节点的折叠状态
    private Dictionary<string, bool> bundleFoldoutStates = new 
        Dictionary<string, bool>();

    [MenuItem("Tools/资源内存监控面板 (AB Monitor)")]
    public static void ShowWindow()
    {
        var window = GetWindow<AssetBundleMonitorWindow>("AB Monitor");
        window.Show();
    }

    // 让窗口在运行模式下实时刷新
    void Update()
    {
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("请在游戏运行模式 (Play Mode) 下查看内存数据。", 
                MessageType.Info);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        // 绘制异步加载队列
        DrawAsyncQueues();
        EditorGUILayout.Space(10);
        // 绘制树形内存资源
        DrawTreeHierarchy();
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 绘制异步加载请求队列
    /// </summary>
    private void DrawAsyncQueues()
    {
        GUILayout.Label("=== 当前异步加载队列 (Async Queues) ===", EditorStyles.boldLabel);
        // 1. Bundle 异步队列
        var asyncBundles = BundleManager.Instance.GetAsyncBundles();

        foreach (var asyncBundle in asyncBundles)
        {
            GUI.color = Color.cyan;
            GUILayout.Label($"[Bundle Loading...] {asyncBundle.url}");
            GUI.color = Color.white; // 恢复默认颜色
        }

        // 2. Resource 异步队列
        var asyncRes = ResourcesManager.Instance.GetAsyncResources();

        foreach (var res in asyncRes)
        {
            GUI.color = Color.yellow;
            GUILayout.Label($"[Resource Loading...] {res.url}");
            GUI.color = Color.white;
        }
    }

    /// <summary>
    /// 绘制 Bundle -> Resource 的树状结构
    /// </summary>
    private void DrawTreeHierarchy()
    {
        GUILayout.Label("=== 内存对象树 (Bundle -> Resources) ===", 
            EditorStyles.boldLabel);

        var allBundles = BundleManager.Instance.GetAllLoadedBundles();
        var allResources = ResourcesManager.Instance.GetAllLoadedResources();

        // 将 Resource 按照它们归属的 Bundle 进行分组
        Dictionary<ABundle, List<AResource>> treeData = 
            new Dictionary<ABundle, List<AResource>>();

        // 初始化字典
        foreach (var bundle in allBundles)
        {
            treeData[bundle] = new List<AResource>();
        }

        // 收集无家可归的 Resource（比如 EditorResource 或者报错丢失 Bundle 的资源）
        List<AResource> orphanResources = new List<AResource>();

        foreach (var res in allResources)
        {
            if (res.bundle != null && treeData.ContainsKey(res.bundle))
            {
                treeData[res.bundle].Add(res);
            }
            else
            {
                orphanResources.Add(res);
            }
        }

        // 绘制树状节点
        foreach (var kvp in treeData)
        {
            ABundle bundle = kvp.Key;
            List<AResource> childResources = kvp.Value;

            // 维护折叠状态
            if (!bundleFoldoutStates.ContainsKey(bundle.url))
            {
                bundleFoldoutStates[bundle.url] = false; // 默认闭合
            }

            // 检查 Bundle 引用计数是否异常 (> 5 标红)
            bool isBundleHighRef = bundle.reference > 5;
            GUI.contentColor = isBundleHighRef ? Color.red : Color.white;

            // 绘制根节点 (Bundle)
            string bundleLabel = $"[Ref: {bundle.reference}] {bundle.url} (包含 {childResources.Count} 个活跃资源)";
            bundleFoldoutStates[bundle.url] = EditorGUILayout.Foldout(
                bundleFoldoutStates[bundle.url], bundleLabel, true);
            GUI.contentColor = Color.white; // 每帧绘制结束恢复颜色

            // 如果节点被展开，绘制子节点 (Resources)
            if (bundleFoldoutStates[bundle.url])
            {
                EditorGUI.indentLevel++; // 缩进增加，形成树状视觉

                foreach (var res in childResources)
                {
                    // 检查 Resource 引用计数是否异常
                    bool isResHighRef = res.reference > 5;
                    GUI.contentColor = isResHighRef ? Color.red : Color.white;
                    EditorGUILayout.LabelField($"└─ [Ref: {res.reference}] {res.url}");
                    GUI.contentColor = Color.white;
                }

                EditorGUI.indentLevel--; // 缩进恢复
            }
        }

        // 绘制脱离 Bundle 的独立资源 (通常在 Editor 模拟模式下出现)
        if (orphanResources.Count > 0)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("=== 独立资源 (EditorResource / No Bundle) ===", 
                EditorStyles.boldLabel);

            foreach (var res in orphanResources)
            {
                bool isResHighRef = res.reference > 5;
                GUI.contentColor = isResHighRef ? Color.red : Color.white;
                EditorGUILayout.LabelField($"[Ref: {res.reference}] {res.url}");
                GUI.contentColor = Color.white;
            }
        }
    }
}
