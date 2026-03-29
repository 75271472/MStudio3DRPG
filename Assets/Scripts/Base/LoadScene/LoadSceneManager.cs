using System;
using System.Collections;
using System.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LoadSceneManager : BaseManager<LoadSceneManager>
{
    public event Action OnPrepareLoadSceneEvent;

    // 持有当前正在游戏的场景资源包引用，用于切场景时的精准垃圾回收
    private IResource currentSceneResource;

    /// <summary>
    /// 同步场景切换
    /// </summary>
    /// <param name="name">切换场景名</param>
    /// <param name="action">切换完成执行事件</param>
    public void LoadScene(string name, Action action)
    {
        SceneManager.LoadScene(name);
        action.Invoke();
    }

    /// <summary>
    /// 异步场景切换
    /// </summary>
    /// <param name="name">切换场景名</param>
    /// <param name="action">切换完成执行事件</param>
    public void LoadSceneAsync(string name, Action action = null)
    {
        // 清空对象池
        PoolManager.Instance.Clear();
        UIManager.Instance.HidePanelAll();
        InputManager.Instance.ResetInputAction();

        // 当准备加载新场景前，强制通知底层剥离上一个驻留在内存里的旧场景包裹引用！
        if (currentSceneResource != null)
        {
            ResourcesManager.Instance.Unload(currentSceneResource);
            currentSceneResource = null;
            Debug.Log($"【LoadSceneManager】已成功向管理器申请卸载上一个场景所在的 AB 资源包引用。");
        }

        OnPrepareLoadSceneEvent?.Invoke();
        OnPrepareLoadSceneEvent = null;

        // 异步加载协程，分帧返回加载进度
        MonoManager.Instance.StartCoroutine(LoadSceneAnsycCoroutine(name, action));
    }

    /// <summary>
    /// 异步场景切换迭代器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    private IEnumerator LoadSceneAnsycCoroutine(string name, Action action)
    {
        Debug.Log("Load Scene Begin");

        LoadScenePanel loadScenePanel =
            UIManager.Instance.ShowPanel<LoadScenePanel>(EUILayer.System);
        yield return loadScenePanel.FadeInCoroutine(2.5f);

        // --- 先把装有该游戏场景的独立 AB 货箱接回内存 ---
        string fullUrl = ResourcesManager.Instance.GetFullUrl(DataManager.SCENEROOTPATH + name);
        currentSceneResource = ResourcesManager.Instance.Load(fullUrl, false);
        Debug.Log($"【LoadSceneManager】已于底层内存成功加载场景所在 AB 货箱：{fullUrl}");

        // --- 然后委托引擎去沙箱里把它拆开实体化 ---
        // 异步加载场景
        AsyncOperation ao = SceneManager.LoadSceneAsync(name);
        // 设置加载完成事件
        // ao.completed += (a) => action?.Invoke();

        // 场景加载完成后执行MonoManager.Instance.Init，
        // 重新执行所有MonoManager的Init方法
        ao.completed += (a) => MonoManager.Instance.Init();
        ao.completed += (a) => loadScenePanel.FadeOut(2.5f);
        // 所有MonoManager初始化完成后执行传入的action回调
        MonoManager.Instance.OnInitCompletedEvent += action;

        // 当前未加载完成
        while (!ao.isDone)
        {
            loadScenePanel.UpdateLoadSceneSlider(ao.progress);
            yield return ao.progress;
        }
        loadScenePanel.UpdateLoadSceneSlider(ao.progress);

        //yield return null;
        //MonoManager.Instance.Init();
    }
}