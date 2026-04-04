/// <summary>
/// 控制ab粒度
/// </summary>
public enum EBundleType
{
    /// <summary>
    /// 以文件作为ab名字（最小粒度），1个文件 = 1个AB包
    /// </summary>
    File,

    /// <summary>
    /// 将资源文件所在的文件夹作为作为一个AB包，同文件夹下的所有文件位于同一AB包
    /// 子目录中文件打入子目录AB包中
    /// </summary>
    Directory,

    /// <summary>
    /// 直接将BuildItem中的assetPath作为AB包，目录内的所有资源位于同一AB包中
    /// </summary>
    All
}
