using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Data/AttackEffect/ForceStand")]
// 攻击后敌人强制站立效果
public class ForceStandEffectSO : AttackEffectSO
{
    public override void CharacterEffect(CharacterStateMachine stateMachine, 
        AttackEffectInfo attackEffectInfo)
    {
        if (stateMachine.Health.IsDie) return;

        if (stateMachine is PlayerStateMachine playerStateMachine)
            stateMachine.SwitchState(new PlayerGetHitState(playerStateMachine, 
                attackEffectInfo.duration));
        else if (stateMachine is MonsterStateMachine monsterStateMachine)
            stateMachine.SwitchState(new MonsterGetHitState(monsterStateMachine,
                attackEffectInfo.duration));
    }
}
