using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PoolData
{

    private GameObject fatherObj;

    public List<GameObject> gameObjects = new List<GameObject>();

    public int Count => gameObjects.Count;

    public PoolData(GameObject obj, GameObject poolObj)
    {
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.SetParent(poolObj.transform, false);
        PushObj(obj);
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
        return obj;
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
        GameObject obj;

        if (poolDic.ContainsKey(name) && poolDic[name].Count > 0)
        {
            obj = poolDic[name].PullObj();
        }
        // 缓存池中没有对象时，异步加载对象
        else
        {
            // 使用ResourcesManager的资源同步加载
            obj = GameObject.Instantiate(ResourcesManager.Instance.LoadResources
                <GameObject>(name));
        }

        return obj;
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

        if (poolDic.ContainsKey(name))
        {
            poolDic[name].PushObj(obj);
        }
        else
        {
            poolDic.Add(name, new PoolData(obj, poolObj));
        }
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void Clear()
    {
        if (poolObj != null)
        {
            GameObject.Destroy(poolObj);
            poolObj = null;
        }

        if (poolDic != null)
        {
            poolDic.Clear();
        }
    }
}