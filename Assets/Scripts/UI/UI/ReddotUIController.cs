using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReddotUIController : MonoBehaviour
{
    public string reddotPath;

    private ReddotUI reddotUI;
    private bool isInit = false;

    void Start()
    {
        InitReddotUI();
    }

    private void InitReddotUI()
    {
        if (isInit) return;

        // 实例化并获取组件
        reddotUI = Instantiate(ResourcesManager.Instance.LoadResources<GameObject>(
            DataManager.REDDOTUI), transform, false).GetComponent<ReddotUI>();

        isInit = true;

        if (!string.IsNullOrEmpty(reddotPath))
        {
            // 向管理器注册监听，当节点值变化时，自动调用 UpdateValue
            ReddotManager.Instance.AddListener(reddotPath, reddotUI.UpdateValue);
            // 主动拉取当前的值，解决实例化晚于事件派发的问题
            reddotUI.UpdateValue(ReddotManager.Instance.GetValue(reddotPath));
        }
        else
        {
            reddotUI.UpdateValue(0);
        }
    }

    /// <summary>
    /// 设置监听红点路径
    /// 如何当前Controller已经初始化，从ReddotManager中移除旧的监听事件
    /// 更新监听路径
    /// 如果没有初始化，进行初始化，添加监听事件，调用获取当前节点值更新到UI中
    /// </summary>
    /// <param name="path"></param>
    public void SetReddotPath(string path)
    {
        if (isInit && !string.IsNullOrEmpty(reddotPath))
        {
            ReddotManager.Instance.RemoveListener(reddotPath, reddotUI.UpdateValue);
        }

        reddotPath = path;

        if (!isInit)
        {
            InitReddotUI();
        }
        else if (!string.IsNullOrEmpty(reddotPath))
        {
            ReddotManager.Instance.AddListener(reddotPath, reddotUI.UpdateValue);
            reddotUI.UpdateValue(ReddotManager.Instance.GetValue(reddotPath));
        }
    }

    void OnDestroy()
    {
        // 必须注销监听，防止内存泄漏和空指针报错
        if (reddotUI != null && !string.IsNullOrEmpty(reddotPath))
        {
            ReddotManager.Instance.RemoveListener(reddotPath, reddotUI.UpdateValue);
        }
    }
}
