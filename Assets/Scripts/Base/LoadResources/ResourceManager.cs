using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : BaseManager<ResourceManager>
{
    private const string ROOTPATH = "Assets/GameRes/";
    private Dictionary<string, Object> objectMap = new Dictionary<string, Object>();

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="resPath">资源路径</param>
    /// <returns></returns>
    public T Load<T>(string resPath) where T : Object // Object时Resources.Load可加载的全部资源的父类
    {

        //当前处于编辑器模式下
#if UNITY_EDITOR
        if (objectMap.ContainsKey(resPath)) return objectMap[resPath] as T;

        string extension = "";

        if (typeof(T) == typeof(GameObject))
        {
            extension = ".prefab";
        }
        else if (typeof(T) == typeof(Sprite) || typeof(T) == typeof(Texture2D))
        {
            // 图片可能是 png 也可能是 jpg，这里稍微麻烦点，通常你的项目里 png 居多
            extension = ".png";
        }
        else if (typeof(T) == typeof(ScriptableObject)) // 你的 .asset 数据文件
        {
            extension = ".asset";
        }
        else if (typeof(T) == typeof(AudioClip))
        {
            extension = ".mp3"; // 或者 .wav
        }

        string fullPath = ROOTPATH + resPath + extension;

        T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(fullPath);

        if (asset == null)
        {
            Debug.LogError($"ResManager.Load Error :{fullPath}");
        }
        else
        {
            Debug.Log($"ResManager.Load :{fullPath}");
        }

        objectMap.Add(resPath, asset);
        return asset;

        // 打包运行状态下
#else
        return null;
#endif
    }

    public void Unload()
    {
        objectMap.Clear();
        // 触发GC，清除内存中没有被引用的数据
        Resources.UnloadUnusedAssets();
    }

    /**
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
    **/
}
