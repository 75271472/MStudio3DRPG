using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    public bool IsPause { get; protected set; } = false;
    protected State currentState;

    private void Update()
    {
        currentState?.Tick(Time.deltaTime);
    }

    public virtual void SwitchState(State newState)
    {
        if (!CheckSwitchStateByState(newState)) return;

        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    // 根据currentState和newState判断当前是否进行状态转换
    // 返回false不进行转换，返回true进行转换
    protected bool CheckSwitchStateByState(State newState)
    {
        // 当前状态机不暂停，进行状态转换
        if (!IsPause) return true;
        // 当前状态机暂停且当前状态为默认状态，不进行状态转换
        if (currentState is IDefaultState) return false;
        // 当前状态机暂停，且当前状态不为默认状态
        // 则判断要转换的状态
        // 要转换状态为默认状态，进行状态转换
        // 要转换状态不为默认状态，不进行状态转换
        return newState is IDefaultState;
    }

    public virtual void Pause() => IsPause = true;
    public virtual void UnPause() => IsPause = false;
}
