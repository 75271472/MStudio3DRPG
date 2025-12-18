using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ToolManager : BaseManager<ToolManager>
{
    public void Timer(float time, UnityAction action)
    {
        if (time == 0)
            action?.Invoke();
        else if (time > 0)
            MonoManager.Instance.StartCoroutine(TimerCoroutine(time, action));
    }

    private IEnumerator TimerCoroutine(float time, UnityAction action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }
}
