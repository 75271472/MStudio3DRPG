using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private const string AnimationTag = "Attack";

    private AttackSO attackSO;
    private MouseTarget targetObj;
    private float normalizedTime;
    private bool isAttackOnce;

    public PlayerAttackState(PlayerStateMachine stateMachine, 
        int attackIndex, MouseTarget targetObj) : base(stateMachine) 
    {
        this.attackSO = stateMachine.PlayerComboList.GetAttack(attackIndex);
        this.targetObj = targetObj;
    }

    public override void Enter()
    {
        if (targetObj == null || attackSO == null)
        {
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
            return;
        }

        SetIdle();
        // 点击物体为可摧毁对象，isAttackOnce置真
        isAttackOnce = targetObj is DestructibleTarget;
        stateMachine.WeaponHandler.SetDamage(attackSO, targetObj.gameObject);
        MouseManager.Instance.OnMoveEvent += OnAttackHandler;
        
        stateMachine.Animator.CrossFadeInFixedTime(
            attackSO.actionName, attackSO.transitionDuration);
    }

    public override void Tick(float deltaTime)
    {
        if (targetObj == null || attackSO == null)
        {
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
            return;
        }

        normalizedTime = GetNormalizedTime(stateMachine.Animator, AnimationTag);

        if (normalizedTime < 1)
        {
            stateMachine.transform.UpdateLookToTarget(targetObj.gameObject,
                stateMachine.PlayerMoveSO.angularSpeed);

            TryCombo(normalizedTime);
        }
        else
        {
            stateMachine.SwitchState(new PlayerRunState(stateMachine, targetObj));
        }
    }

    public override void Exit()
    { 
        MouseManager.Instance.OnMoveEvent -= OnAttackHandler;
        stateMachine.WeaponHandler.DisableAllWeaponLogic();

        // 只有连招的最后一个攻击动画，才会重置攻击CD
        if (attackSO.comboStateIndex == -1)
            stateMachine.PlayerComboList.ResetAttackCD();
    }

    public void TryCombo(float normalizedTime)
    {
        //Debug.Log(attackSO.actionName + Vector3.Distance(targetObj.transform.position,
        //    stateMachine.transform.position));

        //Debug.Log(attackSO == null);
        if (attackSO.comboStateIndex == -1 ||
            normalizedTime < attackSO.comboAttackTime) return;

        // Monster死亡转换 或 isAttackOnce为真 为Idle状态
        if (IsMonsterDie(targetObj) || isAttackOnce)
        {
            // 状态转换后一定要return !!!
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
            return;
        }
            
        // 在攻击范围内，转换attackState状态
        if (stateMachine.transform.IsTargetInAreaByRay(targetObj.gameObject, 
            attackSO.StopDistance, attackSO.attackAngle))
        {
            stateMachine.SwitchState(new PlayerAttackState(stateMachine,
                attackSO.comboStateIndex, targetObj));
        }
        else
            stateMachine.SwitchState(new PlayerRunState(stateMachine, targetObj));
    }

    // 攻击时鼠标点击触发OnMoveEvent，执行OnAttackHandler，如果点击物体为当前攻击物体
    // 不切换状态，否则切换Run状态并向目标跑去
    private void OnAttackHandler(MouseTarget targetObj)
    {
        if (this.targetObj.Equals(targetObj)) return;

        stateMachine.SwitchState(new PlayerRunState(stateMachine, targetObj));
    }
}
