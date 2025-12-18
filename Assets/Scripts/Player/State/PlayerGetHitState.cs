using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerGetHitState : PlayerBaseState
{
    private const string AnimationTag = "GetHit";
    private readonly int GetHitHash = Animator.StringToHash("GetHit");
    private const float CrossFixedTime = 0.3f;
    // 受击状态持续时间
    private float duration;
    // 当前动画播放时间，这就要求GetHit动画要为循环动画
    private float playTime;

    public PlayerGetHitState(PlayerStateMachine stateMachine,
        float duration) : base(stateMachine)
    {
        this.duration = duration;
    }

    public override void Enter()
    {
        SpeedNormalizedAnimation(stateMachine.Animator, "GetHit", duration);
        stateMachine.Animator.CrossFadeInFixedTime(GetHitHash, CrossFixedTime);
        SetIdle();
    }

    public override void Tick(float deltaTime)
    {
        float normalizedTime = GetNormalizedTime(stateMachine.Animator, AnimationTag);

        //Debug.Log(normalizedTime);
        if (normalizedTime < 1)
        {

        }
        else
        {
            // 播放完毕切换到Guard状态，只有Guard状态会循环检测Targeter中的Target
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
        }

        playTime += Time.deltaTime;
    }

    public override void Exit()
    {

    }
}
