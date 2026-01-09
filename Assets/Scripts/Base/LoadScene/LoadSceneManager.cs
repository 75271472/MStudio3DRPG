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

    //private IEnumerator LoadSceneAnsycCoroutine(string name, Action action)
    //{
    //    LoadScenePanel loadScenePanel =
    //        UIManager.Instance.ShowPanel<LoadScenePanel>(EUILayer.System);
    //    yield return loadScenePanel.FadeInCoroutine(2.5f);

    //    // 异步加载场景
    //    AsyncOperation ao = SceneManager.LoadSceneAsync(name);
    //    // 防止场景加载完自动跳转（可选，为了更好的控制进度条表现，通常建议设为false，最后手动设为true）
    //    ao.allowSceneActivation = false;

    //    // 注册回调（注意：这里有潜在的时序风险，见下文"潜在风险提示"）
    //    ao.completed += (a) => MonoManager.Instance.Init();
    //    ao.completed += (a) => loadScenePanel.FadeOut(2.5f);
    //    MonoManager.Instance.OnInitCompletedEvent += action;

    //    // --- 循环部分修改 ---
    //    float targetProgress = 0f;

    //    // 当 allowSceneActivation = false 时，progress 只能达到 0.9
    //    // 当 allowSceneActivation = true (默认) 时，isDone 会在加载完变为 true
    //    while (!ao.isDone)
    //    {
    //        // 优化进度条显示：ao.progress 最大只能到 0.9
    //        // 如果 allowSceneActivation 为 false，需要在这里判断是否到达 0.9 然后手动开启跳转
    //        if (ao.progress >= 0.9f)
    //        {
    //            targetProgress = 1.0f;
    //            ao.allowSceneActivation = true; // 允许跳转
    //        }
    //        else
    //        {
    //            targetProgress = ao.progress;
    //        }

    //        loadScenePanel.UpdateLoadSceneSlider(targetProgress);

    //        // 【关键修改】这里必须 yield return null，确保每帧只执行一次循环体
    //        yield return null;
    //    }

    //    // 循环结束，确保进度条跑满
    //    loadScenePanel.UpdateLoadSceneSlider(1.0f);
    //}
}