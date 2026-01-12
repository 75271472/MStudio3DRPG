using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking; // 引入网络库
using UnityEngine.ResourceManagement.AsyncOperations;

public class HotUpdateManager : BaseManager<HotUpdateManager>
{
    public IEnumerator HotUpdateCoroutine(UnityAction callback)
    {
        // 1. 初始化
        var init = Addressables.InitializeAsync();
        yield return init;

        // 2. 检查更新
        var check = Addressables.CheckForCatalogUpdates(false);
        yield return check;

        if (check.Status == AsyncOperationStatus.Succeeded && check.Result.Count > 0)
        {
            // 3. 发现更新，下载新目录
            Debug.Log($"发现 {check.Result.Count} 个更新，开始下载 Catalog...");
            var update = Addressables.UpdateCatalogs(check.Result, false);
            yield return update;
            Addressables.Release(update);
        }

        Addressables.Release(check);

        // 4. 一切搞定，进入游戏
        callback?.Invoke();
    }
}