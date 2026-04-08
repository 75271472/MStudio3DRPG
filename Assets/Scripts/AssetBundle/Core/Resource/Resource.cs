using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using Object = UnityEngine.Object;

public class Resource : AResource
{
    public override bool keepWaiting => !done;

    /// <summary>
    /// 从ResourcesManager中获取资源所属Bunde路径
    /// 根据Bundle路径从BundleManager中加载所属包及所依赖包
    /// BundleManager中会调用Bundle.Load，将包加载到内存
    /// </summary>
    internal override void Load()
    {
        // 如果资源路径为空
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException($"{nameof(Resource)}.{nameof(Load)}() {nameof(url)} is null.");

        // 如果bundle为空
        if (bundle != null)
            throw new Exception($"{nameof(Resource)}.{nameof(Load)}() {nameof(bundle)} not null.");

        // 无法从ResourcesManager的BundleDic中取出url
        string bundleUrl = null;
        if (!ResourcesManager.Instance.ResourceBunldeDic.TryGetValue(url, out bundleUrl))
            throw new Exception($"{nameof(Resource)}.{nameof(Load)}() {nameof(bundleUrl)} is null.");

        // 每有一个资源被加载，资源所属包引用自增
        bundle = BundleManager.Instance.Load(bundleUrl);
        LoadAsset();
    }

    /// <summary>
    /// 调用资源所属Bundle的LoadAsset获取具体资源
    /// </summary>
    /// <exception cref="Exception"></exception>
    internal override void LoadAsset()
    {
        if (bundle == null)
            throw new Exception($"{nameof(Resource)}.{nameof(LoadAsset)}() {nameof(bundle)} is null.");

        //正在异步加载的资源要变成同步
        FreshAsyncAsset();

        // 场景资源不能调用AssetBundle.LoadAsset，
        // 直接使用SceneManager.LoadScene("场景名")加载场景
        if (!bundle.isStreamedSceneAssetBundle)
            asset = bundle.LoadAsset(url, typeof(Object));

        done = true;

        if (finishedCallback != null)
        {
            Action<AResource> tempCallback = finishedCallback;
            finishedCallback = null;
            tempCallback.Invoke(this);
        }
    }

    internal override void UnLoad()
    {
        if (bundle == null)
            throw new Exception($"{nameof(Resource)}.{nameof(UnLoad)}() {nameof(bundle)} is null.");

        if (asset != null && !(asset is GameObject))
        {
            Resources.UnloadAsset(asset);
            asset = null;
        }

        // 包引用自减
        BundleManager.Instance.UnLoad(bundle);

        bundle = null;
        finishedCallback = null;
    }

    public override T GetAsset<T>()
    {
        Object tempAsset = asset;
        Type type = typeof(T);

        if (type == typeof(Sprite))
        {
            if (asset is Sprite)
            {
                return tempAsset as T;
            }
            // 资源加载：asset = bundle.LoadAsset(url, typeof(Object));
            // Unity加载图片时，它的主资源 (Main Asset) 是 Texture2D
            // 但如果在编辑器里把这张图片设置成了 Sprite (2D and UI)
            // Unity 会在这张 Texture2D 下面生成一个关联的 子资源，类型才是 Sprite
            // 因此对于Sprite类型资源，首先判断是否为Sprite类型，否则大概率是主资源的Texture2D类型
            // 此时需要进行资源卸载，并使用Sprite重新进行具体资源加载
            else
            {
                if (tempAsset && !(tempAsset is GameObject))
                {
                    Resources.UnloadAsset(tempAsset);
                }

                asset = bundle.LoadAsset(url, type);
                return asset as T;
            }
        }
        else
        {
            return tempAsset as T;
        }
    }
}
