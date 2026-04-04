using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;

//<? xml version = "1.0" encoding = "utf-8" ?>
//< BuildSetting ProjectName = "haru" BuildRoot = "../AssetBundle" >
//  < BuildItem AssetPath = "Assets/AssetBundle/Common/" ResourceType = "Direct" BundleType = "File" Suffix = ".renderTexture" />
//  < BuildItem AssetPath = "Assets/AssetBundle/Atlas/" ResourceType = "Direct" BundleType = "Directory" Suffix = ".png|.spriteatlas" />
//  < BuildItem AssetPath = "Assets/AssetBundle/Background/" ResourceType = "Direct" BundleType = "File" Suffix = ".png" />
//  < BuildItem AssetPath = "Assets/AssetBundle/Icon/" ResourceType = "Direct" BundleType = "Directory" Suffix = ".png" />
//  < BuildItem AssetPath = "Assets/AssetBundle/Model/" ResourceType = "Direct" BundleType = "Directory" Suffix = ".prefab" />
//  < BuildItem AssetPath = "Assets/AssetBundle/Shader/" ResourceType = "Direct" BundleType = "Directory" Suffix = ".shader" />
//  < BuildItem AssetPath = "Assets/AssetBundle/UI/" ResourceType = "Direct" BundleType = "File" Suffix = ".prefab" />
//</ BuildSetting >

public class BuildSetting : ISupportInitialize
{
    [DisplayName("项目名称")]
    [XmlAttribute("ProjectName")]
    public string projectName { get; set; }

    [DisplayName("后缀列表")]
    [XmlAttribute("SuffixList")]
    public List<string> suffixList { get; set; } = new List<string>();

    [DisplayName("生成打包文件目标文件夹")]
    [XmlAttribute("BuildRoot")]
    public string buildRoot { get; set; }

    [DisplayName("打包选项")]
    [XmlElement("BuildItem")]
    public List<BuildItem> items { get; set; } = new List<BuildItem>();

    [XmlIgnore]
    public Dictionary<string, BuildItem> buildItemDic =
        new Dictionary<string, BuildItem>();

    public void BeginInit()
    {

    }

    public void EndInit()
    {
        buildRoot = Path.GetFullPath(buildRoot).Replace("\\", "/");
        buildItemDic.Clear();

        for (int i = 0; i < items.Count; i++)
        {
            BuildItem buildItem = items[i];

            // 如果是文件或者目录打包，检查路径是否存在
            if (buildItem.bundleType == EBundleType.All ||
                buildItem.bundleType == EBundleType.Directory)
            {
                if (!Directory.Exists(buildItem.assetPath))
                {
                    throw new Exception($"不存在资源路径:{buildItem.assetPath}");
                }
            }

            // 打包数据类型以"|"进行分割，获取所有打包数据类型
            string[] prefixes = buildItem.suffix.Split('|');
            // 将所有打包数据类型添加到buildItem的suffixes列表中
            for (int ii = 0; ii < prefixes.Length; ii++)
            {
                // 把读取到的所有后缀也进行一次 ToLower()，防止在大小写敏感判断中出错
                string prefix = prefixes[ii].Trim().ToLower(); 
                if (!string.IsNullOrEmpty(prefix))
                    buildItem.suffixes.Add(prefix);
            }

            // 如果该打包项的资源路径已经存在于buildItemDic中，说明有重复的资源路径，抛出异常
            if (buildItemDic.ContainsKey(buildItem.assetPath))
            {
                throw new Exception($"重复的资源路径:{buildItem.assetPath}");
            }
            // 将该打包项添加到buildItemDic中，键为资源路径，值为打包项
            buildItemDic.Add(buildItem.assetPath, buildItem);
        }
    }

