using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CSharpFileInfo
{
    public string ConfigFilePath { get; private set; }
    public bool IsChoice { get; private set; } = false;
    public string ConfigFileName => Path.GetFileName(ConfigFilePath);
    public CSharpFileInfo(string configFilePath) => this.ConfigFilePath = configFilePath;

    public void SetChoice(bool isChoice) => this.IsChoice = isChoice;
}

public class FolderInfo
{
    public string FolderPath { get; private set; }
    public string FolderName => Path.GetFileName(FolderPath);
    public bool IsExpanded { get; set; } = false;
    public bool IsChoice { get; set; } = false;
    public List<FolderInfo> SubFolders { get; private set; } = new List<FolderInfo>();
    public List<CSharpFileInfo> CsFiles { get; private set; } = new List<CSharpFileInfo>();

    public FolderInfo(string folderPath) => this.FolderPath = folderPath;
}

public class GetCSharpCode : EditorWindow
{
    private static string CSharpCodePath = "E:\\UnityProject\\fusion-asteroids-host-simple-2.0.5\\Assets";
    private static string OutputTxtPath = "C:\\Users\\w3496\\Desktop\\CSharpCode.txt";

    private List<FolderInfo> rootFolders = new List<FolderInfo>();
    private Vector2 scrollPosition;
    private bool globalCSharpFilesIsChoice = false;

