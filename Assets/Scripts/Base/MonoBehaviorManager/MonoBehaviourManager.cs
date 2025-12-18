using UnityEngine;

public interface IMonoBehaviour
{
    public void Init();
}

public abstract class MonoBehaviourBase : MonoBehaviour
{
    public bool IsNotSubManagerInit;
    public abstract void Init();
    public abstract void Destroy();
}

public abstract class MonoBehaviourManager<T> : MonoBehaviourBase
    where T : MonoBehaviour
{
    private static T instance = null;
    public static T Instance => instance;

    public override void Init()
    {
        //print(name + " Init");
        if (instance != null && instance != this)
        {
            IsNotSubManagerInit = true;
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        instance = this as T;
    }

    public override void Destroy()
    {
        if (instance == this)
            instance = null;
    }
}
