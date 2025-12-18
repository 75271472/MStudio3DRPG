using UnityEngine;

public class MonsterAttackCDState : MonsterBaseState
{
    private readonly int AttackCDHash = Animator.StringToHash("AttackCD");
    private const float CrossFixedTime = 0.1f;

    private GameObject targetObj;

    public MonsterAttackCDState(MonsterStateMachine stateMachine, GameObject targetObj)
        : base(stateMachine)
    {
        this.targetObj = targetObj;
    }

    public override void Enter()
    {
        if (targetObj == null)
        {
            stateMachine.SwitchState(new MonsterGuardState(stateMachine));
            return;
        }

        // 设置Guard状态的Agent，防止在攻击状态移动
        SetGuard();
        stateMachine.Animator.CrossFadeInFixedTime(AttackCDHash, CrossFixedTime);

        stateMachine.MonsterTargeter.OnTargetExit += OnTargetExitHandler;
    }

    public override void Tick(float deltaTime)
    {
        if (targetObj == null)
        {
            stateMachine.SwitchState(new MonsterGuardState(stateMachine));
            return;
        }

        stateMachine.transform.UpdateLookToTarget(targetObj, 
            stateMachine.MonsterMoveSO.angularSpeed);
        CheckAttackCD();
    }

    public override void Exit()
    {
        stateMachine.MonsterTargeter.OnTargetExit -= OnTargetExitHandler;
    }

    private void CheckAttackCD()
    {
        // 如果targetObj位于攻击区域以外，切换ChaseState状态
        if (!stateMachine.transform.IsTargetInArea(targetObj, 
            stateMachine.MonsterComboList.MaxDiatance, 
            stateMachine.MonsterComboList.MaxAngle))
        {
            stateMachine.SwitchState(new MonsterChaseState(stateMachine, targetObj));
            return;
        }
        // 使用IsTargetInAreaByRay获取的，当前Transform与Hit.point的间距
        // 作为CanAttack中选择AttackCombo的距离判断依据
        if (!stateMachine.MonsterComboList.CanAttack(
            stateMachine.transform.GetTargetDistance(targetObj)))
        {
            return;
        }

        stateMachine.SwitchState(new MonsterAttackState(stateMachine, 0, targetObj));
    }

    private void OnTargetExitHandler(GameObject target)
    {
        if (!this.targetObj.Equals(target)) return;

        stateMachine.SwitchState(new MonsterGuardState(stateMachine));
    }
}
