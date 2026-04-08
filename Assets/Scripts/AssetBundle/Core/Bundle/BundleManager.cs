using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class BundleManager
{
    public static BundleManager Instance { get; private set; } = new BundleManager();

    /// <summary>
    /// 加密偏移
    /// </summary>
    internal ulong offset;
    /// <summary>
    /// 获取资源真实路径回调
    /// </summary>
    private Func<string, string> getFileCallback;
    /// <summary>
    /// bundle依赖管理信息
    /// </summary>
    private AssetBundleManifest assetBundleManifest;

    /// <summary>
    /// 所有已加载的bundle
    /// </summary>
    private Dictionary<string, ABundle> bundleDic = new Dictionary<string, ABundle>();
    
    //异步创建的bundle加载时候需要先保存到该列表
    private List<ABundleAsync> asyncList = new List<ABundleAsync>();
    
    /// <summary>
    /// 需要释放的bundle
    /// </summary>
    private LinkedList<ABundle> needUnloadList = new LinkedList<ABundle>();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="platform">平台</param>
    /// <param name="getFileCallback">获取资源真实路径回调</param>
    /// <param name="offset">加载bundle偏移</param>
    internal void Initialize(string platform, Func<string, string> getFileCallback, 
        ulong offset)
    {
        this.getFileCallback = getFileCallback;
        this.offset = offset;

        // 获取manifest文件 路径：AssetBundle/Window/Window
        string assetBundleManifestFile = getFileCallback.Invoke(platform);

        AssetBundle manifestAssetBundle = AssetBundle.LoadFromFile(
            assetBundleManifestFile);
        // 读取所有Assets文件
        UnityEngine.Object[] objs = manifestAssetBundle.LoadAllAssets();
        // 如果没有读取到，抛异常
        if (objs.Length == 0)
        {
            throw new Exception($"{nameof(BundleManager)}.{nameof(Initialize)}() AssetBundleManifest load fail.");
        }

        assetBundleManifest = objs[0] as AssetBundleManifest;
    }

    /// <summary>
    /// 获取bundle的绝对路径
    /// </summary>
    /// <param name="url"></param>
    /// <returns>bundle的绝对路径</returns>
    internal string GetFileUrl(string url)
    {
        if (getFileCallback == null)
        {
            throw new Exception($"{nameof(BundleManager)}.{nameof(GetFileUrl)}() {nameof(getFileCallback)} is null.");
        }

        //交到外部处理
        return getFileCallback.Invoke(url);
    }

    /// <summary>
    /// 同步加载bundle
    /// </summary>
    /// <param name="url">asset路径</param>
    internal ABundle Load(string url)
    {
        return LoadInternal(url, false);
    }

    /// <summary>
    /// 异步加载bundle
    /// </summary>
    /// <param name="url">asset路径</param>
    internal ABundle LoadAsync(string url)
    {
        return LoadInternal(url, true);
    }

    /// <summary>
    /// 引用自减，引用为零卸载bundle
    /// </summary>
    /// <param name="bundle">要卸载的bundle</param>
    internal void UnLoad(ABundle bundle)
    {
        if (bundle == null)
            throw new ArgumentException($"{nameof(BundleManager)}.{nameof(UnLoad)}() bundle is null.");

        //引用-1
        bundle.ReduceReference();

        //引用为0,直接释放
        if (bundle.reference == 0)
        {
            WillUnload(bundle);
        }
    }

    /// <summary>
    /// 即将要释放的资源，添加到NeedUnloadList中，在下一个LateUpdate中集中释放
    /// </summary>
    /// <param name="resource"></param>
    private void WillUnload(ABundle bundle)
    {
        needUnloadList.AddLast(bundle);
    }

    /// <summary>
    /// 内部加载bundle
    /// </summary>
    /// <param name="url">asset路径</param>
    /// <param name="async">是否异步</param>
    /// <returns>bundle对象</returns>
    private ABundle LoadInternal(string url, bool async)
    {
        ABundle bundle;
        if (bundleDic.TryGetValue(url, out bundle))
        {
            if (bundle.reference == 0)
            {
                needUnloadList.Remove(bundle);
            }

            //从缓存中取并引用+1
            bundle.AddReference();

            return bundle;
        }

        //创建ab
        if (async)
        {
            bundle = new BundleAsync();
            bundle.url = url;
            asyncList.Add(bundle as ABundleAsync);
        }
        else
        {
            bundle = new Bundle();
            bundle.url = url;
        }

        bundleDic.Add(url, bundle);

        // 通过manifest加载依赖
        string[] dependencies = assetBundleManifest.GetDirectDependencies(url);
        if (dependencies.Length > 0)
        {
            bundle.dependencies = new ABundle[dependencies.Length];
            // 递归添加依赖Bundle
            for (int i = 0; i < dependencies.Length; i++)
            {
                string dependencyUrl = dependencies[i];
                ABundle dependencyBundle = LoadInternal(dependencyUrl, async);
                bundle.dependencies[i] = dependencyBundle;
            }
        }

        bundle.AddReference();

        bundle.Load();

        return bundle;
    }

    /// <summary>
    /// 帧驱动所有异步加载包，直到包加载完毕，从asyncList中移除
    /// </summary>
    public void Update()
    {
        for (int i = 0; i < asyncList.Count; i++)
        {
            if (asyncList[i].Update())
            {
                asyncList.RemoveAt(i);
                // for循环中移除元素，注意下标修改
                i--;
            }
        }
    }

    public void LateUpdate()
    {
        if (needUnloadList.Count == 0)
            return;

        // 卸载需要卸载的AB包
        while (needUnloadList.Count > 0)
        {
            ABundle bundle = needUnloadList.First.Value;
            needUnloadList.RemoveFirst();

            if (bundle == null)
                continue;

            bundleDic.Remove(bundle.url);
            // 如果bundle是异步的并且没有加载完毕
            if (!bundle.done && bundle is BundleAsync)
            {
                BundleAsync bundleAsync = bundle as BundleAsync;
                // 从异步列表中移除
                if (asyncList.Contains(bundleAsync))
                    asyncList.Remove(bundleAsync);
            }

            bundle.UnLoad();

            // 依赖包引用自减
            if (bundle.dependencies != null)
            {
                for (int i = 0; i < bundle.dependencies.Length; i++)
                {
                    ABundle temp = bundle.dependencies[i];
                    // 依赖包引用自减
                    UnLoad(temp);
                }
            }
        }
    }

    // 获取当前已经加载的所有Bundle
    public IEnumerable<ABundle> GetAllLoadedBundles()
    {
        return bundleDic.Values;
    }

    // 获取正在加载的异步Bundle队列
    public IEnumerable<ABundleAsync> GetAsyncBundles()
    {
        return asyncList;
    }
}
