using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : BaseManager<ResourceManager>
{
    private Dictionary<string, AsyncOperationHandle> handleMap = 
        new Dictionary<string, AsyncOperationHandle>();
    public T Load<T>(string addressName) where T : Object
    {
        // 打包运行状态下
        //#else
        if (handleMap.ContainsKey(addressName))
        {
            AsyncOperationHandle handle = handleMap[addressName];
            if (handle.IsDone)
            {
                return handle.Result as T;
            }

            return handle.WaitForCompletion() as T;
        }

        var op = Addressables.LoadAssetAsync<T>(addressName);

        T result = op.WaitForCompletion();

        if (op.Status == AsyncOperationStatus.Succeeded)
        {
            handleMap.Add(addressName, op);
            return result;
        }
        else
        {
            Debug.LogError($"Addressables 同步加载失败: {addressName}");
            // 失败了通常需要释放句柄，防止内存泄露
            Addressables.Release(op);
            return null;
        }
    }

    public void Unload(string addressName)
    {
        if (handleMap.ContainsKey(addressName))
        {
            Addressables.Release(handleMap[addressName]);
            handleMap.Remove(addressName);
        }
    }

    public void Unload()
    {
        foreach (var key in handleMap.Keys)
        {
            Addressables.Release(handleMap[key]);
        }

        handleMap.Clear();
    }
}
