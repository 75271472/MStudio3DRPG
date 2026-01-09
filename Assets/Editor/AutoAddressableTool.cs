using Codice.Utils;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AutoAddressableTool : EditorWindow
{
    private static string resourcesRootPath = "E:\\UnityProject\\MStudio3DRPG\\Assets\\GameRes";
    private string tempPath;

    // 在 Unity 顶部菜单栏添加一个按钮
    [MenuItem("Tools/Auto Update Addressables")]
    public static void ShowWindow()
    {
        GetWindow<AutoAddressableTool>("快速添加Addressable Group");
    }

    private void OnGUI()
    {
        GUILayout.Label("路径设置", EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"资源根路径：{resourcesRootPath}", 
            EditorStyles.boldLabel);

        EditorGUILayout.Space();
        GUILayout.Label("路径工具", EditorStyles.boldLabel);

        if (GUILayout.Button("选择资源根路径"))
        {
            tempPath = EditorUtility.OpenFolderPanel("选择资源根路径", 
                Application.dataPath, "");
            if (!string.IsNullOrEmpty(tempPath)) resourcesRootPath = tempPath;
        }

        EditorGUILayout.Space();
        GUILayout.Label("转换操作", EditorStyles.boldLabel);

        if (GUILayout.Button("添加Addressable"))
        {
            UpdateAddressables();
        }
    }

    public static void UpdateAddressables()
    {
        // 1. 获取 Addressable Settings
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("没有找到 Addressable Settings！请先初始化 Addressables。");
            return;
        }

        // 2. 获取该文件夹下所有文件的 GUID (使用 AssetDatabase.FindAssets 也行，
        // 或者 Directory.GetFiles)
        // 建议使用 Directory.GetFiles 配合 SearchOption.AllDirectories，
        // 更容易控制路径逻辑
        string[] filePaths = Directory.GetFiles(resourcesRootPath, "*.*", 
            SearchOption.AllDirectories);

        foreach (var filePath in filePaths)
        {
            // 3. 过滤掉 .meta 文件
            if (filePath.EndsWith(".meta")) continue;

            // 4. TODO: 计算 addressName (GameRes文件夹下的相对路径)
            string addressName = Path.GetRelativePath(resourcesRootPath, filePath).
                Replace("\\", "/");
            // 去除路径后缀名
            addressName.Substring(0, addressName.LastIndexOf("."));
            string unityPath = "Assets/" + Path.GetRelativePath(Application.dataPath,
                filePath).Replace("\\", "/");

            // 5. 获取 GUID
            string guid = AssetDatabase.AssetPathToGUID(unityPath);

            // 6. 添加到 Addressables Group 并改名
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(
                guid, settings.DefaultGroup);
            if (entry != null)
            {
                entry.SetAddress(addressName);
            }
        }

        Debug.Log("资源自动注册完成！");
    }
}