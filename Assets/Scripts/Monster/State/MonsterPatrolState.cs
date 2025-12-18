using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MonsterPatrolState : MonsterBaseState
{
    private readonly int PatrolHash = Animator.StringToHash("Patrol");
    private const float CrossFixedTime = 0.1f;

    private GameObject patrolObj;
    private GameObject targetObj;

    public MonsterPatrolState(MonsterStateMachine stateMachine) : 
        base(stateMachine) 
    {
        //Debug.Log("MonsterPatrol");
    }

    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(PatrolHash, CrossFixedTime);

        SetPatrol(stateMachine.MonsterPatroller.GetPatrolPos());
    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.MonsterTargeter.TryGetTarget(out targetObj))
        {
            stateMachine.SwitchState(new MonsterChaseState(stateMachine, targetObj));
            return;
        }

        if (stateMachine.transform.IsTargetInDistance(patrolObj,
            stateMachine.Agent.stoppingDistance))
        {
            stateMachine.SwitchState(new MonsterGuardState(stateMachine,
                stateMachine.MonsterMoveSO.waitTime));
            return;
        }
    }

    public override void Exit()
    {
        
    }

    private void SetPatrol(GameObject patrolObj)
    {
        this.patrolObj = patrolObj;

        SetPatrol(patrolObj.transform.position);
    }
}
