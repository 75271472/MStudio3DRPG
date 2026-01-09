using Unity.VisualScripting.FullSerializer;
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
        // 让判定在范围内的逻辑更简单，这样在AttacCD中在进行判断存在一定冗余
        // 避免帧更新后距离更新使得在AttackCD和Chase之间来回跳转
        if (!stateMachine.transform.IsTargetInAreaByRay(targetObj, 
            stateMachine.MonsterComboList.MaxDiatance, 
            stateMachine.MonsterComboList.MaxAngle, out float distance))
        {
            // 如果距离够了但角度不够，则退出函数继续修正角度
            if (distance <= stateMachine.MonsterComboList.MaxDiatance) return;

            //Debug.Log($"MonsterAttackCD Distance {distance}");
            stateMachine.SwitchState(new MonsterChaseState(stateMachine, targetObj));
            return;
        }

        // 使用IsTargetInAreaByRay获取的，当前Transform与Hit.point的间距
        // 作为CanAttack中选择AttackCombo的距离判断依据
        if (!stateMachine.MonsterComboList.CanAttack(distance))
            //stateMachine.transform.GetTargetDistanceInSaveHeight(targetObj)))
        {
            //Debug.Log("Monster State AttackCD");
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
