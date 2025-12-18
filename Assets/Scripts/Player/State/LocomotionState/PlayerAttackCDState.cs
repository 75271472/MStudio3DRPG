using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerAttackCDState : PlayerLocomotionState
{
    private const float LocomotionSpeedValue = 0;
    private MouseTarget targetObj;
    private float stopDistance;
    private float stopAngle;

    public PlayerAttackCDState(PlayerStateMachine stateMachine, MouseTarget targetObj) 
        : base(stateMachine)
    {
        SpeedValue = LocomotionSpeedValue;
        this.targetObj = targetObj;
    }

    public override void Enter()
    {
        // 只有Target为Monster才能进入
        if (targetObj == null || !IsMonsterAlive(targetObj))
        {
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
            return;
        }

        base.Enter();
        // 在SetAttack前，先SetIdle，停止自身移动，防止从Run状态切换过来后由于Destination
        // 依然移动
        //SetIdle();
        SetAttack(targetObj.transform.position, ref stopDistance, ref stopAngle);
        MouseManager.Instance.OnMoveEvent += OnMoveHandler;
    }

    public override void Tick(float deltaTime)
    {
        if (targetObj == null)
        {
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
            return;
        }

        base.Tick(deltaTime);

        stateMachine.transform.UpdateLookToTarget(targetObj.gameObject,
            stateMachine.PlayerMoveSO.angularSpeed);
        CheckAttackCD();
    }

    public override void Exit()
    {
        base.Exit();

        MouseManager.Instance.OnMoveEvent -= OnMoveHandler;
    }

    private void CheckAttackCD()
    {
        // target不在攻击范围内，切换Run状态
        if (!stateMachine.transform.IsTargetInAreaByRay(targetObj.gameObject, stopDistance, 
            stopAngle))
        {
            stateMachine.SwitchState(new PlayerRunState(stateMachine, targetObj));
            return;
        }

        // target在攻击范围内，但处于攻击CD状态
        // 停止Agent移动，注意不能直接调用SetIdle，
        // 这样会将stopDistance设置MoveData的stopDistance而不是AttackData中的AttackRange
        if (!stateMachine.PlayerComboList.CanAttackByComboIndex(0))
        {
            StopAgent();
            return;
        }

        stateMachine.SwitchState(new PlayerAttackState(stateMachine, 0, targetObj));
    }

    private void OnMoveHandler(MouseTarget targetObj)
    {
        // 如果目标对象与当前对象相同，退出
        if (this.targetObj.Equals(targetObj)) return;

        stateMachine.SwitchState(new PlayerRunState(stateMachine, targetObj));
    }
}
