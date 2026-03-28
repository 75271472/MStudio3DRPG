using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class Builder
{
#if UNITY_IOS
    private const string PLATFORM = "iOS";
#elif UNITY_ANDROID
    private const string PLATFORM = "Android";
#else
    private const string PLATFORM = "Windows";
#endif

    // 收集打包设置文件的进度范围，x为开始进度，y为结束进度，范围为0-0.2
    public static readonly Vector2 collectRuleFileProgress = new Vector2(0, 0.2f);
    // 收集资源依赖信息进度范围，x为开始进度，y为结束进度，范围为0.2-0.4
    private static readonly Vector2 getDependencyProgress = new Vector2(0.2f, 0.4f);
    // 收集bundle信息进度范围，x为开始进度，y为结束进度，范围为0.4-0.5
    private static readonly Vector2 collectBundleInfoProgress = new Vector2(0.4f, 0.5f);
    // 生成资源描述文件进度范围，x为开始进度，y为结束进度，范围为0.5-0.6
    private static readonly Vector2 generateBuildInfoProgress = new Vector2(0.5f, 0.6f);
    // 打包bundle进度范围，x为开始进度，y为结束进度，范围为0.6-0.7
    private static readonly Vector2 buildBundleProgress = new Vector2(0.6f, 0.7f);
    // 清空多余bundle进度范围，x为开始进度，y为结束进度，范围为0.7-0.9
    private static readonly Vector2 clearBundleProgress = new Vector2(0.7f, 0.9f);
    private static readonly Vector2 buildManifestProgress = new Vector2(0.9f, 1f);

    private static readonly Profiler buildProfiler = new Profiler(nameof(Builder));
    // 子任务Profiler
    private static readonly Profiler loadBuildSettingProfiler = 
        buildProfiler.CreateChild(nameof(LoadSetting));
    private static readonly Profiler switchPlatformProfiler = 
        buildProfiler.CreateChild(nameof(SwitchPlatform));
    private static readonly Profiler collectProfiler =
        buildProfiler.CreateChild(nameof(Collect));
    private static readonly Profiler collectBuildSettingFileProfiler = 
        collectProfiler.CreateChild("CollectBuildSettingFile");
    private static readonly Profiler collectDependencyProfiler = 
        collectProfiler.CreateChild(nameof(CollectDependency));
    private static readonly Profiler collectBundleProfiler = 
        collectProfiler.CreateChild(nameof(CollectBundle));
    private static readonly Profiler generateManifestProfiler = 
        collectProfiler.CreateChild(nameof(GenerateManifest));
    private static readonly Profiler buildBundleProfiler = 
        buildProfiler.CreateChild(nameof(BuildBundle));
    private static readonly Profiler clearBundleProfiler = 
        buildProfiler.CreateChild(nameof(ClearAssetBundle));
    private static readonly Profiler buildManifestBundleProfiler = 
        buildProfiler.CreateChild(nameof(BuildManifest));

    //bundle后缀
    public const string BUNDLE_SUFFIX = ".ab";
    public const string BUNDLE_MANIFEST_SUFFIX = ".manifest";
    //bundle描述文件名称
    public const string MANIFEST = "manifest";

    public static readonly ParallelOptions ParallelOptions = new ParallelOptions()
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount * 2
    };

    // 传入完成路径加载资源而不是资源名加载作用：
    // 如果允许用短名字加载资源，Unity 就必须在每个 AssetBundle 包的内部文件头里，
    // 额外维护一张“短名字 -> 内部资源索引”的查找表（Lookup Table）。
    // 当一个包里的资源非常多时，这张查找表的字符串占用会消耗非常可观的内存。
    // 禁用了短名字后，Unity 就不生成这张表了，从而显著降低了 AssetBundle 加载进内存时的基础开销。
    //bundle打包Options
    public readonly static BuildAssetBundleOptions BuildAssetBundleOptions =
        // 使用 LZ4 算法对 AssetBundle 进行基于块的压缩
        // 游戏在运行时可以按需解压（即用到哪个块的内容，就只解压哪个块）。
        // 它的加载速度与不压缩几乎一样快
        BuildAssetBundleOptions.ChunkBasedCompression |
        // 严格模式，如果在打包过程中发生任何错误（比如资源缺失、脚本编译报错、依赖找不到等）
        // 打包过程会立刻失败并中断，不会生成任何残缺的 AB 包。
        BuildAssetBundleOptions.StrictMode |
        // 禁止通过“不带扩展名的文件名”来加载资源，必须传入Assets文件夹下的完整路径加载资源
        // 默认情况下，可以通过 bundle.LoadAsset("LoginPanel") 来加载。
        // Assets/UI/LoginPanel.prefab
        BuildAssetBundleOptions.DisableLoadAssetByFileName |
        // 禁止通过“带扩展名的文件名”来加载资源
        // 默认情况下，可以通过 bundle.LoadAsset("LoginPanel.prefab") 加载
        // 而当前模式必须传入文件路径
        BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

    // 打包设置对象
    public static BuildSetting buildSetting { get; private set; }
    // 打包设置文件路径
    public static string buildPath { get; private set; }

    /// <summary>
    /// 打包配置
    /// </summary>
    public readonly static string BuildSettingPath = 
        Path.GetFullPath("BuildSetting.xml").Replace("\\", "/");

    /// <summary>
    /// 临时目录,临时生成的文件都统一放在该目录
    /// </summary>
    public readonly static string TempPath = 
        Path.GetFullPath(Path.Combine(Application.dataPath, "Temp")).Replace("\\", "/");

    /// <summary>
    /// 临时目录,临时文件的ab包都放在该文件夹，打包完成后会移除
    /// </summary>
    public readonly static string TempBuildPath = 
        Path.GetFullPath(Path.Combine(Application.dataPath, "../TempBuild")).Replace("\\", "/");

    /// <summary>
    /// 资源描述__文本
    /// </summary>
    public readonly static string ResourcePath_Text = $"{TempPath}/Resource.txt";

    /// <summary>
    /// 资源描述__二进制
    /// </summary>
    public readonly static string ResourcePath_Binary = $"{TempPath}/Resource.bytes";

    /// <summary>
    /// Bundle描述__文本
    /// </summary>
    public readonly static string BundlePath_Text = $"{TempPath}/Bundle.txt";

    /// <summary>
    /// Bundle描述__二进制
    /// </summary>
    public readonly static string BundlePath_Binary = $"{TempPath}/Bundle.bytes";

    /// <summary>
    /// 资源依赖描述__文本
    /// </summary>
    public readonly static string DependencyPath_Text = $"{TempPath}/Dependency.txt";

    /// <summary>
    /// 资源依赖描述__文本
    /// </summary>
    public readonly static string DependencyPath_Binary = $"{TempPath}/Dependency.bytes";

    #region Build MenuItem

    [MenuItem("Tools/ResBuild/Windows")]
    public static void BuildWindows()
    {
        Build();
    }

    [MenuItem("Tools/ResBuild/Android")]
    public static void BuildAndroid()
    {
        Build();
    }

    [MenuItem("Tools/ResBuild/iOS")]
    public static void BuildIos()
    {
        Build();
    }

    public static void SwitchPlatform()
    {
        string platform = PLATFORM;

        switch (platform)
        {
            case "Windows":
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                break; 
            case "Android":
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android, BuildTarget.Android);
                break;
            case "iOS":
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.iOS, BuildTarget.iOS);
                break;
        }
    }

    /// <summary>
    /// 读取配置文件，获取BuildSetting对象
    /// </summary>
    /// <param name="settingPath">配置文件路径</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static BuildSetting LoadSetting(string settingPath)
    {
        // 根据settingPath路径读取BuildSetting对象
        BuildSetting buildSetting = XmlUtility.Read<BuildSetting>(settingPath);
        if (buildSetting == null)
        {
            throw new Exception($"Load buildSetting failed,SettingPath:{settingPath}.");
        }

        (buildSetting as ISupportInitialize)?.EndInit();

        // 获取buildRoot的绝对路径，并确保路径以"/"结尾
        buildPath = Path.GetFullPath(buildSetting.buildRoot).Replace("\\", "/");
        if (buildPath.Length > 0 && buildPath[buildPath.Length - 1] != '/')
        {
            buildPath += "/";
        }
        // 在buildPath后面添加平台名称，并确保路径以"/"结尾
        buildPath += $"{PLATFORM}/";

        return buildSetting;
    }

    private static void Build()
    {
        buildProfiler.Start();

        // 切换平台
        switchPlatformProfiler.Start();
        SwitchPlatform();
        switchPlatformProfiler.Stop();

        // 读取BuildSetting文件，获取BuildSetting对象
        loadBuildSettingProfiler.Start();
        buildSetting = LoadSetting(BuildSettingPath);
        loadBuildSettingProfiler.Stop();

        // 收集Bundle信息，
        // 包括文件路径、文件依赖关系建立bundle与资源的映射关系，生辰资源描述文件
        collectProfiler.Start();
        Dictionary<string, List<string>> bundleDic = Collect();
        collectProfiler.Stop();

        //打包assetbundle
        buildBundleProfiler.Start();
        BuildBundle(bundleDic);
        buildBundleProfiler.Stop();

        //清空多余文件
        clearBundleProfiler.Start();
        ClearAssetBundle(buildPath, bundleDic);
        clearBundleProfiler.Stop();

        //把描述文件打包bundle
        buildManifestBundleProfiler.Start();
        BuildManifest();
        buildManifestBundleProfiler.Stop();

        EditorUtility.ClearProgressBar();

        buildProfiler.Stop();

        Debug.Log($"打包完成: {buildPath} {buildProfiler}");
    }

    private static Dictionary<string, List<string>> Collect()
    {
        // 获取所有需要打包的文件路径
        collectBuildSettingFileProfiler.Start();
        HashSet<string> files = buildSetting.Collect();
        collectBuildSettingFileProfiler.Stop();

        //搜集所有文件的依赖关系
        collectDependencyProfiler.Start();
        Dictionary<string, List<string>> dependencyDic = CollectDependency(files);
        collectDependencyProfiler.Stop();

        //标记所有资源的信息
        Dictionary<string, EResourceType> assetDic = 
            new Dictionary<string, EResourceType>();

        //被打包配置分析到的直接设置为Direct
        foreach (string url in files)
        {
            assetDic.Add(url, EResourceType.Direct);
        }

        //依赖的资源标记为Dependency，已经存在的说明是Direct的资源
        foreach (string url in dependencyDic.Keys)
        {
            if (!assetDic.ContainsKey(url))
            {
                assetDic.Add(url, EResourceType.Dependency);
            }
        }

        // 制定打包计划，即哪些资源要被打进哪个名字的 AB 包里
        // 键：bundle包名，值：bundle包包含的资源路径列表
        collectBundleProfiler.Start();
        Dictionary<string, List<string>> bundleDic = 
            CollectBundle(buildSetting, assetDic, dependencyDic);
        collectBundleProfiler.Stop();

        // 生成Manifest文件
        // 包括资源描述文件（id与资源文件的映射关系）、bundle描述文件、资源依赖描述文件
        generateManifestProfiler.Start();
        GenerateManifest(assetDic, bundleDic, dependencyDic);
        generateManifestProfiler.Stop();

        return bundleDic;
    }

    /// <summary>
    /// 收集指定文件集合所有的依赖信息
    /// </summary>
    /// <param name="files">文件集合</param>
    /// <returns>依赖信息</returns>
    private static Dictionary<string, List<string>> CollectDependency(
        ICollection<string> files)
    {
        float min = getDependencyProgress.x;
        float max = getDependencyProgress.y;

        Dictionary<string, List<string>> dependencyDic = 
            new Dictionary<string, List<string>>();

        //声明fileList后，就不需要递归了
        List<string> fileList = new List<string>(files);

        for (int i = 0; i < fileList.Count; i++)
        {
            string assetUrl = fileList[i];

            if (dependencyDic.ContainsKey(assetUrl))
                continue;

            // 每处理10个文件，更新一次进度条
            if (i % 10 == 0)
            {
                //只能大概模拟进度
                float progress = min + (max - min) * ((float)i / (files.Count * 3));
                EditorUtility.DisplayProgressBar($"{nameof(CollectDependency)}", "搜集依赖信息", progress);
            }

            // 获取assetUrl的所有依赖信息，第二个参数为false，表示只获取直接依赖，不获取间接依赖
            string[] dependencies = AssetDatabase.GetDependencies(assetUrl, false);
            List<string> dependencyList = new List<string>(dependencies.Length);

            //过滤掉不符合要求的依赖资源
            for (int ii = 0; ii < dependencies.Length; ii++)
            {
                string tempAssetUrl = dependencies[ii];
                string extension = Path.GetExtension(tempAssetUrl).ToLower();
                // 过滤掉.cs和.dll文件，因为它们不是资源文件，不需要打包
                if (string.IsNullOrEmpty(extension) || extension == ".cs" || 
                    extension == ".dll")
                    continue;
                dependencyList.Add(tempAssetUrl);
                // 将依赖资源添加到fileList中，迭代处理依赖资源的依赖信息
                if (!fileList.Contains(tempAssetUrl))
                    fileList.Add(tempAssetUrl);
            }

            dependencyDic.Add(assetUrl, dependencyList);
        }

        return dependencyDic;
    }

    /// <summary>
    /// 搜集bundle对应的ab名字
    /// </summary>
    /// <param name="buildSetting"></param>
    /// <param name="assetDic">带标记的资源列表</param>
    /// <param name="dependencyDic">资源依赖信息</param>
    /// <returns>bundle包信息</returns>
    private static Dictionary<string, List<string>> CollectBundle(
        BuildSetting buildSetting, Dictionary<string, EResourceType> assetDic, 
        Dictionary<string, List<string>> dependencyDic)
    {
        float min = collectBundleInfoProgress.x;
        float max = collectBundleInfoProgress.y;

        EditorUtility.DisplayProgressBar($"{nameof(CollectBundle)}", "搜集bundle信息", min);

        Dictionary<string, List<string>> bundleDic = 
            new Dictionary<string, List<string>>();
        //外部资源
        List<string> notInRuleList = new List<string>();

        int index = 0;
        foreach (KeyValuePair<string, EResourceType> pair in assetDic)
        {
            index++;
            string assetUrl = pair.Key;
            // 根据资源路径和资源类型获取bundle包名，buildSetting.
            // GetBundleName方法会根据打包规则来确定bundle包名，
            // 如果资源不符合任何打包规则，则返回null
            string bundleName = buildSetting.GetBundleName(assetUrl, pair.Value);

            //没有bundleName的资源为外部资源
            if (bundleName == null)
            {
                notInRuleList.Add(assetUrl);
                continue;
            }

            // 同属一个bundle包的资源放在同一个列表里
            List<string> list;
            if (!bundleDic.TryGetValue(bundleName, out list))
            {
                list = new List<string>();
                bundleDic.Add(bundleName, list);
            }

            list.Add(assetUrl);

            EditorUtility.DisplayProgressBar($"{nameof(CollectBundle)}", "搜集bundle信息", min + (max - min) * ((float)index / assetDic.Count));
        }

        if (notInRuleList.Count > 0)
        {
            string massage = string.Empty;
            for (int i = 0; i < notInRuleList.Count; i++)
            {
                massage += "\n" + notInRuleList[i];
            }
            EditorUtility.ClearProgressBar();
            throw new Exception($"资源不在打包规则,或者后缀不匹配！！！{massage}");
        }

        // 对同一bundlename的资源路径进行排序，
        // 保证每次打包时同一bundlename的资源路径顺序一致，
        // 避免因为资源路径顺序不同而导致bundle包hash值不同
        foreach (List<string> list in bundleDic.Values)
        {
            list.Sort();
        }

        return bundleDic;
    }

    /// <summary>
    /// 生成资源描述文件
    /// <param name="assetDic">资源与资源类型映射表</param>
    /// <param name="bundleDic">同一bundle包名的资源列表映射</param>
    /// <param name="dependencyDic">资源的依赖资源映射</param>
    /// </summary>
    private static void GenerateManifest(Dictionary<string, EResourceType> assetDic, 
        Dictionary<string, List<string>> bundleDic, 
        Dictionary<string, List<string>> dependencyDic)
    {
        float min = generateBuildInfoProgress.x;
        float max = generateBuildInfoProgress.y;

        EditorUtility.DisplayProgressBar($"{nameof(GenerateManifest)}", "生成打包信息", min);

        //生成临时存放文件的目录
        if (!Directory.Exists(TempPath))
            Directory.CreateDirectory(TempPath);

        //资源映射id
        Dictionary<string, ushort> assetIdDic = new Dictionary<string, ushort>();

        #region 生成资源描述文件
        {
            // 删除旧的资源描述文件
            if (File.Exists(ResourcePath_Text))
                File.Delete(ResourcePath_Text);

            //删除旧的资源描述二进制文件
            if (File.Exists(ResourcePath_Binary))
                File.Delete(ResourcePath_Binary);

            //写入资源列表
            StringBuilder resourceSb = new StringBuilder();
            MemoryStream resourceMs = new MemoryStream();
            BinaryWriter resourceBw = new BinaryWriter(resourceMs);

            // 资源数量超过ushort的最大值，无法用ushort来表示资源id，抛出异常
            // ushort 16位 2字节 无符号整数，最大值为65535
            if (assetDic.Count > ushort.MaxValue)
            {
                EditorUtility.ClearProgressBar();
                throw new Exception($"资源个数超出{ushort.MaxValue}");
            }

            // 资源个数写入二进制文件
            resourceBw.Write((ushort)assetDic.Count);
            List<string> keys = new List<string>(assetDic.Keys);
            keys.Sort();

            // 文本文件写入格式：资源id\t资源路径
            // 二进制文件写入格式：资源路径(string)
            // 同时建立资源与资源id的映射关系，资源id为ushort类型，从0开始递增
            for (ushort i = 0; i < keys.Count; i++)
            {
                string assetUrl = keys[i];
                assetIdDic.Add(assetUrl, i);
                resourceSb.AppendLine($"{i}\t{assetUrl}");
                resourceBw.Write(assetUrl);
            }

            // 将数据从缓冲区刷入底层流中，为后续获取字节数组做准备
            resourceBw.Flush();
            // 获取二进制资源描述信息的字节数组
            byte[] buffer = resourceMs.GetBuffer();
            resourceBw.Close();
            //写入资源描述文本文件
            File.WriteAllText(ResourcePath_Text, resourceSb.ToString(), Encoding.UTF8);
            File.WriteAllBytes(ResourcePath_Binary, buffer);
        }
        #endregion
        
        // 完成 30% 的进度
        EditorUtility.DisplayProgressBar($"{nameof(GenerateManifest)}", "生成打包信息", min + (max - min) * 0.3f);

        #region 生成bundle描述信息
        {
            //删除bundle描述文本文件
            if (File.Exists(BundlePath_Text))
                File.Delete(BundlePath_Text);

            //删除bundle描述二进制文件
            if (File.Exists(BundlePath_Binary))
                File.Delete(BundlePath_Binary);

            //写入bundle信息
            StringBuilder bundleSb = new StringBuilder();
            MemoryStream bundleMs = new MemoryStream();
            BinaryWriter bundleBw = new BinaryWriter(bundleMs);

            //写入bundle个数
            bundleBw.Write((ushort)bundleDic.Count);
            // 遍历每个bundle
            foreach (var kv in bundleDic)
            {
                string bundleName = kv.Key;
                List<string> assets = kv.Value;

                //写入bundle名
                bundleSb.AppendLine(bundleName);
                bundleBw.Write(bundleName);

                //写入该bundle中的资源个数
                bundleBw.Write((ushort)assets.Count);

                // 遍历bundle中的每个资源
                for (int i = 0; i < assets.Count; i++)
                {
                    string assetUrl = assets[i];
                    ushort assetId = assetIdDic[assetUrl];
                    bundleSb.AppendLine($"\t{assetUrl}");
                    //写入资源id,用id替换字符串可以节省内存
                    bundleBw.Write(assetId);
                }
            }

            bundleBw.Flush();
            byte[] buffer = bundleMs.GetBuffer();
            bundleBw.Close();
            //写入资源描述文本文件
            File.WriteAllText(BundlePath_Text, bundleSb.ToString(), Encoding.UTF8);
            File.WriteAllBytes(BundlePath_Binary, buffer);
        }
        #endregion

        // 完成 80% 的进度
        EditorUtility.DisplayProgressBar($"{nameof(GenerateManifest)}", "生成打包信息", min + (max - min) * 0.8f);

        #region 生成资源依赖描述信息
        {
            //删除资源依赖描述文本文件
            if (File.Exists(DependencyPath_Text))
                File.Delete(DependencyPath_Text);

            //删除资源依赖描述二进制文件
            if (File.Exists(DependencyPath_Binary))
                File.Delete(DependencyPath_Binary);

            //写入资源依赖信息
            StringBuilder dependencySb = new StringBuilder();
            MemoryStream dependencyMs = new MemoryStream();
            BinaryWriter dependencyBw = new BinaryWriter(dependencyMs);

            //用于保存资源依赖链
            List<List<ushort>> dependencyList = new List<List<ushort>>();
            foreach (var kv in dependencyDic)
            {
                List<string> dependencyAssets = kv.Value;

                //依赖为0的不需要写入
                if (dependencyAssets.Count == 0)
                    continue;

                string assetUrl = kv.Key;

                List<ushort> ids = new List<ushort>();
                // 第一个id为资源本身的id，后面跟着依赖资源的id
                ids.Add(assetIdDic[assetUrl]);
                // 第一个为资源本身url，后面跟着依赖资源的url
                string content = assetUrl;
                for (int i = 0; i < dependencyAssets.Count; i++)
                {
                    string dependencyAssetUrl = dependencyAssets[i];
                    content += $"\t{dependencyAssetUrl}";
                    ids.Add(assetIdDic[dependencyAssetUrl]);
                }
                // 资源与资源间使用换行符分隔，资源与依赖资源之间使用制表符分隔
                dependencySb.AppendLine(content);

                if (ids.Count > byte.MaxValue)
                {
                    EditorUtility.ClearProgressBar();
                    throw new Exception($"资源{assetUrl}的依赖超出一个字节上限:{byte.MaxValue}");
                }

                dependencyList.Add(ids);
            }

            // 二进制依赖描述文件写入：依赖个数 资源依赖文件个数 资源依赖文件id
            dependencyBw.Write((ushort)dependencyList.Count);
            for (int i = 0; i < dependencyList.Count; i++)
            {
                //写入资源数
                List<ushort> ids = dependencyList[i];
                dependencyBw.Write((ushort)ids.Count);
                for (int ii = 0; ii < ids.Count; ii++)
                    dependencyBw.Write(ids[ii]);
            }

            dependencyMs.Flush();
            byte[] buffer = dependencyMs.GetBuffer();
            dependencyBw.Close();
            //写入资源依赖描述文本文件
            File.WriteAllText(DependencyPath_Text, dependencySb.ToString(), Encoding.UTF8);
            File.WriteAllBytes(DependencyPath_Binary, buffer);
        }
        #endregion

        AssetDatabase.Refresh();
        // 完成 100% 的进度
        EditorUtility.DisplayProgressBar($"{nameof(GenerateManifest)}", "生成打包信息", max);
        // 清除进度条
        EditorUtility.ClearProgressBar();
    }
    #endregion

    /// <summary>
    /// 打包AssetBundle
    /// <param name="assetDic">资源列表</param>
    /// <param name="bundleDic">bundle包信息</param>
    /// <param name="dependencyDic">资源依赖信息</param>
    /// </summary>
    private static AssetBundleManifest BuildBundle(Dictionary<string, 
        List<string>> bundleDic)
    {
        float min = buildBundleProgress.x;
        float max = buildBundleProgress.y;

        EditorUtility.DisplayProgressBar($"{nameof(BuildBundle)}", "打包AssetBundle", min);

        // buildPath 即buildSetting.buildRoot
        if (!Directory.Exists(buildPath))
            Directory.CreateDirectory(buildPath);

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(buildPath, 
            GetBuilds(bundleDic), BuildAssetBundleOptions,
            EditorUserBuildSettings.activeBuildTarget);

        EditorUtility.DisplayProgressBar($"{nameof(BuildBundle)}", "打包AssetBundle", max);

        return manifest;
    }

    /// <summary>
    /// 清空多余的assetbundle
    /// </summary>
    /// <param name="path">打包路径</param>
    /// <param name="bundleDic"></param>
    private static void ClearAssetBundle(string path, 
        Dictionary<string, List<string>> bundleDic)
    {
        float min = clearBundleProgress.x;
        float max = clearBundleProgress.y;

        EditorUtility.DisplayProgressBar($"{nameof(ClearAssetBundle)}", "清除多余的AssetBundle文件", min);
        // 获取path中的所有文件，搜索选项为AllDirectories，即搜索所有子目录
        List<string> fileList = GetFiles(path, null, null);
        // 将文件放入待删除集合去重
        HashSet<string> fileSet = new HashSet<string>(fileList);

        // 移需要保留的bundle包主体.ab文件 和
        // 存储哈希值等AB文件元数据的.manifest文件
        // bundleDic中包含所有资源的bundle包名，包括已被打入buildPath中的资源，和被修改或新添加的资源
        // 每次都会根据新的打包规则和现有资源得到新的bundleDic
        // 对于被删除的资源或根据旧打包规则打包的资源，不包含在buildDic中，会被删除
        // 只会打入被修改或新添加的资源，实现增量打包
        foreach (string bundle in bundleDic.Keys)
        {
            fileSet.Remove($"{path}{bundle}");
            fileSet.Remove($"{path}{bundle}{BUNDLE_MANIFEST_SUFFIX}");
        }
        // 移除平台文件
        fileSet.Remove($"{path}{PLATFORM}");
        fileSet.Remove($"{path}{PLATFORM}{BUNDLE_MANIFEST_SUFFIX}");
        // 多线程并行处理，删除剩余的多余文件
        Parallel.ForEach(fileSet, ParallelOptions, File.Delete);

        EditorUtility.DisplayProgressBar($"{nameof(ClearAssetBundle)}", "清除多余的AssetBundle文件", max);
    }

    /// <summary>
    /// 把Resource.bytes、bundle.bytes、Dependency.bytes 
    /// 二进制描述文件打入一个AB包 manifest.ab中
    /// </summary>
    private static void BuildManifest()
    {
        float min = buildManifestProgress.x;
        float max = buildManifestProgress.y;

        EditorUtility.DisplayProgressBar($"{nameof(BuildManifest)}", "将Manifest打包成AssetBundle", min);
        // 除了.ab文件，还会生成.manifest文件、无后缀名文件
        // 将生成文件放入临时目录中，避免.manifest文件、无后缀名文件污染buildPath目录
        // 每在平台目录（Windows/）中打包一次，都会生成全局文件：一个 Windows文件和 Windows.manifest文件
        // 全局文件记录了游戏里所有成百上千个资源包的依赖关系和 Hash 校验码
        // 如果先后在 平台目录中先后对资源和描述文件进行两次打包，
        // 描述文件打包会直接覆盖，使得资源文件打包时生成全局文件丢失

        // .manifest文件存储 hash值，用于资源文件的增量更新，
        // 但描述文件无需增量更新，所有只会将增量文件 的 manifest.ab 放入 buildpath目录中
        if (!Directory.Exists(TempBuildPath))
            Directory.CreateDirectory(TempBuildPath);

        string prefix = Application.dataPath.Replace("/Assets", "/").Replace("\\", "/");

        AssetBundleBuild manifest = new AssetBundleBuild();
        manifest.assetBundleName = $"{MANIFEST}{BUNDLE_SUFFIX}";
        manifest.assetNames = new string[3]
        {
            // AssetBundleBuild要求传入路径为Assets下的相对路径，以 Assets/... 开头
            ResourcePath_Binary.Replace(prefix,""),
            BundlePath_Binary.Replace(prefix,""),
            DependencyPath_Binary.Replace(prefix,""),
        };

        EditorUtility.DisplayProgressBar($"{nameof(BuildManifest)}", "将Manifest打包成AssetBundle", min + (max - min) * 0.5f);

        // 生成包 manifest.ab 到 Tempbuild目录下
        AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(
            TempBuildPath, new AssetBundleBuild[] { manifest }, 
            BuildAssetBundleOptions, EditorUserBuildSettings.activeBuildTarget);

        //把文件copy到build目录
        if (assetBundleManifest)
        {
            string manifestFile = $"{TempBuildPath}/{MANIFEST}{BUNDLE_SUFFIX}";
            string target = $"{buildPath}/{MANIFEST}{BUNDLE_SUFFIX}";
            if (File.Exists(manifestFile))
            {
                File.Copy(manifestFile, target);
            }
        }

        //删除临时目录
        if (Directory.Exists(TempBuildPath))
            Directory.Delete(TempBuildPath, true);

        EditorUtility.DisplayProgressBar($"{nameof(BuildManifest)}", "将Manifest打包成AssetBundle", max);
    }

    /// <summary>
    /// 获取所有需要打包的AssetBundleBuild
    /// </summary>
    /// <param name="bundleTable">bundleName与资源列表的映射</param>
    /// <returns></returns>
    private static AssetBundleBuild[] GetBuilds(
        Dictionary<string, List<string>> bundleTable)
    {
        int index = 0;
        AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[bundleTable.Count];
        foreach (KeyValuePair<string, List<string>> pair in bundleTable)
        {
            // 一个bundleName对应一个AssetBundleBuild，
            // AssetBundleBuild包含bundleName和资源路径列表
            assetBundleBuilds[index++] = new AssetBundleBuild()
            {
                assetBundleName = pair.Key,
                assetNames = pair.Value.ToArray(),
            };
        }

        return assetBundleBuilds;
    }

    /// <summary>
    /// 获取指定路径中指定后缀的文件列表
    /// 如果文件是以prefix开头的，则忽略该文件
    /// </summary>
    /// <param name="path">指定路径</param>
    /// <param name="prefix">前缀</param>
    /// <param name="suffixes">后缀集合</param>
    /// <returns>文件列表</returns>
    public static List<string> GetFiles(string path, string prefix, 
        params string[] suffixes)
    {
        // 获取指定路径中所有文件列表，搜索选项为AllDirectories，即搜索所有子目录
        string[] files = Directory.GetFiles(path, $"*.*", SearchOption.AllDirectories);
        // 设置足够长的列表，以避免在添加文件时频繁扩容
        List<string> result = new List<string>(files.Length);

        for (int i = 0; i < files.Length; ++i)
        {
            string file = files[i].Replace('\\', '/');

            // 如果文件是以prefix开头的，则忽略该文件
            if (prefix != null && !file.StartsWith(prefix, 
                StringComparison.InvariantCulture))
            {
                continue;
            }

            // 如果文件是以suffixes中任意一个后缀结尾的，则保留该文件，否则忽略该文件
            if (suffixes != null && suffixes.Length > 0)
            {
                bool exist = false;

                for (int ii = 0; ii < suffixes.Length; ii++)
                {
                    string suffix = suffixes[ii];
                    if (file.EndsWith(suffix, StringComparison.InvariantCulture))
                    {
                        exist = true;
                        break;
                    }
                }

                if (!exist)
                    continue;
            }

            result.Add(file);
        }

        return result;
    }
}
