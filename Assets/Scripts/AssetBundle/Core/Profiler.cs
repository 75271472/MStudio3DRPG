using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Profiler
{
    private static readonly Stopwatch stopwatch = new Stopwatch();
    private static readonly StringBuilder stringBuilder = new StringBuilder();
    private static readonly List<Profiler> stack = new List<Profiler>();

    private List<Profiler> childrenList;
    private string name;
    // Profiler层级，根Profiler层级为0，子Profiler层级为父Profiler层级+1，用于输出Profiler信息时的缩进
    private int level;
    private long time;
    // 开始时间戳
    private long stampTime;
    // Profiler执行的次数，Start和Stop方法成对调用，调用一次Start和Stop方法Count加1
    private int count;

    public Profiler(string name)
    {
        childrenList = null;
        this.name = name;
        level = 0;
        stampTime = -1;
        time = 0;
        count = 0;
    }

    private Profiler(string name, int level) : this(name)
    {
        this.level = level;
    }

    public Profiler CreateChild(string name)
    {
        if (childrenList == null)
        {
            childrenList = new List<Profiler>();
        }

        Profiler child = new Profiler(name, level + 1);
        childrenList.Add(child);
        return child;
    }

    // 在Start和Stop方法之间执行操作，获取操作执行时间
    public void Start()
    {
        if (stampTime != -1)
        {
            //throw new Exception($"{nameof(Profiler)} {nameof(Start)} 发生重入: {name}");
            Debug.LogWarning($"{nameof(Profiler)} {nameof(Start)} 发生重入: {name}");
        }
        // 记录开始时间戳，直接强制覆盖脏数据
        stampTime = stopwatch.ElapsedTicks;
    }

    public void Stop()
    {
        if (stampTime == -1)
        {
            throw new Exception($"{nameof(Profiler)} {nameof(Stop)} error {name}");
        }

        // 计算时间差并累加到总时间
        time += stopwatch.ElapsedTicks - stampTime;
        count++;
        // 重置时间戳
        stampTime = -1;
    }

//打包完成
//Builder[Count: 1, Time: 1092.66毫秒    1.09秒    0.0182分]
//|--LoadSetting[Count: 1, Time: 91.93毫秒    0.09秒    0.0015分]
//|--SwitchPlatform[Count: 1, Time: 0.10毫秒    0.00秒    0.0000分]
//|--Collect[Count: 1, Time: 83.85毫秒    0.08秒    0.0014分]
//|  |--CollectBuildSettingFile[Count: 1, Time: 3.29毫秒    0.00秒    0.0001分]
//|  |--CollectDependency[Count: 1, Time: 49.68毫秒    0.05秒    0.0008分]
//|  |--CollectBundle[Count: 1, Time: 1.33毫秒    0.00秒    0.0000分]
//|  |--GenerateManifest[Count: 1, Time: 28.05毫秒    0.03秒    0.0005分]
//|--BuildBundle[Count: 1, Time: 576.73毫秒    0.58秒    0.0096分]
//|--ClearAssetBundle[Count: 1, Time: 7.36毫秒    0.01秒    0.0001分]
//|--BuildManifest[Count: 1, Time: 332.57毫秒    0.33秒    0.0055分]
//UnityEngine.Debug:Log (object)
//AssetBundleFramework.Builder:Build () (at Assets/AssetBundleFramework/Editor/Builder.cs:216)
//AssetBundleFramework.Builder:BuildWindows() (at Assets/AssetBundleFramework/Editor/Builder.cs:119)
    private void Format()
    {
        stringBuilder.AppendLine();

        for (int i = 0; i < level; ++i)
        {
            stringBuilder.Append(i < level - 1 ? "|  " : "|--");
        }

        stringBuilder.Append(name);

        if (count <= 0)
        {
            return;
        }

        stringBuilder.Append(" [");
        stringBuilder.Append("Count");
        stringBuilder.Append(": ");
        stringBuilder.Append(count);
        stringBuilder.Append(", ");
        stringBuilder.Append("Time");
        stringBuilder.Append(": ");
        
        stringBuilder.Append($"{(float)time / TimeSpan.TicksPerMillisecond:F2}");
        stringBuilder.Append("毫秒    ");
        
        stringBuilder.Append($"{(float)time / TimeSpan.TicksPerSecond:F2}");
        stringBuilder.Append("秒    ");
        
        stringBuilder.Append($"{(float)time / TimeSpan.TicksPerMinute:F4}");
        stringBuilder.Append("分");
        
        stringBuilder.Append("]");
    }

    public override string ToString()
    {
        stringBuilder.Clear();
        stack.Clear();
        stack.Add(this);

        // 使用栈通过迭代方式遍历Child树，打印Child Profiler信息 
        while (stack.Count > 0)
        {
            // 只操作栈顶元素，出栈
            Profiler profiler = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            profiler.Format();

            if (profiler.childrenList != null)
            {
                // 倒序入栈，保证输出Child信息时是正序
                for (int i = profiler.childrenList.Count - 1; i >= 0; i--)
                {
                    // 子Profiler入栈，后续会被处理
                    stack.Add(profiler.childrenList[i]);
                }
            }
        }

        return stringBuilder.ToString();
    }
}
