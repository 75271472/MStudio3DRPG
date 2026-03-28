using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ABundle
{
    /// <summary>
    /// AssetBundle
    /// </summary>
    internal AssetBundle assetBundle { get; set; }

    /// <summary>
    /// 包中资源是否为场景资源，场景资源包中可以有多个场景资源，但不能有其他类型资源
    /// 场景资源包进入内存，直接使用SceneManager.LoadScene("场景名")加载场景
    /// 不能使用LoadAsset进行加载
    /// </summary>
    internal bool isStreamedSceneAssetBundle { get; set; }

    /// <summary>
    /// bundle url
    /// </summary>
    internal string url { get; set; }

    /// <summary>
    /// 引用计数器
    /// </summary>
    internal int reference { get; set; }

    //bundle是否加载完成
    internal bool done { get; set; }

    /// <summary>
    /// bundle依赖
    /// </summary>
    internal ABundle[] dependencies { get; set; }

    /// <summary>
    /// 加载bundle
    /// </summary>
    internal abstract void Load();

    /// <summary>
    /// 卸载bundle
    /// </summary>
    internal abstract void UnLoad();

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="type">资源Type</param>
    /// <returns>AssetBundleRequest</returns>
    internal abstract AssetBundleRequest LoadAssetAsync(string name, Type type);

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="type">资源Type</param>
    /// <returns>指定名字的资源</returns>
    internal abstract UnityEngine.Object LoadAsset(string name, Type type);

    /// <summary>
    /// 增加引用
    /// </summary>
    internal void AddReference()
    {
        //自己引用+1
        ++reference;
    }

    /// <summary>
    /// 减少引用
    /// </summary>
    internal void ReduceReference()
    {
        //自己引用-1
        --reference;

        if (reference < 0)
        {
            throw new Exception($"{GetType()}.{nameof(ReduceReference)}() less than 0,{nameof(url)}:{url}.");
        }
    }
}
