using System.IO;
using UnityEngine;

public class ResourcesInit : BaseManager<ResourcesInit>
{
    private bool hasInit = false;
    // 前置路径
    private string PrefixPath { get; set; }
    // 平台
    private string Platform { get; set; }

    public override void Start()
    {
        // 确保整个游戏过程中只进行依次ResourcesManager初始化
        if (hasInit) return;

        Platform = GetPlatform();
        // 获取打包文件前置路径，以AssetBundle结尾
        PrefixPath = Path.GetFullPath(Path.Combine(
            Application.dataPath, "../AssetBundle")).Replace("\\", "/");
        // 平台路径拼接
        PrefixPath += $"/{Platform}";

        ResourcesManager.Instance.Initialize(Platform, GetFileUrl, false, 0);

        hasInit = true;
    }

    // 获取当前平台
    private string GetPlatform()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
            default:
                throw new System.Exception($"未支持的平台:{Application.platform}");
        }
    }

    private string GetFileUrl(string assetUrl)
    {
        return $"{PrefixPath}/{assetUrl}";
    }
}
