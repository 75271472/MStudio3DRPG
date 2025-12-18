using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Data/Attack")]
public class AttackSO : ScriptableObject
{
    // 攻击动画名称
    public string actionName;
    // 与前一动画的过度时间
    public float transitionDuration;
    // 下一连击动画下标
    public int comboStateIndex;
    // 可连击时间比例，注意DisableWeaponLogic的时间
    // 否则连击时WeaponLogic不会Disable
    public float comboAttackTime;
    // 攻击力系数
    public float damageCoeff;
    // 攻击半径
    public float attackRange;
    // 攻击允许的角度（角度制）
    public float attackAngle;
    // navMeshStopDistance 冗余系数，通常设置为0.9
    public float attackDieCoeff;
    public float StopDistance => attackRange * attackDieCoeff;

    public AttackEffectInfo attackEffectInfo;
    public List<AttackEffectSO> attackEffectList;

    public void ApplyEffect(EffectApplicator applicator)
    {
        foreach (var attackEffect in attackEffectList)
        {
            attackEffect?.CharacterEffect(applicator.StateMachine, attackEffectInfo);
        }
    }
}
