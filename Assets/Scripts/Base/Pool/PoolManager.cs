using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PoolData
{
    private IResource resource;
    private GameObject fatherObj;
    public List<GameObject> gameObjects = new List<GameObject>();
    public int Count => gameObjects.Count;

    public PoolData(string name, GameObject poolObj)
    {
        fatherObj = new GameObject(name);
        fatherObj.transform.SetParent(poolObj.transform, false);
        // 使用load加载资源前务必使用GetFullUrl获取完整路径
        string fullUrl = ResourcesManager.Instance.GetFullUrl(name);
        // 使用 AB 框架的同步加载接口，此时该资源的 reference 引用计数 + 1
        resource = ResourcesManager.Instance.Load(fullUrl, false);
    }

    public void PushObj(GameObject obj)
    {
        obj.transform.SetParent(fatherObj.transform, false);
        obj.SetActive(false);
        gameObjects.Add(obj);
    }

    public GameObject PullObj()
    {
        GameObject obj = null;
        if (gameObjects.Count > 0)
        {
            obj = gameObjects[0];
            obj.transform.SetParent(null);
            obj.SetActive(true);
            gameObjects.RemoveAt(0);
        }
        else
        {
            // 缓存池为空时，通过持有的 IResource 实例化。
            obj = resource.Instantiate();
            obj.name = fatherObj.name;
        }
        return obj;
    }

    public void Clear()
    {
        // 销毁池子里所有缓存的闲置 GameObject
        foreach (var obj in gameObjects)
        {
            if (obj != null)
                GameObject.Destroy(obj);
        }
        gameObjects.Clear();

        // 销毁父节点
        if (fatherObj != null)
            GameObject.Destroy(fatherObj);

        // 3. 通知 AB 框架卸载资源
        // 此时该资源的 reference 引用计数 -1，如果归零，框架会在 LateUpdate 真正卸载包
        if (resource != null)
        {
            ResourcesManager.Instance.Unload(resource);
            resource = null;
        }
    }
}

public class PoolManager : BaseManager<PoolManager>
{
    private GameObject poolObj;
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();

    /// <summary>
    /// 从对象池中同步获取对象，如果对象池为空，则同步Load对象
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject PullObj(string name)
    {
        if (poolObj == null)
            poolObj = new GameObject("PoolObj");

        // 如果池子里还没有这个分类，先创建 PoolData
        // 创建的同时，PoolData 构造函数内部会把 AB 包资源 Load 进内存
        if (!poolDic.ContainsKey(name))
        {
            poolDic.Add(name, new PoolData(name, poolObj));
        }

        // 直接从 PoolData 中取即可，里面已经封装了不够就 Instantiate 的逻辑
        return poolDic[name].PullObj();
    }

    /// <summary>
    /// 向对象池中添加对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public void PushObj(string name, GameObject obj)
    {
        if (poolObj == null)
            poolObj = new GameObject("PoolObj");

        if (!poolDic.ContainsKey(name))
        {
            // 正常情况下 Push 时肯定已经有了，但防御性编程加上
            poolDic.Add(name, new PoolData(name, poolObj));
        }

        poolDic[name].PushObj(obj);
    }

    /// <summary>
    /// 清空对象池，只在切换场景，没有活跃GameObject时调用
    /// 否则UnloadPrefab，会使得活跃的Gameobject丢失材质变为粉色
    /// </summary>
    public void Clear()
    {
        // 遍历清理每一个 PoolData，触发底层的 AB 卸载
        foreach (var pool in poolDic.Values)
        {
            pool.Clear();
        }
        poolDic.Clear();


        if (poolObj != null)
        {
            GameObject.Destroy(poolObj);
            poolObj = null;
        }
    }

    /// <summary>
    /// 清理指定名字的对象池，释放单个模块的内存
    /// </summary>
    public void Clear(string name)
    {
        if (poolDic.TryGetValue(name, out PoolData pool))
        {
            pool.Clear();
            poolDic.Remove(name);
        }
    }
}