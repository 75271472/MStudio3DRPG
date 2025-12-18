using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public abstract class PlayerLocomotionState : PlayerBaseState
{
    protected readonly int LocomotionSpeedHash = Animator.StringToHash("Speed");
    // 动画混合树值设置的阻尼时间
    protected const float AnimatorDampTime = 0.05f;
    protected float SpeedValue;

    private readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    // 动画切换阻尼时间
    private const float CrossFixedTime = 0.3f;

    public PlayerLocomotionState(PlayerStateMachine stateMachine) : 
        base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(LocomotionHash, CrossFixedTime);
    }

    public override void Tick(float deltaTime)
    {
        UpdateAnimator(SpeedValue, deltaTime);
    }

    public override void Exit()
    {
        
    }

    private void UpdateAnimator(float speedValue, float deltaTime)
    {
        stateMachine.Animator.SetFloat(LocomotionSpeedHash,
            speedValue, AnimatorDampTime, deltaTime);
    }
}
