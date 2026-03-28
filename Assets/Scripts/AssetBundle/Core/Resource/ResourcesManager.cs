using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using UnityEditor.Experimental;
using UnityEngine;

public class ResourcesManager : BaseManager<ResourcesManager>
{
    private const string MANIFEST_BUNDLE = "manifest.ab";
    private const string RESOURCE_ASSET_NAME = "Assets/Temp/Resource.bytes";
    private const string BUNDLE_ASSET_NAME = "Assets/Temp/Bundle.bytes";
    private const string DEPENDENCY_ASSET_NAME = "Assets/Temp/Dependency.bytes";

    /// <summary>
    /// 是否使用AssetDatabase进行加载
    /// </summary>
    private bool isEditor;

    /// <summary>
    /// 资源与Bundle名的映射
    /// </summary>
    internal Dictionary<string, string> ResourceBunldeDic =
        new Dictionary<string, string>();

    /// <summary>
    /// 资源 与 依赖资源 List 间的映射
    /// </summary>
    internal Dictionary<string, List<string>> ResourceDependencyDic =
        new Dictionary<string, List<string>>();

    /// <summary>
    /// 资源路径与AResource映射，已加载的所有资源集合，进行资源缓存，不用每次都从AB中获取
    /// </summary>
    private Dictionary<string, AResource> resourceDic =
        new Dictionary<string, AResource>();

    /// <summary>
    /// 需要释放的资源
    /// </summary>
    private LinkedList<AResource> needUnloadList = new LinkedList<AResource>();

