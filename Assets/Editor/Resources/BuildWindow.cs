using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// AssetBundle 打包控制面板
/// </summary>
public class BuildWindow : EditorWindow
{
    private enum BuildPlatform
    {
        Windows,
        Android,
        iOS
    }

    private BuildPlatform selectedPlatform;

    // 复制相关路径
    private string sourcePath = "";
    private string targetPath = "";
    private const string SourcePathKey = "ABPack_SourcePath";
    private const string TargetPathKey = "ABPack_TargetPath";

    // 上传相关路径
    private string uploadSourcePath = "";
    private string uploadUrl = "";
    private const string UploadSourcePathKey = "ABPack_UploadSourcePath";
    private const string UploadUrlKey = "ABPack_UploadUrl";

    private UnityWebRequest uploadRequest; // 用于存储当前正在进行的上传请求

    [MenuItem("Tools/ResBuild/Build Window")]
    public static void Open()
    {
        BuildWindow window = GetWindow<BuildWindow>("AB打包面板");
        window.minSize = new Vector2(400, 450);
        window.Show();
    }

    private void OnEnable()
    {
        // 加载记忆路径
        sourcePath = EditorPrefs.GetString(SourcePathKey, "");
        targetPath = EditorPrefs.GetString(TargetPathKey, "");
        
        uploadSourcePath = EditorPrefs.GetString(UploadSourcePathKey, "");
        uploadUrl = EditorPrefs.GetString(UploadUrlKey, "http://127.0.0.1/upload");

        // 根据宏进行当前平台的默认选择
#if UNITY_STANDALONE_WIN
        selectedPlatform = BuildPlatform.Windows;
#elif UNITY_ANDROID
        selectedPlatform = BuildPlatform.Android;
#elif UNITY_IPHONE || UNITY_IOS
        selectedPlatform = BuildPlatform.iOS;
#else
        // 默认回退到编辑器当前设定的活跃平台
        BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
        if (activeTarget == BuildTarget.Android)
            selectedPlatform = BuildPlatform.Android;
        else if (activeTarget == BuildTarget.iOS)
            selectedPlatform = BuildPlatform.iOS;
        else
            selectedPlatform = BuildPlatform.Windows;
#endif

        // 初始化时设置 Builder 中的 Platform
        UpdateBuilderPlatform();
    }

    private void OnDisable()
    {
        // 窗口关闭时清理上传任务监听
        EditorApplication.update -= HandleUploadUpdate;
        if (uploadRequest != null)
        {
            uploadRequest.Abort();
            uploadRequest.Dispose();
            uploadRequest = null;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        // 基础打包模块
        DrawBuildSection();

        EditorGUILayout.Space(15);

        // AB 包复制模块
        DrawCopySection();

        EditorGUILayout.Space(15);

        // AB 包上传模块
        DrawUploadSection();
    }

    /// <summary>
    /// 绘制基础打包部分
    /// </summary>
    private void DrawBuildSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("AssetBundle 核心打包", GetHeaderStyle());
            EditorGUILayout.Space(10);

            // 平台选择
            EditorGUI.BeginChangeCheck();
            selectedPlatform = (BuildPlatform)EditorGUILayout.EnumPopup("目标平台", selectedPlatform);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateBuilderPlatform();
            }

            EditorGUILayout.Space(15);

