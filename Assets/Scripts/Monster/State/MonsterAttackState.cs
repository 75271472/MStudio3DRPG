using UnityEngine;

public class MonsterAttackState : MonsterBaseState
{
    private const string AnimationTag = "Attack";

    private GameObject targetObj;
    private AttackSO attackSO;

    public MonsterAttackState(MonsterStateMachine stateMachine, int attackIndex,
        GameObject targetObj) : base(stateMachine)
    {
        this.targetObj = targetObj;
        attackSO = stateMachine.MonsterComboList.GetAttack(attackIndex);
    }

    public override void Enter()
    {
        if (targetObj == null || attackSO == null)
        {
            stateMachine.SwitchState(new MonsterGuardState(stateMachine));
            return;
        }

        // 设置Guard状态的Agent，防止在攻击状态移动
        SetGuard();
        stateMachine.WeaponHandler.SetDamage(attackSO, targetObj);
        stateMachine.Animator.CrossFadeInFixedTime(
            attackSO.actionName, attackSO.transitionDuration);

        stateMachine.MonsterTargeter.OnTargetExit += OnTargetExitHandler;
    }

    public override void Tick(float deltaTime)
    {
        if (targetObj == null)
        {
            stateMachine.SwitchState(new MonsterGuardState(stateMachine));
            return;
        }

        float normalizedTime = GetNormalizedTime(stateMachine.Animator, AnimationTag);

        if (normalizedTime < 1)
        {
            //stateMachine.transform.UpdateLookToTarget(targetObj,
            //    stateMachine.MonsterMoveSO.angularSpeed);
            TryCombo(normalizedTime);
        }
        else
        {
            stateMachine.SwitchState(new MonsterChaseState(stateMachine, targetObj));
        }
    }

    public override void Exit()
    {
        stateMachine.MonsterTargeter.OnTargetExit -= OnTargetExitHandler;
        stateMachine.WeaponHandler.DisableAllWeaponLogic();

        // 只有连招的最后一个攻击动画，才会重置攻击CD
        if (attackSO.comboStateIndex == -1)
            stateMachine.MonsterComboList.ResetAttackCD();
    }

    private void OnTargetExitHandler(GameObject target)
    {
        if (!this.targetObj.Equals(target)) return;

        stateMachine.SwitchState(new MonsterGuardState(stateMachine));
    }

    private void TryCombo(float normalizedTime)
    {
        //Debug.Log(attackSO.actionName + Vector3.Distance(targetObj.transform.position,
        //    stateMachine.transform.position));
        if (attackSO.comboStateIndex == -1 || 
            normalizedTime < attackSO.comboAttackTime) return;

        if (stateMachine.transform.IsTargetInAreaByRay(targetObj, 
            attackSO.StopDistance, attackSO.attackAngle, out var distance))
        {
            stateMachine.SwitchState(new MonsterAttackState(stateMachine,
                attackSO.comboStateIndex, targetObj));
        }
        else
            stateMachine.SwitchState(new MonsterChaseState(stateMachine, targetObj));
    }
}
