using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerLocomotionState, IDefaultState
{
    private const float LocomotionSpeedValue = 0;

    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        SpeedValue = LocomotionSpeedValue;
    }

    public override void Enter()
    {
        base.Enter();

        MouseManager.Instance.OnMoveEvent += OnMoveHandler;
        SetIdle();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
    }

    public override void Exit()
    {
        base.Exit();

        MouseManager.Instance.OnMoveEvent -= OnMoveHandler;
    }

    private void OnMoveHandler(MouseTarget targetObj)
    {
        if (stateMachine.IsPause) return;

        stateMachine.SwitchState(new PlayerRunState(stateMachine, targetObj));
    }
}