    /// <summary>
    /// 获取所有在打包设置的文件列表
    /// </summary>
    /// <returns>文件列表</returns>
    public HashSet<string> Collect()
    {
        float min = Builder.collectRuleFileProgress.x;
        float max = Builder.collectRuleFileProgress.y;

        EditorUtility.DisplayProgressBar($"{nameof(Collect)}", "搜集打包规则资源", min);

        //处理每个规则忽略的目录,buildI.assetPath = A/B buildJ.assetPath = A/B/C
        // buildI打包时会忽略A/B/C，避免buildJ重复打包A/B/C的资源
        for (int i = 0; i < items.Count; i++)
        {
            BuildItem buildItem_i = items[i];
            // 只检索主动配置项
            if (buildItem_i.resourceType != EResourceType.Direct)
                continue;

            buildItem_i.ignorePaths.Clear();
            for (int j = 0; j < items.Count; j++)
            {
                BuildItem buildItem_j = items[j];
                if (i != j && buildItem_j.resourceType == EResourceType.Direct)
                {
                    // 如果buildItem_j的资源路径以buildItem_i的资源路径开头，
                    // 说明buildItem_j是buildItem_i的子目录或者子文件，
                    // 需要将buildItem_j的资源路径添加到buildItem_i的ignorePaths列表中
                    // buildItem_i在打包时会忽略buildItem_j的资源路径，从而避免重复打包
                    if (buildItem_j.assetPath.StartsWith(buildItem_i.assetPath,
                        StringComparison.InvariantCulture))
                    {
                        buildItem_i.ignorePaths.Add(buildItem_j.assetPath);
                    }
                }
            }
        }

        // 使用集合存储被规则分析到的所有文件，避免重复文件
        HashSet<string> files = new HashSet<string>();

        // 遍历打包设置中的每个打包项，搜集符合条件的文件列表，并将文件添加到files集合中
        for (int i = 0; i < items.Count; i++)
        {
            BuildItem buildItem = items[i];
            // 更新进度条
            EditorUtility.DisplayProgressBar($"{nameof(Collect)}", "搜集打包规则资源",
                min + (max - min) * ((float)i / (items.Count - 1)));

            if (buildItem.resourceType != EResourceType.Direct)
                continue;

            // 获取buildItem.assetPath路径下所有符合buildItem.suffixes列表中后缀的文件列表
            List<string> tempFiles = Builder.GetFiles(buildItem.assetPath, null,
                buildItem.suffixes.ToArray());
            // 遍历文件列表，过滤掉被忽略的文件，并将剩余的文件添加到files集合中
            for (int j = 0; j < tempFiles.Count; j++)
            {
                string file = tempFiles[j];

                //过滤被忽略的
                if (IsIgnore(buildItem.ignorePaths, file))
                    continue;

                files.Add(file);
            }

            // 更新进度条
            EditorUtility.DisplayProgressBar($"{nameof(Collect)}", "搜集打包设置资源",
                (float)(i + 1) / items.Count);
        }

        return files;
    }

    /// <summary>
    /// 文件是否在忽略列表列表
    /// </summary>
    /// <param name="ignoreList">忽略路径列</param>
    /// <param name="file">文件路径</param>
    /// <returns></returns>
    public bool IsIgnore(List<string> ignoreList, string file)
    {
        for (int i = 0; i < ignoreList.Count; i++)
        {
            string ignorePath = ignoreList[i];
            if (string.IsNullOrEmpty(ignorePath))
                continue;
            // 如果文件路径以ignorePath开头，说明该文件在ignorePath路径下，需要被忽略，返回true
            if (file.StartsWith(ignorePath, StringComparison.InvariantCulture))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 根据资源路径获取BuildItem
    /// </summary>
    /// <param name="assetUrl">资源路径</param>
    /// <returns>打包选项</returns>
    public BuildItem GetBuildItem(string assetUrl)
    {
        BuildItem item = null;
        for (int i = 0; i < items.Count; ++i)
        {
            BuildItem tempItem = items[i];
            //前面是否匹配
            if (assetUrl.StartsWith(tempItem.assetPath, StringComparison.InvariantCulture))
            {
                //找到优先级最高的Rule,路径越长说明优先级越高
                if (item == null || item.assetPath.Length < tempItem.assetPath.Length)
                {
                    item = tempItem;
                }
            }
        }

        return item;
    }

    /// <summary>
    /// 获取BundleName
    /// </summary>
    /// <param name="assetUrl">资源路径</param>
    /// <param name="resourceType">资源类型</param>
    /// <returns>BundleName</returns>
    public string GetBundleName(string assetUrl, EResourceType resourceType)
    {
        // 根据资源路径匹配BuildItem
        BuildItem buildItem = GetBuildItem(assetUrl);
        // 如果没有BuildItem匹配该资源路径
        if (buildItem == null)
        {
            return null;
        }

        string name;

        //依赖类型一定要匹配后缀
        if (buildItem.resourceType == EResourceType.Dependency)
        {
            string extension = Path.GetExtension(assetUrl).ToLower();
            bool exist = false;
            for (int i = 0; i < buildItem.suffixes.Count; i++)
            {
                if (buildItem.suffixes[i] == extension)
                {
                    exist = true;
                }
            }
            // 如果资源没有匹配BuildItem中的任何一个后缀
            if (!exist)
            {
                return null;
            }
        }

        switch (buildItem.bundleType)
        {
            case EBundleType.All:
                name = buildItem.assetPath;
                if (buildItem.assetPath[buildItem.assetPath.Length - 1] == '/')
                    name = buildItem.assetPath.Substring(0, buildItem.assetPath.Length - 1);
                name = $"{name}{Builder.BUNDLE_SUFFIX}".ToLowerInvariant();
                break;
            case EBundleType.Directory:
                // 截取最后一个 / 之前的路径
                name = $"{assetUrl.Substring(0, assetUrl.LastIndexOf('/'))}{Builder.BUNDLE_SUFFIX}".ToLowerInvariant();
                break;
            case EBundleType.File:
                name = $"{assetUrl}{Builder.BUNDLE_SUFFIX}".ToLowerInvariant();
                break;
            default:
                throw new Exception($"无法获取{assetUrl}的BundleName");
        }

        buildItem.Count += 1;

        return name;
    }
}
