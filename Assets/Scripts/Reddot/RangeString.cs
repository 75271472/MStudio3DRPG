using System;
using Unity.VisualScripting;

// 继承IEquatable接口，而不是直接重写Object的Equals，能够直接与RangeString比较，而不是Object
public struct RangeString : IEquatable<RangeString>
{
    // 源字符串
    private string source;

    // 开始索引
    private int startIndex;

    // 结束索引
    private int endIndex; 

    // 长度
    private int length;

    // 源字符串是否为null或空字符串
    private bool isSourceNullOrEmpty;

    // 哈希码
    private int hashCode;

    public RangeString(string source, int startIndex, int endIndex)
    {
        this.source = source;
        this.startIndex = startIndex;
        this.endIndex = endIndex;
        this.length = endIndex - startIndex + 1;
        isSourceNullOrEmpty = string.IsNullOrEmpty(source);
        hashCode = 0;
    }

    public bool Equals(RangeString other) 
    {
        bool isOtherNullOrEmpty = string.IsNullOrEmpty(other.source);

        // 均为空
        if (isOtherNullOrEmpty && isSourceNullOrEmpty) return true;
        // 其中一个为空
        if (isOtherNullOrEmpty || isSourceNullOrEmpty) return false;
        // 长度不等
        if (other.length != length) return false;
        for (int i = startIndex, j = other.startIndex; i < endIndex; i++, j++)
        {
            if (source[i] != other.source[j]) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        if (hashCode == 0 && !isSourceNullOrEmpty)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                hashCode = hashCode * 31 + source[i];
            }
        }

        return hashCode;
    }

    // 所有RangeString使用ReddotManager中的一个StringBuilder对象作为ToString缓冲
    public override string ToString()
    {
        ReddotManager.Instance.CachedSb.Clear();
        for (int i = startIndex; i <= endIndex; i++)
        {
            ReddotManager.Instance.CachedSb.Append(source[i]);
        }
        string str = ReddotManager.Instance.CachedSb.ToString();

        return str;
    }
}
