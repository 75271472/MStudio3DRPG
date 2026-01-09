using UnityEngine;

public class MonsterChaseState : MonsterBaseState
{
    private readonly int ChaseHash = Animator.StringToHash("Chase");
    private const float CrossFixedTime = 0.1f;

    private GameObject targetObj;

    public MonsterChaseState(MonsterStateMachine stateMachine, 
        GameObject targetObj = null) : base(stateMachine) 
    {
        this.targetObj = targetObj;
    }

    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(ChaseHash, CrossFixedTime);

        stateMachine.MonsterTargeter.OnTargetExit += OnTargetExitHandler;
        SetChase();
    }

    public override void Tick(float deltaTime)
    {
        CheckChase();
    }

    public override void Exit()
    {
        stateMachine.MonsterTargeter.OnTargetExit -= OnTargetExitHandler;
    }

    private void SetChase()
    {
        if (targetObj != null)
        {
            SetChase(targetObj.transform.position);
        }
        else
        {
            stateMachine.SwitchState(new MonsterGuardState(stateMachine));
        }
    }

    // 判断是否在攻击距离内 -> 判断AttackCD时间 -> 条件满足跳转AttackState
    // 不在攻击距离内设置Agent.destination
    // 不在AttackCD时间跳转AttackCD
    private void CheckChase()
    {
        if (targetObj == null)
        {
            stateMachine.SwitchState(new MonsterGuardState(stateMachine));
            return;
        }

        // 判断二维平面距离，实际的三维距离可能大于MaxDistance

        if (!stateMachine.transform.IsTargetInAreaByRay(targetObj,
            stateMachine.MonsterComboList.MaxDiatance, 
            stateMachine.MonsterComboList.MaxAngle, out float distance))
        {
            SetDestination(targetObj.transform.position);
            stateMachine.transform.UpdateLookToTarget(targetObj,
                stateMachine.MonsterMoveSO.angularSpeed);
            return;
        }
        
        // 距离判定通过的状态下，判断CD
        if (!stateMachine.MonsterComboList.CanAttack(distance))
            //stateMachine.transform.GetTargetDistanceInSaveHeight(targetObj)))
        {
            //Debug.Log($"MonsterChase Distance {distance}");
            stateMachine.SwitchState(new MonsterAttackCDState(stateMachine, targetObj));
            return;
        }

        stateMachine.SwitchState(new MonsterAttackState(stateMachine, 0, targetObj));
    }

    private void OnTargetExitHandler(GameObject targetObj)
    {
        if (!this.targetObj.Equals(targetObj)) return;

        stateMachine.SwitchState(new MonsterGuardState(stateMachine));
    }
}
