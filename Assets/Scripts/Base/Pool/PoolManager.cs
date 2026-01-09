using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// �����������
/// </summary>
public class PoolData
{
    // �������ݿն���
    private GameObject fatherObj;
    // �����Ļ���ض����б�
    public List<GameObject> gameObjects = new List<GameObject>();
    // �б��ж����������
    public int Count => gameObjects.Count;

    /// <summary>
    /// �����๹�췽��
    /// </summary>
    /// <param name="obj">��һ�����뻺��صĶ���</param>
    /// <param name="poolObj">����ؿն���</param>
    public PoolData(GameObject obj, GameObject poolObj)
    {
        // �����������ݿն������ø�����Ϊ����ؿն���
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.SetParent(poolObj.transform, false);
        // ������Ķ�����뻺���
        PushObj(obj);
    }

    /// <summary>
    /// ������󣬷��������Դ�������
    /// </summary>
    /// <param name="obj"></param>
    public void PushObj(GameObject obj)
    {
        // ���ø�����Ϊ���ݿն���ʧ����뻺���
        obj.transform.SetParent(fatherObj.transform, false);
        obj.SetActive(false);
        gameObjects.Add(obj);
    }

    /// <summary>
    /// �Ӹ�����Դ�������ȡ������
    /// </summary>
    /// <returns></returns>
    public GameObject PullObj()
    {
        GameObject obj = null;
        // ������ʿ��б�
        if (gameObjects.Count > 0)
        {
            // ȡ����һ����Դ����ȡ�����������ã��ӻ�������Ƴ�
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
    // ͨ���ֵ���������
    // name��ʾ���������Դ�ļ���·����List��ʾ��ͬ����·����Դ�б�
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();

    /**
    /// <summary>
    /// 从对象池中异步获取对象，如果对象池为空，则异步Load对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action">使用ResourcesManager异步加载对象，通过回调函数处理返回值</param>
    public void PullObjAsync(string name, UnityAction<GameObject> action)
    {
        if (poolDic.ContainsKey(name) && poolDic[name].Count > 0)
        {
            action?.Invoke(poolDic[name].PullObj());
        }
        // �������û�ж���ʱ���첽���ض���
        else
        {
            // ʹ��ResourcesManager����Դ�첽���أ����������Դ��ɿ���
            LoadResourceManager.Instance.LoadResourcesAsync<GameObject>(name, (o) =>
            {
                o.name = name;
                action?.Invoke(o);
            });
        }
    }
    **/

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
            obj = GameObject.Instantiate(ResourceManager.Instance.Load
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
            // ��һ���򻺴���з��������Դʱ
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