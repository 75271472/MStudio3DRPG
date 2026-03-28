using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using Object = UnityEngine.Object;

public class ResourceAsync : AResourceAsync
{
    public override bool keepWaiting => !done;

    /// <summary>
    /// 异步加载资源的AssetBundleRequest
    /// </summary>
    private AssetBundleRequest assetBundleRequest;

    internal override void Load()
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException($"{nameof(Resource)}.{nameof(Load)}() {nameof(url)} is null.");

        if (bundle != null)
            throw new Exception($"{nameof(Resource)}.{nameof(Load)}() {nameof(bundle)} not null.");

        string bundleUrl = null;
        if (!ResourcesManager.Instance.ResourceBunldeDic.TryGetValue(url, out bundleUrl))
            throw new Exception($"{nameof(ResourceAsync)}.{nameof(Load)}() {nameof(bundleUrl)} is null.");

        bundle = BundleManager.Instance.LoadAsync(bundleUrl);
    }

    internal override void LoadAsset()
    {
        if (bundle == null)
            throw new Exception($"{nameof(ResourceAsync)}.{nameof(LoadAsset)}() {nameof(bundle)} is null.");

        // 场景资源不能调用AssetBundle.LoadAsset，
        // 直接使用SceneManager.LoadScene("场景名")加载场景
        if (!bundle.isStreamedSceneAssetBundle)
        {
            if (assetBundleRequest != null)
            {
                asset = assetBundleRequest.asset;
            }
            else
            {
                asset = bundle.LoadAsset(url, typeof(Object));
            }
        }

        done = true;

        if (finishedCallback != null)
        {
            // finishedCallback.Invoke(this);
            // finishedCallback = null;
            // 避免在调用后置空，如果在finishedCallback中又调用了这个资源对象
            // 并设置finishedCallback，由于是异步操作，
            // 当前逻辑调用完Invoke后会直接置空finishedCallback
            // 而资源对象后被设置的finishedCallback会被直接摸出，导致逻辑无法执行
            // 使用临时变量先置空后调用，本次回调逻辑清空并且不会影响Invoke中下次回调执行
            Action<AResource> tempCallback = finishedCallback;
            finishedCallback = null;
            tempCallback.Invoke(this);
        }
    }

    /// <summary>
    /// 异步加载资源，调用bundle.LoadAssetAsync异步获取Request
    /// </summary>
    /// <exception cref="Exception"></exception>
    internal override void LoadAssetAsync()
    {
        if (bundle == null)
            throw new Exception($"{nameof(ResourceAsync)}.{nameof(LoadAssetAsync)}() {nameof(bundle)} is null.");

        assetBundleRequest = bundle.LoadAssetAsync(url, typeof(Object));
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
            else
            {
                if (tempAsset && !(tempAsset is GameObject))
                {
                    // Resources.UnloadAsset只能卸载非实例化的资源，
                    // 不能卸载GameObject 预制体、Component 组件
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

    internal override void UnLoad()
    {
        if (bundle == null)
            throw new Exception($"{nameof(Resource)}.{nameof(UnLoad)}() {nameof(bundle)} is null.");

        // 资源不为空并且类型为GameObject
        if (base.asset != null && !(base.asset is GameObject))
        {
            // Resources.UnloadAsset只能卸载非实例化的资源，
            // 不能卸载GameObject 预制体、Component 组件
            Resources.UnloadAsset(base.asset);
            asset = null;
        }

        assetBundleRequest = null;
        // 对于GameObject类型资源，在包卸载时才会被卸载
        BundleManager.Instance.UnLoad(bundle);
        bundle = null;
        finishedCallback = null;
    }

    public override bool Update()
    {
        if (done)
            return true;

        // 依赖检查
        if (dependencies != null)
        {
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (!dependencies[i].done)
                    return false;
            }
        }

        if (!bundle.done)
            return false;

        if (assetBundleRequest == null)
        {
            LoadAssetAsync();
        }

        // 如果请求不为空并且请求没完成，返回false
        if (assetBundleRequest != null && !assetBundleRequest.isDone)
            return false;

        LoadAsset();

        return true;
    }
}
