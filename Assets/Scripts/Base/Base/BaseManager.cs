using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseManager<T> where T : new()
{
    private static T instance = new T();
    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = new T();
            return instance;
        }
    }

    // 懒汉模式单例对象启动方法
    // 供外界调用Instance实例化单例模式对象
    public void Start() { }
}
