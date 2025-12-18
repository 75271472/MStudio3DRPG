using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState : State
{
    protected PlayerStateMachine stateMachine;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    protected void SetAttack(Vector3 pos, ref float stopDistance, ref float stopAngle)
    {
        if (!stateMachine.Agent.enabled) return;

        stateMachine.Agent.isStopped = false;
        stateMachine.Agent.speed = stateMachine.PlayerMoveSO.defaultSpeed;
        stateMachine.Agent.stoppingDistance =
            stateMachine.PlayerComboList.MaxDiatance;
        stopDistance = stateMachine.PlayerComboList.MaxDiatance;
        stopAngle = stateMachine.PlayerComboList.MaxAngle;
        stateMachine.Agent.destination = pos;
    }

    protected void SetMove(Vector3 pos, ref float stopDistance, ref float stopAngle)
    {
        if (!stateMachine.Agent.enabled) return;

        stateMachine.Agent.isStopped = false;
        stateMachine.Agent.speed = stateMachine.PlayerMoveSO.defaultSpeed;
        stateMachine.Agent.stoppingDistance =
            stateMachine.PlayerMoveSO.stopDistance;
        stopDistance = stateMachine.PlayerMoveSO.stopDistance;
        stopAngle = -1;
        stateMachine.Agent.destination = pos;
    }

    protected void SetIdle()
    {
        if (!stateMachine.Agent.enabled) return;

        stateMachine.Agent.isStopped = false;
        stateMachine.Agent.speed = 0;
        stateMachine.Agent.stoppingDistance =
            stateMachine.PlayerMoveSO.stopDistance;
        StopAgent();
    }

    protected void StopAgent()
    {
        stateMachine.Agent.destination = stateMachine.transform.position;
    }

    protected void OnIdleHandler()
    {
        stateMachine.SwitchState(new PlayerIdleState(stateMachine));
    }

    protected void OnRunHandler(MouseTarget targetObj)
    {
        stateMachine.SwitchState(new PlayerRunState(stateMachine, targetObj));
    }

    // 三种状态：
    // 点击地面：IsGroundTarget = true && IsMonsterAlive = false;
    // 点击活Monster：IsGroundTarget = false && IsMonsterAlive = true
    // 点击死Monster：IsGroundTarget = false && IsMonsterAlive = false
    protected bool IsGroundTarget(MouseTarget targetObj)
    {
        return targetObj is GroundTarget;
    }

    protected bool IsMonsterAlive(MouseTarget targetObj)
    {
        if (targetObj is EnemyTarget target)
        {
            return target.CheckMonster();
        }
        return false;
    }

    protected bool IsMonsterDie(MouseTarget targetObj)
    {
        return !IsGroundTarget(targetObj) && !IsMonsterAlive(targetObj);
    }
}
