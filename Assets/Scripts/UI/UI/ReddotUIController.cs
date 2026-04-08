using UnityEngine;

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

        isInit = true;

        if (!string.IsNullOrEmpty(reddotPath))
        {
            // 向管理器注册监听，当节点值变化时，自动调用 OnReddotValueChanged
            ReddotManager.Instance.AddListener(reddotPath, OnReddotValueChanged);
            // 主动拉取当前的值，解决实例化晚于事件派发的问题
            OnReddotValueChanged(ReddotManager.Instance.GetValue(reddotPath));
        }
    }

    private void OnReddotValueChanged(int value)
    {
        if (value >= 1)
        {
            // 当值 >= 1 时，如果没有红点实例，则从池中获取
            if (reddotUI == null)
            {
                reddotUI = PoolManager.Instance.PullObj(DataManager.REDDOTUI).GetComponent<ReddotUI>();
                reddotUI.transform.SetParent(transform, false);
                reddotUI.transform.localScale = Vector3.one;
            }
            // 更新数值显示
            reddotUI.UpdateValue(value);
        }
        else
        {
            // 当等于 0 时，如果有红点实例，则将其放入对象池
            if (reddotUI != null)
            {
                PoolManager.Instance.PushObj(DataManager.REDDOTUI, reddotUI.gameObject);
                reddotUI = null;
            }
        }
    }

    /// <summary>
    /// 设置监听红点路径
    /// 如果当前Controller已经初始化，从ReddotManager中移除旧的监听事件
    /// 更新监听路径
    /// 如果没有初始化，进行初始化，添加监听事件，调用获取当前节点值更新到UI中
    /// </summary>
    /// <param name="path"></param>
    public void SetReddotPath(string path)
    {
        if (isInit && !string.IsNullOrEmpty(reddotPath))
        {
            ReddotManager.Instance.RemoveListener(reddotPath, OnReddotValueChanged);
        }

        reddotPath = path;

        if (!isInit)
        {
            InitReddotUI();
        }
        else if (!string.IsNullOrEmpty(reddotPath))
        {
            ReddotManager.Instance.AddListener(reddotPath, OnReddotValueChanged);
            OnReddotValueChanged(ReddotManager.Instance.GetValue(reddotPath));
        }
        else
        {
            // 如果传入的路径为空，强制使红点值归零，触发对象池回收
            OnReddotValueChanged(0);
        }
    }

    void OnDestroy()
    {
        // 必须注销监听，防止内存泄漏和空指针报错
        if (isInit && !string.IsNullOrEmpty(reddotPath))
        {
            ReddotManager.Instance.RemoveListener(reddotPath, OnReddotValueChanged);
        }

        // 如果在销毁时还持有对象，将其放回池中
        if (reddotUI != null)
        {
            PoolManager.Instance.PushObj(DataManager.REDDOTUI, reddotUI.gameObject);
            reddotUI = null;
        }
    }
}

