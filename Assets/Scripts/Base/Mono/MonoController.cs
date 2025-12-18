using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoController : MonoBehaviour
{
    // 没有继承Mono类在Update函数中执行事件
    private UnityAction updateEvent;
    private UnityAction awakeEvent;
    private UnityAction startEvent;
    private UnityAction destroyEvent;

    //private void Awake()
    //{
    //    // 过场景不移除，保持游戏对象唯一性，进而保证脚本唯一性
    //    DontDestroyOnLoad(gameObject);
    //}

    void Update()
    {
        updateEvent?.Invoke();
    }

    public void AddAwakeEventListener(UnityAction action)
    {
        awakeEvent += action;
    }

    public void RemoveAwakeEventListener(UnityAction action)
    {
        awakeEvent -= action;
    }

    public void AddStartEventListener(UnityAction action)
    {
        startEvent += action;
    }

    public void RemoveStartEventListener(UnityAction action)
    {
        startEvent -= action;
    }

    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="action"></param>
    public void AddUpdateEventListener(UnityAction action)
    {
        updateEvent += action;
    }

    /// <summary>
    /// 移出事件
    /// </summary>
    /// <param name="action"></param>
    public void RemoveUpdateEventListener(UnityAction action)
    {
        updateEvent -= action;
    }

    public void AddDestroyEventListener(UnityAction action)
    {
        destroyEvent += action;
    }

    public void RemoveDestroyEventListener(UnityAction action)
    {
        destroyEvent -= action;
    }

    private void OnDestroy()
    {
        awakeEvent = null;
        startEvent = null;
        updateEvent = null;
    }
}
