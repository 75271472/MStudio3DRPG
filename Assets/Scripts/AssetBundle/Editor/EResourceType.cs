/// <summary>
/// 引用类型
/// </summary>
public enum EResourceType
{
    /// <summary>
    /// 主动配置，在打包设置中分析到的资源，会在Builder中主动检索
    /// </summary>
    Direct = 1,

    /// <summary>
    /// 依赖资源，Direct类型资源的依赖项，在Builder中通过依赖检查进行检索
    /// </summary>
    Dependency = 2,

    /// <summary>
    /// 生成的文件
    /// </summary>
    Ganerate = 3,
}