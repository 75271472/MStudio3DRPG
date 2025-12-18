using LitJson;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using static UnityEngine.Rendering.VolumeComponent;

/// <summary>
/// 序列化和反序列化Json时  使用的是哪种方案
/// </summary>
public enum EJsonType
{
    JsonUtlity,
    LitJson,
}

/// <summary>
/// Json数据管理类 主要用于进行 Json的序列化存储到硬盘 和 反序列化从硬盘中读取到内存中
/// </summary>
public class JsonManager : BaseManager<JsonManager>
{
    //存储Json数据 序列化
    public bool SaveData(object data, string fileName, 
        EJsonType type = EJsonType.LitJson)
    {
        //确定存储路径
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        // 获取目录路径
        string directory = Path.GetDirectoryName(path);

        // 检查目录是否存在，如果不存在则创建
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        //序列化 得到Json字符串
        switch (type)
        {
            case EJsonType.JsonUtlity:
                //jsonStr = JsonUtility.ToJson(data);
                return JsonUtilitySave(fileName, path, data);
            case EJsonType.LitJson:
                //jsonStr = JsonMapper.ToJson(data, JsonWriter);
                return JsonMapperSave(fileName, path, data);
            default:
                return false;
        }
        //把序列化的Json字符串 存储到指定路径的文件中
        //File.WriteAllText(path, jsonStr);

        //Debug.Log($"name: {fileName} " +
        //    $"path: {path}");

        //return true;
    }

    private bool JsonUtilitySave(string fileName, string path, object data)
    {
        string jsonStr = JsonUtility.ToJson(data);

        //把序列化的Json字符串 存储到指定路径的文件中
        File.WriteAllText(path, jsonStr);

        Debug.Log($"name: {fileName} " +
            $"path: {path}");

        return true;
    }

    // 使用JsonWriter让LitJons输出的Json文件更易读
    private bool JsonMapperSave(string fileName, string path, object data)
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter writer = new JsonWriter(sb);

        // 设置格式化选项
        writer.PrettyPrint = true;
        writer.IndentValue = 4;

        // 写入数据
        JsonMapper.ToJson(data, writer);

        // 保存到文件
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

        return true;
    }

    public T LoadData<T>(string fileName,
        EJsonType jsonType = EJsonType.LitJson) where T : new()
    {
        LoadData<T>(fileName, out var value, jsonType);
        return value;
    }

    //读取指定文件中的 Json数据 反序列化
    public bool LoadData<T>(string fileName, out T value,
        EJsonType jsonType = EJsonType.LitJson) 
        where T : new()
    {
        // 先在StreamingAsstes文件夹中查找
        if (!FindFromStreamingAssets(fileName, out var path))
        {
            value = default(T);
            return false;
        }

        // 再到PersistentData文件夹中查找
        if (!FindFromPersistentData(fileName, out path))
        {
            value = default(T);
            return false;
        }
            
        value = GetJsonStr<T>(path, jsonType);
        return true;
    }

    public T LoadDataFromStreamingAssets<T>(string fileName, 
        EJsonType jsonType = EJsonType.LitJson) where T : new()
    {
        LoadDataFromStreamingAssets<T>(fileName, out var value, jsonType);
        return value;
    }

    public T LoadDataFromPersistentData<T>(string fileName,
        EJsonType jsonType = EJsonType.LitJson) where T : new()
    {
        LoadDataFromPersistentData<T>(fileName, out var value, jsonType);
        return value;
    }

    public bool LoadDataFromStreamingAssets<T>(string fileName, out T value,
        EJsonType jsonType = EJsonType.LitJson) where T : new()
    {
        if (!FindFromStreamingAssets(fileName, out var path))
        {
            value = default(T);
            return false;
        }

        value = GetJsonStr<T>(path, jsonType);
        return true;
    }

    public bool LoadDataFromPersistentData<T>(string fileName, out T value,
    EJsonType jsonType = EJsonType.LitJson) where T : new()
    {
        if (!FindFromPersistentData(fileName, out var path))
        {
            value = default(T);
            return false;
        }

        value = GetJsonStr<T>(path, jsonType);
        return true;
    }
    
    public bool FindFromStreamingAssets(string fileName)
    {
        return FindFromStreamingAssets(fileName, out var path);
    }

    public bool FindFromPersistentData(string fileName)
    {
        return FindFromPersistentData(fileName, out var path);
    }

    private bool FindFromStreamingAssets(string fileName, out string path)
    {
        // 在StreamingAsstes文件夹中查找
        path = Application.streamingAssetsPath + "/" + fileName + ".json";
        return File.Exists(path);
    }

    private bool FindFromPersistentData(string fileName, out string path)
    {
        // 在PersistentDataPath中查找
        path = Application.persistentDataPath + "/" + fileName + ".json";
        return File.Exists(path);
    }

    private T GetJsonStr<T>(string path, EJsonType jsonType) where T : new()
    {
        //进行反序列化
        string jsonStr = File.ReadAllText(path);
        //数据对象
        T data = default(T);
        switch (jsonType)
        {
            case EJsonType.JsonUtlity:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case EJsonType.LitJson:
                data = JsonMapper.ToObject<T>(jsonStr);
                break;
        }

        //把对象返回出去
        return data;
    }
}
