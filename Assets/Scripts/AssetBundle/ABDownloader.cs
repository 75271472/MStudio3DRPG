using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// AB包下载器
/// </summary>
public class ABDownloader
{

    /// <summary>
    /// 当前下载器下载的AB包数据
    /// </summary>
    private ABPackInfo _obj_ABDecs;

    /// <summary>
    /// 是否资源下载中
    /// </summary>
    private bool _bIsDownloading = false;
    public bool bIsDownloading { get => _bIsDownloading; }

    /// <summary>
    /// 获取当前下载资源的大小
    /// </summary>
    /// <returns></returns>
    public float GetDownloadResSize()
    {
        if (_obj_ABDecs != null)
        {
            return _obj_ABDecs.nSize;
        }

        return 0;
    }

    /// <summary>
    /// 下载AB包
    /// </summary>
    /// <param name="obj_ABDecs">AB包资源的说明，包含文件名，大小，md5值</param>
    /// <returns></returns>
    public IEnumerator DownloadABPack(ABPackInfo obj_ABDecs)
    {
        _bIsDownloading = true;
        string sDownloadUrl = HotUpdateManager._sBaseUrl + @"/" + obj_ABDecs.sABName;
        Debug.Log("下载资源：" + sDownloadUrl);
        UnityWebRequest uObj_web = UnityWebRequest.Get(sDownloadUrl);
        yield return uObj_web.SendWebRequest();

        if (uObj_web.result == UnityWebRequest.Result.ConnectionError || uObj_web.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("获取AB包 " + sDownloadUrl + " 错误: " + uObj_web.error);
            HotUpdateManager.Instance.DispatchUpdateError();
            yield break;
        }
        else
        {
            string sABPath = Application.persistentDataPath + @"/" + obj_ABDecs.sABName;
            Debug.Log("AB包 保存本地路径是：" + sABPath);

            IOUtils.CreateDirectroryOfFile(sABPath);

            if (!File.Exists(sABPath))
            {
                File.Create(sABPath).Dispose();
            }
            File.WriteAllBytes(sABPath, uObj_web.downloadHandler.data);

            // 下载完成后，更新本地版本数据
            // 每下载完一个包，立刻调用 UpdateClientABInfo 更新本地的 ABVersionFile.txt
            // 这样哪怕玩家中途断网或闪退，下次进来也不用重新下载已经下好的包。
            HotUpdateManager.Instance.UpdateClientABInfo(obj_ABDecs);
            _bIsDownloading = false;
            HotUpdateManager.Instance.ChangeDownloadNextABPack(this);
        }
    }
}