            // 打包按钮
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("开始打包 (Build AB)", GUILayout.Height(40)))
            {
                UpdateBuilderPlatform();
                Builder.Build();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制 AB 包复制工具部分
    /// </summary>
    private void DrawCopySection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("AB 包复制工具", GetHeaderStyle());
            EditorGUILayout.Space(10);

            // 第一步：源路径
            DrawPathRow("AB 包源路径", ref sourcePath, SourcePathKey);

            EditorGUILayout.Space(5);

            // 第二步：目标路径
            DrawPathRow("复制到路径", ref targetPath, TargetPathKey);

            EditorGUILayout.Space(15);

            // 复制按钮
            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f); // 淡绿色
            if (GUILayout.Button("执行复制 (Copy Assets)", GUILayout.Height(40)))
            {
                DoCopyAssets();
                AssetDatabase.Refresh();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制 AB 包上传工具部分
    /// </summary>
    private void DrawUploadSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("AB 包上传工具 (UnityWebRequest)", GetHeaderStyle());
            EditorGUILayout.Space(10);

            // 第一条：本地路径
            DrawPathRow("待上传路径", ref uploadSourcePath, UploadSourcePathKey);

            EditorGUILayout.Space(5);

            // 第二条：URL 输入
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("目标 URL", GUILayout.Width(90));
                EditorGUI.BeginChangeCheck();
                uploadUrl = EditorGUILayout.TextField(uploadUrl);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetString(UploadUrlKey, uploadUrl);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(15);

            // 上传按钮
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("执行上传 (Upload to Server)", GUILayout.Height(40)))
            {
                StartUpload();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 核心上传逻辑
    /// </summary>
    private void StartUpload()
    {
        if (string.IsNullOrEmpty(uploadSourcePath) || string.IsNullOrEmpty(uploadUrl))
        {
            EditorUtility.DisplayDialog("提示", "请先选择本地上传路径并输入合法的 URL！", "确定");
            return;
        }

        if (!Directory.Exists(uploadSourcePath))
        {
            EditorUtility.DisplayDialog("提示", $"本地路径不存在:\n{uploadSourcePath}", "确定");
            return;
        }

        // 收集所有要上传的文件
        string[] allFiles = Directory.GetFiles(uploadSourcePath, "*", SearchOption.AllDirectories);
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        // 获取根文件夹名。例如选择的路径是 E:/Project/AssetBundle，则该值为 AssetBundle
        string rootDirName = Path.GetFileName(uploadSourcePath);

        foreach (string file in allFiles)
        {
            if (file.EndsWith(".meta")) continue;

            byte[] fileData = File.ReadAllBytes(file);
            // 组合路径，包含根文件夹名。例如: AssetBundle/Windows/manifest.ab
            string subPath = file.Replace(uploadSourcePath, "").Replace("\\", "/").TrimStart('/');
            string relativePath = rootDirName + "/" + subPath;
            
            // 使用相对于上传根目录的路径作为字段名，方便服务器端保持目录结构
            formData.Add(new MultipartFormFileSection(relativePath, fileData, relativePath, "application/octet-stream"));
        }

        if (formData.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "该目录下没有可上传的文件。", "确定");
            return;
        }

        // 发送 Post 请求
        uploadRequest = UnityWebRequest.Post(uploadUrl, formData);
        uploadRequest.SendWebRequest();

        // 注册编辑器更新回调，用于轮询异步请求进度
        EditorApplication.update += HandleUploadUpdate;
        
        Debug.Log($"<color=blue>正在开始上传 {formData.Count} 个文件到: {uploadUrl}</color>");
    }

    /// <summary>
    /// 轮询上传进度
    /// </summary>
    private void HandleUploadUpdate()
    {
        if (uploadRequest == null) return;

        if (uploadRequest.isDone)
        {
            EditorApplication.update -= HandleUploadUpdate;
            EditorUtility.ClearProgressBar();

            if (uploadRequest.result == UnityWebRequest.Result.Success)
            {
                EditorUtility.DisplayDialog("上传成功", $"资源已成功同步至服务器！\n状态码: {uploadRequest.responseCode}", "好的");
                Debug.Log("<color=green>AB 包上传成功！</color>");
            }
            else
            {
                Debug.LogError($"上传失败: {uploadRequest.error}\n详情: {uploadRequest.downloadHandler.text}");
                EditorUtility.DisplayDialog("上传失败", $"错误信息: {uploadRequest.error}", "确定");
            }

            uploadRequest.Dispose();
            uploadRequest = null;
        }
        else
        {
            float progress = uploadRequest.uploadProgress;
            bool cancel = EditorUtility.DisplayCancelableProgressBar("正在上传 AB 包", $"进度: {(progress * 100):F1}% (正在发送表单数据...)", progress);
            
            if (cancel)
            {
                uploadRequest.Abort();
                EditorApplication.update -= HandleUploadUpdate;
                EditorUtility.ClearProgressBar();
                uploadRequest.Dispose();
                uploadRequest = null;
                Debug.LogWarning("用户手动取消了上传。");
            }
        }
    }

    /// <summary>
    /// 绘制一行路径选择 UI
    /// </summary>
    private void DrawPathRow(string label, ref string pathValue, string prefKey)
    {
        EditorGUILayout.BeginHorizontal();
        {
            // 左侧显示标签和路径
            EditorGUILayout.LabelField(label, GUILayout.Width(90));

            // 路径显示框（ReadOnly）
            string displayPath = string.IsNullOrEmpty(pathValue) ? "请选择目录..." : pathValue;
            EditorGUILayout.TextField(displayPath, EditorStyles.label);

            // 右侧 "..." 按钮
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string selected = EditorUtility.OpenFolderPanel("选择文件夹", pathValue, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    pathValue = selected;
                    EditorPrefs.SetString(prefKey, pathValue);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 执行复制逻辑
    /// </summary>
    private void DoCopyAssets()
    {
        if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(targetPath))
        {
            EditorUtility.DisplayDialog("错误", "请先正确选择源路径和目标路径！", "确定");
            return;
        }

        if (!Directory.Exists(sourcePath))
        {
            EditorUtility.DisplayDialog("错误", "源路径不存在，请检查后重试。", "确定");
            return;
        }

        try
        {
            // 获取源文件夹名，并在目标路径下创建同名文件夹
            string dirName = Path.GetFileName(sourcePath);
            string finalDest = Path.Combine(targetPath, dirName);

            if (!Directory.Exists(finalDest))
            {
                Directory.CreateDirectory(finalDest);
            }

            // 执行递归复制
            CopyDirectory(sourcePath, finalDest);

            EditorUtility.DisplayDialog("成功", $"AB 包已成功复制到：\n{finalDest}", "好的");
            Debug.Log($"<color=green>资源复制成功: {finalDest}</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"复制失败: {e.Message}");
            EditorUtility.DisplayDialog("复制失败", e.Message, "确定");
        }
    }

    /// <summary>
    /// 递归复制文件夹
    /// </summary>
    private void CopyDirectory(string sourceDir, string destDir)
    {
        // 复制所有文件
        string[] files = Directory.GetFiles(sourceDir);
        foreach (string file in files)
        {
            // 排除 meta 文件
            if (file.EndsWith(".meta")) continue;

            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile, true);
        }

        // 递归复制所有子文件夹
        string[] subDirs = Directory.GetDirectories(sourceDir);
        foreach (string subDir in subDirs)
        {
            string dirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destDir, dirName);
            if (!Directory.Exists(destSubDir))
            {
                Directory.CreateDirectory(destSubDir);
            }
            CopyDirectory(subDir, destSubDir);
        }
    }

    /// <summary>
    /// 同步平台名称到 Builder 类
    /// </summary>
    private void UpdateBuilderPlatform()
    {
        string platformName = selectedPlatform.ToString();
        Builder.ChangePlatformByWindow(platformName);
    }

    private GUIStyle GetHeaderStyle()
    {
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.fontSize = 14;
        style.alignment = TextAnchor.MiddleCenter;
        return style;
    }
}
