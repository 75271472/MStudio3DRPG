using System.IO;
using System.Xml;
using System.Xml.Serialization;

public static class XmlUtility
{
    /// <summary>
    /// 根据文件路径读取为T类型对象，读取失败返回默认值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName">文件路径</param>
    /// <returns></returns>
    public static T Read<T>(string fileName) where T : class
    {
        FileStream stream = null;
        if (!File.Exists(fileName))
        {
            return default(T);
        }
        // 不阻塞线程
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            stream = File.OpenRead(fileName);
            XmlReader reader = XmlReader.Create(stream);
            T instance = (T)serializer.Deserialize(reader);
            stream.Close();
            return instance;
        }
        catch
        {
            if (stream != null)
                stream.Close();
            return default(T);
        }
    }
}
