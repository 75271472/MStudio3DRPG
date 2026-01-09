using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum EUILayer
{
    Bot,
    Mid,
    Top,
    System
}

public enum ECanvasType
{
    Overlay,
    World,
}

public class UIManager : BaseManager<UIManager>
{
    // 面板存储字典 面板名 - 面板脚本
    public Dictionary<string, BasePanel> panelDic = 
        new Dictionary<string, BasePanel>();

    private Transform bot;
    private Transform mid;
    private Transform top;
    private Transform system;

    // 记录Canvas对象，方便外部获取
    public RectTransform OverlayCanvas { get; private set; }
    public RectTransform WorldCanvas { get; private set; }
    public EventSystem EventSystem { get; private set; }

    /// <summary>
    /// 面板构造函数，场景中创建Canvas和EventSystem对象，并设置其过场景不移除
    /// 寻找Canvas中的Bot、Mid、Top、System层级子对象
    /// </summary>
    public UIManager()
    {
        OverlayCanvasInit();
        WorldCanvasInit();
        EventSystemInit();

        LayerInit();
    }

    private void OverlayCanvasInit()
    {
        GameObject obj = GameObject.Instantiate(ResourceManager.Instance.
            Load<GameObject>(DataManager.OVERLAYCANVAS));
        OverlayCanvas = obj.transform as RectTransform;
        GameObject.DontDestroyOnLoad(obj);
    }

    private void LayerInit()
    {
        if (OverlayCanvas == null) return;
        
        bot = OverlayCanvas.Find("Bot");
        mid = OverlayCanvas.Find("Mid");
        top = OverlayCanvas.Find("Top");
        system = OverlayCanvas.Find("System");
    }

    private void WorldCanvasInit()
    {
        GameObject obj = GameObject.Instantiate(ResourceManager.Instance.
            Load<GameObject>(DataManager.WORLDCANVAS));
        WorldCanvas = obj.transform as RectTransform;
        WorldCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
        WorldCanvas.GetComponent<Canvas>().planeDistance = 7.5f;
        GameObject.DontDestroyOnLoad(obj);
    }

    private void EventSystemInit()
    {
        // 查找场景中多余的EventSystem并删除
        foreach (var es in GameObject.FindObjectsByType<EventSystem>(
            FindObjectsSortMode.InstanceID))
        {
            GameObject.Destroy(es.gameObject);
        }

        GameObject obj = GameObject.Instantiate(ResourceManager.Instance.
            Load<GameObject>(DataManager.EVENTSYSTEM));
        EventSystem = obj.GetComponent<EventSystem>();

        GameObject.DontDestroyOnLoad(obj);
    }

    /// <summary>
    /// 获取层级Transform
    /// </summary>
    /// <param name="layer">传入层级枚举</param>
    /// <returns></returns>
    public Transform GetLayer(EUILayer layer)
    {
        switch (layer) 
        {
            case EUILayer.Bot: return bot;
            case EUILayer.Mid: return mid;
            case EUILayer.Top: return top;
            case EUILayer.System: return system;
            default: return null;
        }
    }

    /**
    /// <summary>
    /// 异步面板加载，从Resources文件夹中创建面板到场景中的方法
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <param name="layer">实例化面板</param>
    /// <param name="callBack">实例化面板后调用的回调函数</param>
    public void ShowPanelAsync<T>(EUILayer layer = EUILayer.Mid, 
        UnityAction<T> callBack = null) where T : BasePanel
    {
        string panelName = typeof(T).Name;
        // 根据传入枚举，设置父对象层级
        Transform father = GetLayer(layer);

        // 当前字典中含有该面板，设置父对象，调用显示方法并执行回调函数
        if (panelDic.ContainsKey(panelName))
        {
            panelDic[panelName].transform.SetParent(father);
            panelDic[panelName].ShowMe();
            callBack?.Invoke(panelDic[panelName] as T);
            return;
        }

        // 字典中没有该面板，异步加载资源，设置父对象，设置相对位置与缩放，
        // 重置偏移量，执行panel脚本ShowMe方法，执行回调函数，添加到面板字典中
        LoadResourceManager.Instance.LoadResourcesAsync<GameObject>(
            DataManager.PANELROOTPATH + panelName, (obj) =>
        {
            obj.transform.SetParent(father);

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            (obj.transform as RectTransform).offsetMax = Vector3.zero;
            (obj.transform as RectTransform).offsetMin = Vector3.zero;

            T panel = obj.GetComponent<T>();
            panel.ShowMe();
            callBack?.Invoke(panel);

            panelDic.Add(panelName, panel);
        });
    }
    **/

    /// <summary>
    /// 同步面板加载
    /// </summary>
    /// <typeparam name="T">面板泛型</typeparam>
    /// <param name="layer">设定层级</param>
    /// <returns></returns>
    public T ShowPanel<T>(EUILayer layer = EUILayer.Mid) where T : BasePanel
    {
        return ShowOverlayPanel<T>(layer);
    }

    private T ShowOverlayPanel<T>(EUILayer layer) where T : BasePanel
    {
        string panelName = typeof(T).Name;
        // 根据传入枚举，设置父对象层级
        Transform father = GetLayer(layer);

        // 当前字典中含有该面板，设置父对象，调用显示方法并执行回调函数
        if (panelDic.ContainsKey(panelName))
        {
            panelDic[panelName].transform.SetParent(father);
            panelDic[panelName].ShowMe();
            return panelDic[panelName] as T;
        }

        // 字典中没有该面板，设置父对象，设置相对位置与缩放，
        // 重置偏移量，执行panel脚本ShowMe方法，执行回调函数，添加到面板字典中
        T panel = GameObject.Instantiate(ResourceManager.Instance.Load<GameObject>(
            DataManager.PANELROOTPATH + panelName).GetComponent<T>());
        panel.transform.SetParent(father);

        panel.transform.localPosition = Vector3.zero;
        panel.transform.localScale = Vector3.one;
        (panel.transform as RectTransform).offsetMax = Vector3.zero;
        (panel.transform as RectTransform).offsetMin = Vector3.zero;

        panel.ShowMe();
        panelDic.Add(panelName, panel);
        return panel;
    }

    /// <summary>
    /// 从场景中删除面板方法
    /// </summary>
    public void HidePanel<T>()
    {
        string panelName = typeof(T).Name;
        // 调用面板隐藏方法，销毁对象，从字典中移除
        if (panelDic.ContainsKey(panelName))
        {
            panelDic[panelName].HideMe();
            GameObject.Destroy(panelDic[panelName].gameObject);
            panelDic.Remove(panelName);
        }
    }

    public void HidePanelAll()
    {
        foreach (var panel in panelDic.Values)
        {
            panel.HideMe();
            GameObject.Destroy(panel.gameObject);
        }

        panelDic.Clear();
    }

    /// <summary>
    /// 获取面板中已存储的面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <returns></returns>
    public T GetPanel<T>() where T : BasePanel
    {
        string name = typeof(T).Name;
        if (panelDic.ContainsKey(name))
            return panelDic[name] as T;
        return null;
    }

    /// <summary>
    /// 为控件添加自定义事件
    /// </summary>
    /// <param name="control">控件</param>
    /// <param name="type">事件类型</param>
    /// <param name="callBack">回调函数</param>
    public static void AddControlListener(UIBehaviour control, 
        EventTriggerType type, UnityAction<BaseEventData> callBack)
    {
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = control.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callBack);
        trigger.triggers.Add( entry );
    }
}
