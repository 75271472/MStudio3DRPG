using UnityEngine;

public abstract class MonsterBaseState : State
{
    protected MonsterStateMachine stateMachine;

    public MonsterBaseState(MonsterStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    protected void SetChase(Vector3 pos)
    {
        if (!stateMachine.Agent.enabled) return;

        MonsterMoveInfo moveData = stateMachine.MonsterMoveSO;

        stateMachine.Agent.isStopped = false;
        stateMachine.Agent.angularSpeed = moveData.angularSpeed;
        stateMachine.Agent.acceleration = moveData.acceleration;
        stateMachine.Agent.speed = moveData.chaseSpeed;
        stateMachine.Agent.stoppingDistance = 
            stateMachine.MonsterComboList.MaxDiatance;
        SetDestination(pos);
    }

    protected void SetPatrol(Vector3 pos)
    {
        if (!stateMachine.Agent.enabled) return;

        MonsterMoveInfo moveData = stateMachine.MonsterMoveSO;

        stateMachine.Agent.isStopped = false;
        stateMachine.Agent.angularSpeed = moveData.angularSpeed;
        stateMachine.Agent.acceleration = moveData.acceleration;
        stateMachine.Agent.speed = moveData.defaultSpeed;
        stateMachine.Agent.stoppingDistance = moveData.stopDistance;
        SetDestination(pos);
    }

    protected void SetGuard()
    {
        if (!stateMachine.Agent.enabled) return;

        MonsterMoveInfo moveData = stateMachine.MonsterMoveSO;

        stateMachine.Agent.isStopped = false;
        stateMachine.Agent.angularSpeed = moveData.angularSpeed;
        stateMachine.Agent.acceleration = moveData.acceleration;
        stateMachine.Agent.speed = 0;
        stateMachine.Agent.stoppingDistance = moveData.stopDistance;
        SetDestination(stateMachine.transform.position);
    }

    protected void SetDestination(Vector3 pos)
    {
        if (!stateMachine.Agent.enabled) return;
        stateMachine.Agent.destination = pos;
    }
}
