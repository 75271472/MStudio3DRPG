using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Data/AttackEffect/Repel")]
public class RepelEffectSO : AttackEffectSO
{
    public override void CharacterEffect(CharacterStateMachine stateMachine, 
        AttackEffectInfo attackEffectInfo)
    {
        //Debug.Log("RepelEffect: " + attackEffectInfo.attacker.Equals(stateMachine.gameObject));
        if (attackEffectInfo.attacker.Equals(stateMachine.gameObject)) return;

        stateMachine.ForceApplicator.AddForce(
            attackEffectInfo.force * attackEffectInfo.forceVect.normalized);
    }
}
