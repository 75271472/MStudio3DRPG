//里氏转换原则  来避免装箱拆箱
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public interface IEventInfo { }//空接口

public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}

public class EventInfo : IEventInfo
{
    public UnityAction actions;

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

/// <summary>
/// 事件中心
/// </summary>
public class EventCenter : BaseManager<EventCenter>
{
    /// <summary>
    /// key：事件的名字
    /// vslue: 对应的是监听该事件的委托方法(父类装子类)
    /// </summary>
    private Dictionary<Events, IEventInfo> eventDic = new Dictionary<Events, IEventInfo>();

    /// <summary>
    /// 监听事件(带泛型参数)
    /// </summary>
    /// <param name="events">事件的名字</param>
    /// <param name="action">用来处理该事件的方法</param>
    public void EventListenner<T>(Events events, UnityAction<T> action)
    {
        //有没有对应的事件监听
        //有
        if (eventDic.ContainsKey(events))
        {
            //委托 一对多
            (eventDic[events] as EventInfo<T>).actions += action;
        }
        else//没有
        {
            eventDic[events] = new EventInfo<T>(action);
        }
    }

    /// <summary>
    /// 监听事件不带参数
    /// </summary>
    /// <param name="events"></param>
    /// <param name="action"></param>
    public void EventListenner(Events events, UnityAction action)
    {
        //有没有对应的事件监听
        //有
        if (eventDic.ContainsKey(events))
        {
            //委托 一对多
            (eventDic[events] as EventInfo).actions += action;
        }
        else//没有
        {
            eventDic[events] = new EventInfo(action);
        }
    }

    /// <summary>
    /// 事件触发（带泛型参数）
    /// </summary>
    /// <param name="events">那个名字的事件触发了</param>
    public void EventTrigger<T>(Events events, T info)
    {
        if (eventDic.ContainsKey(events))
        {
            // eventDic[events]?.Invoke(info);
            if ((eventDic[events] as EventInfo<T>).actions != null)
                (eventDic[events] as EventInfo<T>).actions(info);//执行委托函数
        }
    }

    /// <summary>
    /// 事件触发（不带泛型参数）
    /// </summary>
    /// <param name="events"></param>
    public void EventTrigger(Events events)
    {
        if (eventDic.ContainsKey(events))
        {
            // eventDic[events]?.Invoke(info);
            if ((eventDic[events] as EventInfo).actions != null)
                (eventDic[events] as EventInfo).actions();//执行委托函数
        }
    }

    /// <summary>
    /// 移除对应事件(事件有加就有减 不然会出问题)
    /// </summary>
    /// <param name="events">事件的名字</param>
    /// <param name="action">对应之间添加的委托函数</param>
    public void RemoveEvent<T>(Events events, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(events))
        {
            (eventDic[events] as EventInfo<T>).actions -= action;
        }
    }

    /// <summary>
    /// 不带参数的
    /// </summary>
    /// <param name="events"></param>
    /// <param name="action"></param>
    public void RemoveEvent(Events events, UnityAction action)
    {
        if (eventDic.ContainsKey(events))
        {
            (eventDic[events] as EventInfo).actions -= action;
        }
    }
}
