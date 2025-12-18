using System.IO;
using UnityEditor;
using UnityEngine;

public class Tool_FastScreenShot : EditorWindow
{
    private static string directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\Screen Shot";
    private static string latestScreenshotPath = "";

    [MenuItem("PIXEL/Screen Capture/打开存储文件夹 &2", false, 2)]
    private static void ShowFolder()
    {
        if (File.Exists(latestScreenshotPath))
        {
            EditorUtility.RevealInFinder(latestScreenshotPath);
            return;
        }

        Directory.CreateDirectory(directory);
        EditorUtility.RevealInFinder(directory);
    }

    [MenuItem("PIXEL/Screen Capture/截图 &1", false, 1)]
    private static void TakeScreenshot()
    {
        Directory.CreateDirectory(directory);
        var currentTime = System.DateTime.Now;
        var filename = currentTime.ToString().Replace('/', '-').Replace(':', '_') + ".png";
        var path = directory + "\\" + filename;
        ScreenCapture.CaptureScreenshot(path);
        latestScreenshotPath = path;
        Debug.Log($"截图路径: <b>{path}</b> 分辨率： <b>{GetResolution()}</b>");
    }

    private static string GetResolution()
    {
        Vector2 size = Handles.GetMainGameViewSize();
        Vector2Int sizeInt = new Vector2Int((int)size.x, (int)size.y);
        return $"{sizeInt.x}x{sizeInt.y}";
    }
}
