using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ExcelConvertJson : EditorWindow
{
    // python转换脚本路径
    private static string pythonScript = "";
    // python解释器路径
    private const string interpreter = "python";

    // 用户自定义路径
    private static string jsonFilePath = "";
    private static string excelFilePath = "";

    private string tempPath;

    [MenuItem("Tools/ExcelConvertJson")]
    public static void ShowWindow()
    {
        GetWindow<ExcelConvertJson>("Excel/JSON转换工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("路径设置", EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"Python脚本路径：{pythonScript}", EditorStyles.boldLabel);
        EditorGUILayout.TextField($"JSON文件路径：{jsonFilePath}", EditorStyles.boldLabel);
        EditorGUILayout.TextField($"Excel文件路径：{excelFilePath}", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        GUILayout.Label("路径工具", EditorStyles.boldLabel);

        if (GUILayout.Button("选择JSON文件夹"))
        {
            tempPath = EditorUtility.OpenFolderPanel("选择JSON文件夹", Application.dataPath, "");
            if (!string.IsNullOrEmpty(tempPath)) jsonFilePath = tempPath;
        }
        if (GUILayout.Button("选择Excel文件夹"))
        {
            tempPath = EditorUtility.OpenFolderPanel("选择Excel文件夹", Application.dataPath, "");
            if (!string.IsNullOrEmpty(tempPath)) excelFilePath = tempPath;
        }
        if (GUILayout.Button("选择Python脚本"))
        {
            tempPath = EditorUtility.OpenFilePanel("选择Python脚本", Application.dataPath, "py");
            if (!string.IsNullOrEmpty(tempPath)) pythonScript = tempPath;
        }

        EditorGUILayout.Space();
        GUILayout.Label("转换操作", EditorStyles.boldLabel);

        if (GUILayout.Button("Excel To Json"))
        {
            Execute("2");  // 2 表示 Excel 转 JSON
        }
        //if (GUILayout.Button("Json To Excel"))
        //{
        //    Execute("1");  // 1 表示 JSON 转 Excel
        //}
    }

    private void Execute(string argument)
    {
        // 验证路径是否设置
        if (string.IsNullOrEmpty(pythonScript))
        {
            EditorUtility.DisplayDialog("错误", "请先设置Python脚本路径", "确定");
            return;
        }

        if (string.IsNullOrEmpty(jsonFilePath))
        {
            EditorUtility.DisplayDialog("错误", "请先设置JSON文件路径", "确定");
            return;
        }

        if (string.IsNullOrEmpty(excelFilePath))
        {
            EditorUtility.DisplayDialog("错误", "请先设置Excel文件路径", "确定");
            return;
        }

        // 配置 Python 执行环境
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = interpreter,
            Arguments = $"\"{pythonScript}\" {argument} \"{jsonFilePath}\" \"{excelFilePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // 启动进程
        using (Process process = new Process())
        {
            process.StartInfo = startInfo;
            process.Start();

            // 读取 Python 输出
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError("Python Error: " + error);
                EditorUtility.DisplayDialog("转换错误", $"转换过程中出现错误：\n{error}", "确定");
            }
            else
            {
                UnityEngine.Debug.Log("Python Output: " + output);
                EditorUtility.DisplayDialog("转换完成", $"转换成功！\n{output}", "确定");

                // 刷新Asset数据库，让Unity检测到新文件
                AssetDatabase.Refresh();
            }
        }
    }

    private void OnEnable()
    {
        // 初始化默认路径
        if (string.IsNullOrEmpty(pythonScript))
        {
            pythonScript = Application.dataPath + "/Editor/main.py";
        }
        if (string.IsNullOrEmpty(jsonFilePath))
        {
            jsonFilePath = Application.dataPath + "/StreamingAssets";
        }
        if (string.IsNullOrEmpty(excelFilePath))
        {
            excelFilePath = Application.dataPath + "/Excel";
        }
    }
}