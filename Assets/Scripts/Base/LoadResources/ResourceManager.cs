using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourcesManager : BaseManager<ResourcesManager>
{
    // 缓存已经加载完成的资源对象
    private Dictionary<string, Object> resCache = new Dictionary<string, Object>();

    // 缓存正在进行中的异步加载请求，处理高并发下对同一资源的重复请求
    private Dictionary<string, ResourceRequest> loadingRequests = new Dictionary<string, ResourceRequest>();

    public void LoadResourcesAsync<T>(string pathName, UnityAction<T> callback) where T : Object
    {
        // 1. 当资源已经加载完毕，直接从缓存获取并调用回调
        if (resCache.TryGetValue(pathName, out var res))
        {
            callback?.Invoke(res as T);
            return;
        }

        // 2. 高并发环境下同时加载一个资源时
        // 如果资源正在加载中，将回调委托追加到当前请求的 completed 事件中，避免重复发起加载
        if (loadingRequests.TryGetValue(pathName, out var request))
        {
            request.completed += (req) =>
            {
                callback?.Invoke((req as ResourceRequest).asset as T);
            };
            return;
        }

        // 3. 开启全新的异步加载
        ResourceRequest newRequest = Resources.LoadAsync<T>(pathName);
        loadingRequests.Add(pathName, newRequest);

        newRequest.completed += (req) =>
        {
            // 加载完成，从"加载中"字典移除
            loadingRequests.Remove(pathName);

            if (newRequest.asset != null)
            {
                // 存入"已完成"字典
                if (!resCache.ContainsKey(pathName))
                {
                    resCache.Add(pathName, newRequest.asset);
                }
                callback?.Invoke(newRequest.asset as T);
            }
            else
            {
                Debug.LogError("Resources LoadAsync Error, 找不到资源: " + pathName);
                callback?.Invoke(null);
            }
        };
    }

    public T LoadResources<T>(string pathName) where T : Object
    {
        // 1. 如果缓存中已有，直接返回
        if (resCache.TryGetValue(pathName, out var res))
        {
            return res as T;
        }

        // 注意：Resources API 不支持像 Addressables 那样将异步 Request 强制转换为阻塞等待 (WaitForCompletion)。
        // 如果遇到一边正在异步加载，同帧又要求同步加载的极端情况，最稳妥的做法是直接执行同步加载并覆盖。
        T asset = Resources.Load<T>(pathName);

        if (asset != null)
        {
            if (!resCache.ContainsKey(pathName))
            {
                resCache.Add(pathName, asset);
            }
            return asset;
        }
        else
        {
            Debug.LogError("Resources Load Error, 找不到资源: " + pathName);
            return null;
        }
    }

    public void Unload(string pathName)
    {
        //if (resCache.TryGetValue(pathName, out var asset))
        //{
        //    // 核心差异：Resources.UnloadAsset 只能卸载非 GameObject/Component 类型的资源（如 AudioClip, Texture2D, Material 等）。
        //    // 对于 GameObject（如 Prefab），必须使用 Destroy() 销毁实例，并在合适时机调用 Resources.UnloadUnusedAssets() 清理内存。
        //    if (!(asset is GameObject || asset is Component))
        //    {
        //        Resources.UnloadAsset(asset);
        //    }
        //    resCache.Remove(pathName);
        //}
    }

    public void UnloadAll()
    {
        //foreach (var asset in resCache.Values)
        //{
        //    if (!(asset is GameObject || asset is Component))
        //    {
        //        Resources.UnloadAsset(asset);
        //    }
        //}
        //resCache.Clear();
        //// 建议在场景切换等大内存变动时，在外部手动调用一次 Resources.UnloadUnusedAssets();
    }
}