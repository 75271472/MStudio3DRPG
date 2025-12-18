using UnityEngine;

public class MonsterDieState : MonsterBaseState
{
    private readonly int DieHash = Animator.StringToHash("Die");
    private const float CrossFixedTime = 0.3f;

    public MonsterDieState(MonsterStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(DieHash, CrossFixedTime);
    }

    public override void Exit()
    {
        
    }

    public override void Tick(float deltaTime)
    {
        
    }
}
