using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerRunState : PlayerLocomotionState
{
    private const float LocomotionSpeedValue = 1;
    private MouseTarget targetObj;
    private float stopDistance;
    private float stopAngle;
    // 目标是否为Monster
    private bool isChasing;

    public PlayerRunState(PlayerStateMachine stateMachine, 
        MouseTarget targetObj = null) : base(stateMachine)
    {
        SpeedValue = LocomotionSpeedValue;
        this.targetObj = targetObj;
    }

    public override void Enter()
    {
        // target = null或 Monster已死
        if (!stateMachine.Agent.enabled || targetObj == null || 
            targetObj is EnemyTarget && IsMonsterDie(targetObj))
        {
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
            return;
        }

        base.Enter();

        // target != null 并且 Monster存活 或 点击地面
        SetRun();
        // 再次点击时重新进入状态，
        // 否则若之前Run为点击Gournd时进入
        // 如果此时点击Monster，IsChasie仍未之前的false，
        MouseManager.Instance.OnMoveEvent += OnRunHandler;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        CheckRun();
    }

    public override void Exit()
    {
        base.Exit();

        MouseManager.Instance.OnMoveEvent -= OnRunHandler;
    }

    private void SetRun()
    {
        // 是否点击Monster 或点击 可摧毁物体
        isChasing = IsMonsterAlive(targetObj) || targetObj is DestructibleTarget;

        if (isChasing)
        {
            SetAttack(targetObj.transform.position, ref stopDistance, ref stopAngle);
        }
        else
        {
            SetMove(targetObj.transform.position, ref stopDistance, ref stopAngle);
        }
    }

    private void CheckRun()
    {
        // targetObj = null 或 初始目标为Monster但Monster.IsActiveTarget = false（目标死亡）
        if (targetObj == null || targetObj is EnemyTarget && IsMonsterDie(targetObj))
        {
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
            return;
        }

        // 向Monster或Destructible移动状态下
        if (isChasing)
        {
            if (!stateMachine.transform.IsTargetInAreaByRay(targetObj.gameObject, 
                stopDistance, stopAngle))
            {
                // 更新Monster位置和攻击朝向
                stateMachine.Agent.destination = targetObj.transform.position;
                stateMachine.transform.UpdateLookToTarget(targetObj.gameObject,
                    stateMachine.PlayerMoveSO.angularSpeed);

                return;
            }
        }
        // 向Target移动状态下
        else
        {
            if (!stateMachine.transform.IsTargetInDistance(targetObj.gameObject, 
                stopDistance))
            {
                // 只更新Monster位置
                stateMachine.Agent.destination = targetObj.transform.position;

                return;
            }
        }

        if (isChasing && !stateMachine.PlayerComboList.CanAttackByComboIndex(0))
            stateMachine.SwitchState(new PlayerAttackCDState(stateMachine, targetObj));
        else if (isChasing)
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, 0, targetObj));
        else
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
    }
}
