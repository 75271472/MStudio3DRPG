using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum ETriggerTiming
{
    Awake,
    Start,
    Update,
    Destroy,
}

public class MonoManager : MonoBehaviourManager<MonoManager>
{
    [field: SerializeField] public bool IsDebug { get; private set; }

    public event Action OnInitCompletedEvent;

    [SerializeField] private List<MonoBehaviourBase> monoList = 
        new List<MonoBehaviourBase>();
    private MonoController monoController;

    private void Awake()
    {
        //print("Awake");
        Init();
    }

    /// <summary>
    /// 构造方法中创建游戏对象并挂载monoController
    /// </summary>
    public override void Init()
    {
        base.Init();

        bool isStartScene =
            SceneManager.GetActiveScene().name == DataManager.STARTSCENE;
        SetManagerIsNotInit(isStartScene);
        // 如果是开始场景，则失活所有继承MonoBaseManager的Manager
        SetManagerActive(!isStartScene);
        // UIManager启动方法，UIManager属于懒汉模式的单例对象
        // 不调用Instance不实例化，不会生成EventSystem对象
        UIManager.Instance.Start();

        if (monoController == null)
        {
            monoController = gameObject.AddComponent<MonoController>();
        }

        // 必须手动添加Manager，并考虑不同Manager间的初始化顺序
        foreach (var mono in monoList)
        {
            if (!mono.Equals(this))
            {
                mono.Init();
            }
        }

        // 先执行完所有子类的Init，在判断IsNotInstance
        // 让子类对自己是否为Instance进行判断
        if (IsNotSubManagerInit) return;

        OnInitCompletedEvent?.Invoke();
        OnInitCompletedEvent = null;
    }

    /// <summary>
    /// 添加事件监听方法封装
    /// </summary>
    /// <param name="action"></param>
    public void AddEventListener(UnityAction action, ETriggerTiming triggerTiming)
    {
        switch (triggerTiming)
        {
            case ETriggerTiming.Awake:
                monoController.AddAwakeEventListener(action);
                break;
            case ETriggerTiming.Update:
                monoController.AddUpdateEventListener(action);
                break; 
            case ETriggerTiming.Start:
                monoController.AddStartEventListener(action);
                break;
            case ETriggerTiming.Destroy:
                monoController.AddDestroyEventListener(action);
                break;
        }
        
    }

    /// <summary>
    /// 移除事件监听方法封装
    /// </summary>
    /// <param name="action"></param>
    public void RemoveEventListener(UnityAction action, ETriggerTiming triggerTiming)
    {
        switch (triggerTiming)
        {
            case ETriggerTiming.Awake:
                monoController.RemoveAwakeEventListener(action);
                break;
            case ETriggerTiming.Update:
                monoController.RemoveUpdateEventListener(action);
                break;
            case ETriggerTiming.Start:
                monoController.RemoveStartEventListener(action);
                break;
            case ETriggerTiming.Destroy:
                monoController.RemoveDestroyEventListener(action);
                break;
        }
    }

    private void SetManagerIsNotInit(bool isNotInit)
    {
        foreach (var mono in monoList)
        {
            if (!mono.Equals(this))
            {
                mono.IsNotSubManagerInit = isNotInit;
            }
        }
    }

    private void SetManagerActive(bool isActive)
    {
        foreach (var mono in monoList)
        {
            if (!mono.Equals(this))
            {
                mono.gameObject.SetActive(isActive);
            }
        }
    }
}
