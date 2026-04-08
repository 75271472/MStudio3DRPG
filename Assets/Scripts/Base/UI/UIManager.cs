using System.Collections.Generic;
using Unity.VisualScripting;
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
    // 实例化的面板存储字典 (面板名 - 面板实例脚本)
    public Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();
    // 从AB中加载出来的预制体资源 (面板名 - IResource)
    private Dictionary<string, IResource> resDic = new Dictionary<string, IResource>();

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
        GameObject obj = GameObject.Instantiate(ResourcesManager.Instance.
            LoadResources<GameObject>(DataManager.OVERLAYCANVAS));
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
        GameObject obj = GameObject.Instantiate(ResourcesManager.Instance.
            LoadResources<GameObject>(DataManager.WORLDCANVAS));
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

        GameObject obj = GameObject.Instantiate(ResourcesManager.Instance.
            LoadResources<GameObject>(DataManager.EVENTSYSTEM));
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

        // 1. 如果当前字典中已经实例化过该面板，直接显示
        if (panelDic.TryGetValue(panelName, out BasePanel existingPanel))
        {
            existingPanel.transform.SetParent(father, false);
            existingPanel.ShowMe();
            return existingPanel as T;
        }

        // 2. 如果面板还没被实例化，检查 AB 资源是否已经读取到内存
        if (!resDic.TryGetValue(panelName, out IResource resource))
        {
            string fullUrl = ResourcesManager.Instance.GetFullUrl(DataManager.PANELROOTPATH + panelName);
            resource = ResourcesManager.Instance.Load(fullUrl, false);
            resDic.Add(panelName, resource);
        }

        // 3. 从内存的预制体资源中实例化对象
        T panel = GameObject.Instantiate(resource.GetAsset<GameObject>()).GetComponent<T>();
        panel.transform.SetParent(father, false);

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
        
        // 调用隐藏并销毁实例
        if (panelDic.ContainsKey(panelName))
        {
            panelDic[panelName].HideMe();
            GameObject.Destroy(panelDic[panelName].gameObject);
            panelDic.Remove(panelName);
        }

        // 从 AB 底层释放对预制件的引用并卸载
        if (resDic.ContainsKey(panelName))
        {
            ResourcesManager.Instance.Unload(resDic[panelName]);
            resDic.Remove(panelName);
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

        foreach (var resource in resDic.Values)
        {
            ResourcesManager.Instance.Unload(resource);
        }
        resDic.Clear();
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
        trigger.triggers.Add(entry);
    }
}
