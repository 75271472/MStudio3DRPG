using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LoadSceneManager : BaseManager<LoadSceneManager>
{
    public event Action OnPrepareLoadSceneEvent;

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
        ResourceManager.Instance.Unload();  
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
        LoadScenePanel loadScenePanel =
            UIManager.Instance.ShowPanel<LoadScenePanel>(EUILayer.System);
        yield return loadScenePanel.FadeInCoroutine(2.5f);

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