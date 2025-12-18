using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LoadResourceManager : BaseManager<LoadResourceManager>
{
    // 使用prefab GameObject预设体对象加载GameObject对象
    public GameObject LoadGameObjectByPrefab(GameObject prefab)
    {
        return GameObject.Instantiate(prefab);
    }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="resPath">资源路径</param>
    /// <returns></returns>
    public T LoadResources<T>(string resPath) where T : Object // Object时Resources.Load可加载的全部资源的父类
    {
        T obj = Resources.Load<T>(resPath);
        if (obj is GameObject)
            // 当资源为GameObject类型时直接实例化
            return GameObject.Instantiate(obj);
        else
            return obj;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="resPath">资源路径</param>
    /// <param name="action">加载后执行的回调函数</param>
    public void LoadResourcesAsync<T>(string resPath, UnityAction<T> action) where T : Object
    {
        MonoManager.Instance.StartCoroutine(LoadResourcesAsyncIE(resPath, action));
    }

    /// <summary>
    /// 异步加载资源协程
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resPath"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    private IEnumerator LoadResourcesAsyncIE<T>(string resPath, UnityAction<T> action) where T : Object
    {
        // 为异步加载资源添加完成事件
        ResourceRequest resourceRequest = Resources.LoadAsync<T>(resPath);
        resourceRequest.completed += (a) => {
            if ( resourceRequest.asset is GameObject )
            {
                action?.Invoke(GameObject.Instantiate(resourceRequest.asset) as T);
            }
            else
            {
                action?.Invoke(resourceRequest.asset as T);
            }};
        yield return resourceRequest;
    }
}
