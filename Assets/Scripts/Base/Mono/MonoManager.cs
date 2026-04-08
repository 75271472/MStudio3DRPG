using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum ETriggerTiming
{
    Update,
    LateUpdate,
}

public class MonoManager : MonoBehaviourManager<MonoManager>
{
    [field: SerializeField] public bool IsDebug { get; private set; }

    public event Action OnInitCompletedEvent;

    [SerializeField]
    private List<MonoBehaviourBase> monoList =
        new List<MonoBehaviourBase>();

    private event UnityAction updateEvent;
    private event UnityAction lateUpdateEvent;

    private void Awake()
    {
        ResetAllEvent();
        Init();
    }

    public override void Init()
    {
        base.Init();

        bool isStartScene =
            SceneManager.GetActiveScene().name == DataManager.STARTSCENE;
        SetManagerIsNotInit(isStartScene);
        // 如果是开始场景，则失活所有继承MonoBaseManager的Manager
        SetManagerActive(!isStartScene);

        // 开始场景，进行ResourcesManager的激活，读取Bundle文件
        if (isStartScene)
            ResourcesInit.Instance.Start();

        // UIManager启动方法，UIManager属于懒汉模式的单例对象
        // 不调用Instance不实例化，不会生成EventSystem对象
        UIManager.Instance.Start();

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

    private void Update()
    {
        updateEvent?.Invoke();
    }

    private void LateUpdate()
    {
        lateUpdateEvent?.Invoke();
    }

    public void AddEventListener(UnityAction action, ETriggerTiming triggerTiming)
    {
        switch (triggerTiming)
        {
            case ETriggerTiming.Update:
                updateEvent += action;
                break;
            case ETriggerTiming.LateUpdate:
                lateUpdateEvent += action;
                break;
        }
    }

    public void RemoveEventListener(UnityAction action, ETriggerTiming triggerTiming)
    {
        switch (triggerTiming)
        {
            case ETriggerTiming.Update:
                updateEvent -= action;
                break;
            case ETriggerTiming.LateUpdate:
                lateUpdateEvent -= action;
                break;
        }
    }

    private void ResetAllEvent()
    {
        updateEvent = null;
        lateUpdateEvent = null;
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

    public void DestroyAllManagers()
    {
        foreach (var mono in monoList)
        {
            if (!mono.Equals(this))
            {
                mono.DestroyManager();
            }
        }
    }
}