    [MenuItem("CustomTools/GetCSharpCode")]
    public static void ShowWindow()
    {
        GetWindow<GetCSharpCode>("GetCSharpWindow");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));

        GUILayout.Label("根路径：", EditorStyles.boldLabel);
        CSharpCodePath = EditorGUILayout.TextField(CSharpCodePath);

        GUILayout.Label("输出txt文件（带文件名）：", EditorStyles.boldLabel);
        OutputTxtPath = EditorGUILayout.TextField(OutputTxtPath);

        if (GUILayout.Button("查找CSharp文件"))
        {
            UpdateCSharpFileInfo(CSharpCodePath);
        }

        if (GUILayout.Button("输出到txt文件"))
        {
            OutputSelectedFilesToTxt();
        }

        bool previousGlobalChoice = globalCSharpFilesIsChoice;
        globalCSharpFilesIsChoice = EditorGUILayout.Toggle("全选所有文件：", globalCSharpFilesIsChoice);

        if (previousGlobalChoice != globalCSharpFilesIsChoice)
        {
            SetAllFilesChoiceState(globalCSharpFilesIsChoice);
        }

        if (rootFolders.Count == 0)
        {
            GUILayout.Label("未找到CSharp代码文件", EditorStyles.boldLabel);
        }
        else
        {
            foreach (FolderInfo folder in rootFolders)
            {
                DrawFolder(folder);
            }

            if (!globalCSharpFilesIsChoice && AreAllFilesSelected())
            {
                globalCSharpFilesIsChoice = true;
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawFolder(FolderInfo folder)
    {
        EditorGUILayout.BeginHorizontal();

        // 展开/收起箭头
        string arrow = folder.IsExpanded ? "▼" : "▶";
        if (GUILayout.Button(arrow, GUILayout.Width(20)))
        {
            folder.IsExpanded = !folder.IsExpanded;
        }

        // 文件夹选择 Toggle
        bool previousFolderChoice = folder.IsChoice;
        folder.IsChoice = EditorGUILayout.Toggle(folder.IsChoice, GUILayout.Width(20));

        if (previousFolderChoice != folder.IsChoice)
        {
            SetFolderAndFilesChoiceState(folder, folder.IsChoice);
        }

        // 文件夹图标 + 名称（会随 indentLevel 缩进）
        GUILayout.Label(EditorGUIUtility.IconContent("Folder Icon"), GUILayout.Width(16), GUILayout.Height(16));
        EditorGUILayout.LabelField(folder.FolderName);

        EditorGUILayout.EndHorizontal();

        // 展开显示子文件夹和文件
        if (folder.IsExpanded)
        {
            EditorGUI.indentLevel++;
            foreach (FolderInfo subFolder in folder.SubFolders)
            {
                DrawFolder(subFolder);
            }
            foreach (CSharpFileInfo csFile in folder.CsFiles)
            {
                DrawCSharpFile(csFile);
            }
            EditorGUI.indentLevel--;
        }
    }

    private void DrawCSharpFile(CSharpFileInfo csFile)
    {
        EditorGUILayout.BeginHorizontal();

        // 缩进（和文件夹保持一致）
        GUILayout.Space(EditorGUI.indentLevel * 20);

        // Toggle 在前，文件名在后
        bool newChoice = EditorGUILayout.Toggle(csFile.IsChoice, GUILayout.Width(20));
        if (newChoice != csFile.IsChoice)
            csFile.SetChoice(newChoice);

        EditorGUILayout.LabelField(csFile.ConfigFileName);

        EditorGUILayout.EndHorizontal();
    }

    private void UpdateCSharpFileInfo(string rootPath)
    {
        if (!Directory.Exists(rootPath)) return;

        rootFolders.Clear();

        string[] directories = Directory.GetDirectories(rootPath);
        foreach (string directory in directories)
        {
            FolderInfo folderInfo = new FolderInfo(directory);
            BuildFolderStructure(folderInfo);
            rootFolders.Add(folderInfo);
        }

        string[] rootCsFiles = Directory.GetFiles(rootPath, "*.cs");
        if (rootCsFiles.Length > 0)
        {
            FolderInfo rootFilesFolder = new FolderInfo(rootPath);
            foreach (string filePath in rootCsFiles)
            {
                rootFilesFolder.CsFiles.Add(new CSharpFileInfo(filePath));
            }
            rootFolders.Insert(0, rootFilesFolder);
        }
    }

    private void BuildFolderStructure(FolderInfo parentFolder)
    {
        string[] subDirectories = Directory.GetDirectories(parentFolder.FolderPath);
        foreach (string directory in subDirectories)
        {
            FolderInfo subFolder = new FolderInfo(directory);
            BuildFolderStructure(subFolder);
            parentFolder.SubFolders.Add(subFolder);
        }

        string[] csFiles = Directory.GetFiles(parentFolder.FolderPath, "*.cs");
        foreach (string filePath in csFiles)
        {
            parentFolder.CsFiles.Add(new CSharpFileInfo(filePath));
        }
    }

    private void SetAllFilesChoiceState(bool choice)
    {
        foreach (FolderInfo folder in rootFolders)
        {
            SetFolderAndFilesChoiceState(folder, choice);
        }
    }

    private void SetFolderAndFilesChoiceState(FolderInfo folder, bool choice)
    {
        folder.IsChoice = choice;
        foreach (CSharpFileInfo csFile in folder.CsFiles)
        {
            csFile.SetChoice(choice);
        }
        foreach (FolderInfo subFolder in folder.SubFolders)
        {
            SetFolderAndFilesChoiceState(subFolder, choice);
        }
    }

    private bool AreAllFilesSelected()
    {
        foreach (FolderInfo folder in rootFolders)
        {
            if (!AreFolderFilesSelected(folder))
                return false;
        }
        return true;
    }

    private bool AreFolderFilesSelected(FolderInfo folder)
    {
        foreach (CSharpFileInfo csFile in folder.CsFiles)
        {
            if (!csFile.IsChoice)
                return false;
        }
        foreach (FolderInfo subFolder in folder.SubFolders)
        {
            if (!AreFolderFilesSelected(subFolder))
                return false;
        }
        return true;
    }

    private void OutputSelectedFilesToTxt()
    {
        if (string.IsNullOrEmpty(OutputTxtPath))
        {
            EditorUtility.DisplayDialog("错误", "请先设置输出文件路径", "确定");
            return;
        }

        List<CSharpFileInfo> selectedFiles = GetSelectedFiles();
        if (selectedFiles.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有选择任何CSharp文件", "确定");
            return;
        }

        try
        {
            using (StreamWriter writer = new StreamWriter(OutputTxtPath, false, Encoding.UTF8))
            {
                writer.WriteLine("// 自动生成的CSharp代码文件");
                writer.WriteLine($"// 生成时间: {System.DateTime.Now}");
                writer.WriteLine($"// 共包含 {selectedFiles.Count} 个文件");
                writer.WriteLine("// ===========================================");
                writer.WriteLine();

                foreach (CSharpFileInfo file in selectedFiles)
                {
                    WriteFileContent(writer, file);
                }
            }

            EditorUtility.DisplayDialog("成功", $"已成功输出 {selectedFiles.Count} 个文件到:\n{OutputTxtPath}", "确定");

            // 可选：在资源管理器中显示文件
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{OutputTxtPath}\"");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("错误", $"输出文件时发生错误:\n{ex.Message}", "确定");
            Debug.LogError($"输出CSharp代码到txt文件失败: {ex}");
        }
    }

    private List<CSharpFileInfo> GetSelectedFiles()
    {
        List<CSharpFileInfo> selectedFiles = new List<CSharpFileInfo>();
        foreach (FolderInfo folder in rootFolders)
        {
            CollectSelectedFilesFromFolder(folder, selectedFiles);
        }
        return selectedFiles;
    }

    private void CollectSelectedFilesFromFolder(FolderInfo folder, List<CSharpFileInfo> selectedFiles)
    {
        // 添加当前文件夹中选中的文件
        foreach (CSharpFileInfo csFile in folder.CsFiles)
        {
            if (csFile.IsChoice)
            {
                selectedFiles.Add(csFile);
            }
        }

        // 递归添加子文件夹中选中的文件
        foreach (FolderInfo subFolder in folder.SubFolders)
        {
            CollectSelectedFilesFromFolder(subFolder, selectedFiles);
        }
    }

    private void WriteFileContent(StreamWriter writer, CSharpFileInfo file)
    {
        try
        {
            if (!File.Exists(file.ConfigFilePath))
            {
                writer.WriteLine($"// 文件不存在: {file.ConfigFileName}");
                writer.WriteLine();
                return;
            }

            string fileContent = File.ReadAllText(file.ConfigFilePath, Encoding.UTF8);

            writer.WriteLine("// ===========================================");
            writer.WriteLine($"// 文件名: {file.ConfigFileName}");
            writer.WriteLine($"// 文件路径: {file.ConfigFilePath}");
            writer.WriteLine($"// 文件大小: {new FileInfo(file.ConfigFilePath).Length} 字节");
            writer.WriteLine("// ===========================================");
            writer.WriteLine();
            writer.WriteLine(fileContent);
            writer.WriteLine();
            writer.WriteLine();
        }
        catch (System.Exception ex)
        {
            writer.WriteLine($"// 读取文件失败: {file.ConfigFileName}");
            writer.WriteLine($"// 错误信息: {ex.Message}");
            writer.WriteLine();
            Debug.LogError($"读取文件失败 {file.ConfigFilePath}: {ex}");
        }
    }
}