    /// <summary>
    /// 异步加载集合
    /// </summary>
    private List<AResourceAsync> asyncList = new List<AResourceAsync>();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="platform">平台</param>
    /// <param name="getFileCallback">获取资源真实路径回调</param>
    /// <param name="editor">是否直接使用AssetDataBase加载</param>
    /// <param name="offset">获取bundle的偏移值 打包加密</param>
    public void Initialize(string platform, Func<string, string> getFileCallback,
        bool editor, ulong offset)
    {
        isEditor = editor;

        if (isEditor) return;

        BundleManager.Instance.Initialize(platform, getFileCallback, offset);

        string manifestBunldeFile = getFileCallback.Invoke(MANIFEST_BUNDLE);
        AssetBundle manifestAssetBundle = AssetBundle.LoadFromFile(
            manifestBunldeFile, 0, offset);

        TextAsset resourceTextAsset = manifestAssetBundle.LoadAsset(
            RESOURCE_ASSET_NAME) as TextAsset;
        TextAsset bundleTextAsset = manifestAssetBundle.LoadAsset(
            BUNDLE_ASSET_NAME) as TextAsset;
        TextAsset dependencyTextAsset = manifestAssetBundle.LoadAsset(
            DEPENDENCY_ASSET_NAME) as TextAsset;

        byte[] resourceBytes = resourceTextAsset.bytes;
        byte[] bundleBytes = bundleTextAsset.bytes;
        byte[] dependencyBytes = dependencyTextAsset.bytes;

        manifestAssetBundle.Unload(true);
        manifestAssetBundle = null;

        //保存id对应的asseturl
        Dictionary<ushort, string> assetUrlDic = new Dictionary<ushort, string>();

        // resources二进制描述文件中记录的是
        // 所有资源数量 每个资源路径
        #region 读取资源信息
        {
            MemoryStream resourceMemoryStream = new MemoryStream(resourceBytes);
            BinaryReader resourceBinaryReader = new BinaryReader(resourceMemoryStream);
            //获取资源个数
            ushort resourceCount = resourceBinaryReader.ReadUInt16();
            // id从0开始增长
            for (ushort i = 0; i < resourceCount; i++)
            {
                // 读取资源路径
                string assetUrl = resourceBinaryReader.ReadString();
                assetUrlDic.Add(i, assetUrl);
            }
        }
        #endregion

        // bundle二进制描述文件中记录的是
        // bundle个数 每个bundle路径 每个bundle中资源个数 每个bundle中资源id
        #region 读取bundle信息
        {
            ResourceBunldeDic.Clear();
            MemoryStream bundleMemoryStream = new MemoryStream(bundleBytes);
            BinaryReader bundleBinaryReader = new BinaryReader(bundleMemoryStream);
            //获取bundle个数
            ushort bundleCount = bundleBinaryReader.ReadUInt16();
            // 遍历所有Bundle
            for (int i = 0; i < bundleCount; i++)
            {
                // 获取Bundle路径
                string bundleUrl = bundleBinaryReader.ReadString();
                //string bundleFileUrl = getFileCallback(bundleUrl);
                string bundleFileUrl = bundleUrl;
                //获取bundle内的资源个数
                ushort resourceCount = bundleBinaryReader.ReadUInt16();
                // 获取资源id，根据id，建立资源路径 - bundle路径映射
                for (int ii = 0; ii < resourceCount; ii++)
                {
                    ushort assetId = bundleBinaryReader.ReadUInt16();
                    string assetUrl = assetUrlDic[assetId];
                    ResourceBunldeDic.Add(assetUrl, bundleFileUrl);
                }
            }
        }
        #endregion

        // dependency二进制描述文件中记录的是
        // 依赖个数 资源依赖文件个数 资源依赖文件id(第一个为资源本身id)
        #region 读取资源依赖信息
        {
            ResourceDependencyDic.Clear();
            MemoryStream dependencyMemoryStream = new MemoryStream(dependencyBytes);
            BinaryReader dependencyBinaryReader = new BinaryReader(dependencyMemoryStream);
            //获取依赖链个数
            ushort dependencyCount = dependencyBinaryReader.ReadUInt16();
            for (int i = 0; i < dependencyCount; i++)
            {
                //获取资源个数
                ushort resourceCount = dependencyBinaryReader.ReadUInt16();
                // 读取资源本身id
                ushort assetId = dependencyBinaryReader.ReadUInt16();
                string assetUrl = assetUrlDic[assetId];
                List<string> dependencyList = new List<string>(resourceCount);
                // 读取剩下依赖文件id，加入 资源本身路径 - 依赖文件路径 映射
                for (int ii = 1; ii < resourceCount; ii++)
                {
                    ushort dependencyAssetId = dependencyBinaryReader.ReadUInt16();
                    string dependencyUrl = assetUrlDic[dependencyAssetId];
                    dependencyList.Add(dependencyUrl);
                }

                ResourceDependencyDic.Add(assetUrl, dependencyList);
            }
        }
        #endregion
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="url">资源Url</param>
    /// <param name="async">是否异步</param>
    /// <returns> Task<AResource> </returns>
    public IResource Load(string url, bool async)
    {
        return LoadInternal(url, async, false);
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="url">资源Url</param>
    /// <param name="async">是否异步</param>
    /// <param name="callback">异步加载完成回调</param>
    public void LoadWithCallback(string url, bool async, Action<IResource> callback)
    {
        AResource resource = LoadInternal(url, async, false);
        if (resource.done)
        {
            callback?.Invoke(resource);
        }
        else
        {
            resource.finishedCallback += callback;
        }
    }

    /// <summary>
    /// 短路径到全路径的映射缓存
    /// </summary>
    private Dictionary<string, string> shortPathMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 获取全路径并附带拓展名
    /// 例如 "Prefabs/Weapon/Rock" -> "Assets/.../Prefabs/Weapon/Rock.prefab"
    /// </summary>
    public string GetFullUrl(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        if (shortPathMapping.TryGetValue(path, out string fullUrl))
        {
            return fullUrl;
        }

        string searchTarget = path.Replace("\\", "/");
        if (!searchTarget.StartsWith("/"))
        {
            searchTarget = "/" + searchTarget;
        }

        foreach (var url in ResourceBunldeDic.Keys)
        {
            int lastDotIdx = url.LastIndexOf('.');
            string urlWithoutExt = lastDotIdx != -1 ? url.Substring(0, lastDotIdx) : url;

            if (urlWithoutExt.EndsWith(searchTarget, StringComparison.OrdinalIgnoreCase))
            {
                shortPathMapping[path] = url;
                return url;
            }
        }

        shortPathMapping[path] = path;
        return path;
    }

    /// <summary>
    /// 兼容旧版同步加载
    /// 临时采用加载后永不回收的策略兜底，只进行 Load 而不主动 Unload
    /// </summary>
    public T LoadResources<T>(string path) where T : UnityEngine.Object
    {
        string fullUrl = GetFullUrl(path);
        IResource resource = Load(fullUrl, false);
        if (resource != null)
        {
            return resource.GetAsset<T>();
        }
        return null;
    }

    /// <summary>
    /// 兼容旧版异步加载
    /// </summary>
    public void LoadResourcesAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object
    {
        string fullUrl = GetFullUrl(path);
        LoadWithCallback(fullUrl, true, res =>
        {
            if (res != null)
            {
                callback?.Invoke(res.GetAsset<T>());
            }
            else
            {
                callback?.Invoke(null);
            }
        });
    }

    /// <summary>
    /// 内部加载资源
    /// </summary>
    /// <param name="url">资源url</param>
    /// <param name="async">是否异步</param>
    /// <param name="dependency">是否依赖</param>
    /// <returns></returns>
    private AResource LoadInternal(string url, bool async, bool dependency)
    {
        AResource resource = null;
        // 如果是已经加载出来的资源，直接返回
        if (resourceDic.TryGetValue(url, out resource))
        {
            //从需要释放的列表中移除
            if (resource.reference == 0)
            {
                needUnloadList.Remove(resource);
            }
            // 引用数自增
            resource.AddReference();

            return resource;
        }

        // 直接加载
        if (isEditor)
        {
            resource = new EditorResource();
        }
        // 异步加载
        else if (async)
        {
            ResourceAsync resourceAsync = new ResourceAsync();
            // 添加到异步资源集合中
            asyncList.Add(resourceAsync);
            resource = resourceAsync;
        }
        // 同步非编辑器加载
        else
        {
            resource = new Resource();
        }

        resource.url = url;
        resourceDic.Add(url, resource);

        //加载依赖
        List<string> dependencies = null;
        ResourceDependencyDic.TryGetValue(url, out dependencies);
        // 如果有依赖
        if (dependencies != null && dependencies.Count > 0)
        {
            // 加入成员对象
            resource.dependencies = new AResource[dependencies.Count];
            // 递归加载依赖，不会出现相互依赖无限递归，否则打包时就会出错
            for (int i = 0; i < dependencies.Count; i++)
            {
                string dependencyUrl = dependencies[i];
                AResource dependencyResource = LoadInternal(dependencyUrl, async, true);
                resource.dependencies[i] = dependencyResource;
            }
        }

        // 引用次数自增
        resource.AddReference();
        resource.Load();

        return resource;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="resource"></param>
    public void Unload(IResource resource)
    {
        if (resource == null)
        {
            throw new ArgumentException($"{nameof(ResourceManager)}.{nameof(Unload)}() {nameof(resource)} is null.");
        }

        AResource aResource = resource as AResource;
        aResource.ReduceReference();

        if (aResource.reference == 0)
        {
            WillUnload(aResource);
        }
    }

    /// <summary>
    /// 添加即将要释放的资源，添加到待卸载列表中
    /// </summary>
    /// <param name="aResource"></param>
    private void WillUnload(AResource aResource)
    {
        needUnloadList.AddLast(aResource);
    }

    public void Update()
    {
        BundleManager.Instance.Update();

        for (int i = 0; i < asyncList.Count; i++)
        {
            AResourceAsync resourceAsync = asyncList[i];
            if (resourceAsync.Update())
            {
                asyncList.RemoveAt(i);
                i--;
            }
        }
    }

    public void LateUpdate()
    {
        if (needUnloadList.Count != 0)
        {
            while (needUnloadList.Count > 0)
            {
                AResource resource = needUnloadList.First.Value;
                // 移除AResource，AResource没有引用计数会变成垃圾自动被清理
                needUnloadList.RemoveFirst();
                if (resource == null)
                    continue;


                resourceDic.Remove(resource.url);

                resource.UnLoad();

                //依赖引用-1
                if (resource.dependencies != null)
                {
                    for (int i = 0; i < resource.dependencies.Length; i++)
                    {
                        AResource temp = resource.dependencies[i];
                        Unload(temp);
                    }
                }
            }
        }

        BundleManager.Instance.LateUpdate();
    }
}
