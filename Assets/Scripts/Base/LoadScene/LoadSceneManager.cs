using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;


public class LoadSceneManager : BaseManager<LoadSceneManager>
{
    public event Action OnPrepareLoadSceneEvent;
    // 保存当前场景的句柄，用于后续卸载 (Addressables 特有要求)
    private AsyncOperationHandle<SceneInstance> handle;

    ///// <summary>
    ///// 同步场景切换
    ///// </summary>
    ///// <param name="name">切换场景名</param>
    ///// <param name="action">切换完成执行事件</param>
    //public void LoadScene(string name, Action action)
    //{
    //    SceneManager.LoadScene(name);
    //    action.Invoke();
    //}

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

        // StartScene为游戏初始启动场景，存储在BuildSettings中，由原生SceneManager管理
        // 因此对于StartScene使用SceneManager进行加载
        // 同时意味着游戏启动场景不可热更新
        if (name == DataManager.STARTSCENE)
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync(name);

            while (!ao.isDone)
            {
                loadScenePanel.UpdateLoadSceneSlider(ao.progress);
                yield return null;
            }
        }
        // GameScene不存储在BuildSettings中，由Addressables管理
        // 使用Addressables进行加载
        // 可以热更新
        else
        {
            // 异步加载场景
            handle = Addressables.LoadSceneAsync(DataManager.SCENEROOTPATH + name,
                LoadSceneMode.Single);

            // 场景加载完成后执行MonoManager.Instance.Init，
            // 重新执行所有MonoManager的Init方法
            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Failed)
                {
                    Debug.LogError($"Scene Log Error {name}");
                }
            };

            // 当前未加载完成
            while (!handle.IsDone)
            {
                loadScenePanel.UpdateLoadSceneSlider(handle.PercentComplete);
                yield return null;
            }
        }

        loadScenePanel.UpdateLoadSceneSlider(1f);

        // 如果从Addressables场景切换到原生场景
        // 需要手动释放Addressbles资源
        if (name == DataManager.STARTSCENE)
        {
            // 清空所有AB包资源
            ResourceManager.Instance.Unload();
            // 手动Release handle
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        loadScenePanel.FadeOut(2.5f);
        // 所有MonoManager初始化完成后执行传入的action回调
        MonoManager.Instance.OnInitCompletedEvent += action;
        // Init中会有op.WaitForCompletion，不能在handle.Completed中执行
        // 因此放到协程结尾，当场景加载完毕后执行
        MonoManager.Instance.Init();
    }
}