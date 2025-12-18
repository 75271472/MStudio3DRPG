using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDieState : PlayerBaseState
{
    private readonly int DieHash = Animator.StringToHash("Die");
    private const float CrossFixedTime = 0.3f;

    public PlayerDieState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(DieHash, CrossFixedTime);
    }

    public override void Tick(float deltaTime)
    {

    }

    public override void Exit()
    {
        
    }
}
