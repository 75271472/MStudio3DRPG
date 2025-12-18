using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MonsterGetHitState : MonsterBaseState
{
    private const string AnimationTag = "GetHit";
    private readonly int GetHitHash = Animator.StringToHash("GetHit");
    private const float CrossFixedTime = 0.2f;
    // 受击状态持续时间

    private float duration;
    // 当前动画播放时间，这就要求GetHit动画要为循环动画
    //private float playTime;

    public MonsterGetHitState(MonsterStateMachine stateMachine, 
        float duration = 0) : base(stateMachine)
    {
        this.duration = duration;
        //playTime = 0;
    }

    public override void Enter()
    {
        // TODO:放弃使用调整动画速度适配duration的方案
        // 从GetHitState转GetHitState会停留在GuradState，不知道为啥
        // 不duration使得每个角色的GetHit时长都是固定的，无法跟随外部调整
        stateMachine.Animator.CrossFadeInFixedTime(GetHitHash, CrossFixedTime);
        //SpeedNormalizedAnimation(stateMachine.Animator, "GetHit", duration);
        // 延迟一帧再调整速度
        //stateMachine.StartCoroutine(DelayedSpeedAdjustment());

        SetGuard();
    }

    private IEnumerator DelayedSpeedAdjustment()
    {
        yield return null; // 等待一帧，确保 Animator 已初始化

        SpeedNormalizedAnimation(stateMachine.Animator, "GetHit", duration);
    }

    public override void Tick(float deltaTime)
    {
        float normalizedTime = GetNormalizedTime(stateMachine.Animator, AnimationTag);

        if (normalizedTime < 1)
        {

        }
        //if (playTime < duration)
        //{

        //}
        else
        {
            Debug.Log("switch to gurad");
            // 播放完毕切换到Guard状态，只有Guard状态会循环检测Targeter中的Target
            stateMachine.SwitchState(new MonsterGuardState(stateMachine));
        }

        //playTime += Time.deltaTime;
    }

    public override void Exit()
    {

    }
}
