using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MonsterGuardState : MonsterBaseState, IDefaultState
{
    private readonly int GuartHash = Animator.StringToHash("Guard");

    private const float CrossFixedTime = 0.1f;

    // 巡逻基点对象，如果为Guard状态，则patrolObj不应为null
    private GameObject patrolObj = null;
    private float guartTime;

    public MonsterGuardState(MonsterStateMachine stateMachine, float guartTime = 0) : 
        base(stateMachine) 
    {
        this.guartTime = guartTime;
    }

    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(GuartHash, CrossFixedTime);

        // 只有Guard状态才会对patrolObj赋值
        if (stateMachine.MonsterMoveType == EMonsterMoveType.Guard)
        {
            patrolObj = stateMachine.MonsterPatroller.GetPatrolPos();
        }

        SetGuard();
    }

    // 在Tick中先检查是否有Target，否则检查Patrol
    public override void Tick(float deltaTime)
    {
        if (stateMachine.IsPause) return;

        if (stateMachine.MonsterTargeter.TryGetTarget(out var targetObj))
        {
            // TODO:
            // SwitchState之后一定要RETURN退出当前状态不在执行剩余语句！！！
            // 否则可能会出现Switch到新的State之后又因为没有及时退出旧状态而再次Switch
            stateMachine.SwitchState(new MonsterChaseState(stateMachine, targetObj));
            return;
        }

        CheckGuard();
    }

    public override void Exit()
    {
        
    }

    private void CheckGuard()
    {
        // 当前处于Guard状态，并且与巡逻基点距离小于Agent.stoppingDistance，保持Guard状态
        // 并向patrolObj巡逻基点的旋转角度旋转
        if (patrolObj != null && stateMachine.transform.IsTargetInDistance(
            patrolObj, stateMachine.Agent.stoppingDistance))
        {
            stateMachine.transform.UpdateRotateToTarget(
                patrolObj, stateMachine.Agent.angularSpeed);
            return;
        }

        if (guartTime <= 0)
        {
            stateMachine.SwitchState(new MonsterPatrolState(stateMachine));
        }
        else
        {
            guartTime -= Time.deltaTime;
        }
    }
}
