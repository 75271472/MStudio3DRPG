using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

public class BundleAsync : ABundleAsync
{
    /// <summary>
    /// 异步bundle的AssetBundleCreateRequest
    /// </summary>
    private AssetBundleCreateRequest assetBundleCreateRequest;

    /// <summary>
    /// 获取Request
    /// </summary>
    internal override void Load()
    {
        if (assetBundleCreateRequest != null)
        {
            throw new Exception($"{nameof(BundleAsync)}.{nameof(Load)}() {nameof(assetBundleCreateRequest)} not null, {this}.");
        }

        string file = BundleManager.Instance.GetFileUrl(url);

#if UNITY_EDITOR || UNITY_STANDALONE
        if (!File.Exists(file))
        {
            throw new FileNotFoundException($"{nameof(BundleAsync)}.{nameof(Load)}() {nameof(file)} not exist, file:{file}.");
        }
#endif

        assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(
            file, 0, BundleManager.Instance.offset);
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="type">资源Type</param>
    /// <returns>指定名字的资源</returns>
    internal override Object LoadAsset(string name, Type type)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"{nameof(BundleAsync)}.{nameof(LoadAsset)}() name is null.");

        if (assetBundleCreateRequest == null)
            throw new NullReferenceException($"{nameof(BundleAsync)}.{nameof(LoadAsset)}() m_AssetBundleCreateRequest is null.");

        if (assetBundle == null)
            assetBundle = assetBundleCreateRequest.assetBundle;

        return assetBundle.LoadAsset(name, type);
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="type">资源Type</param>
    /// <returns>AssetBundleRequest</returns>
    internal override AssetBundleRequest LoadAssetAsync(string name, Type type)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"{nameof(BundleAsync)}.{nameof(LoadAssetAsync)}() name is null.");

        if (assetBundleCreateRequest == null)
            throw new NullReferenceException($"{nameof(BundleAsync)}.{nameof(LoadAssetAsync)}() m_AssetBundleCreateRequest is null.");

        if (assetBundle == null)
            assetBundle = assetBundleCreateRequest.assetBundle;

        return assetBundle.LoadAssetAsync(name, type);
    }

    /// <summary>
    /// 卸载bundle
    /// </summary>
    internal override void UnLoad()
    {
        // 卸载已经加载的AB包
        if (assetBundle)
        {
            // 一同卸载所有已经实例化的资源
            assetBundle.Unload(true);
        }
        else
        {
            //正在异步加载的资源也要切到主线程进行释放
            if (assetBundleCreateRequest != null)
            {
                assetBundle = assetBundleCreateRequest.assetBundle;
            }

            if (assetBundle)
            {
                assetBundle.Unload(true);
            }
        }

        assetBundleCreateRequest = null;
        done = false;
        reference = 0;
        assetBundle = null;
        isStreamedSceneAssetBundle = false;
    }

    /// <summary>
    /// 帧驱动，包和依赖包均加载完毕返回true
    /// </summary>
    /// <returns></returns>
    internal override bool Update()
    {
        // 如果已经判断过加载完毕，直接返回真
        if (done)
            return true;

        // 如果当前AB包有依赖，并且依赖中有没有加载完的包，返回false
        if (dependencies != null)
        {
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (!dependencies[i].done)
                    return false;
            }
        }

        // 如果当前包没加载完，返回false
        if (!assetBundleCreateRequest.isDone)
            return false;
        // 当前包和所有依赖包都加载完了，置真
        done = true;
  
        assetBundle = assetBundleCreateRequest.assetBundle;

        isStreamedSceneAssetBundle = assetBundle.isStreamedSceneAssetBundle;

        if (reference == 0)
        {
            UnLoad();
        }

        return true;
    }
}
