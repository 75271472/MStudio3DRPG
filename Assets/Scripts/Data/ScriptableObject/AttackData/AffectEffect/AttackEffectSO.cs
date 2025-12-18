using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackEffectInfo
{
    // 相对动画的标准化持续时间
    public float duration;
    // 击退力大小
    public float force;
    // 击退力方向（外部赋值）
    [HideInInspector] public Vector3 forceVect;
    // 攻击者（外部赋值）
    [HideInInspector] public GameObject attacker;
}

public abstract class AttackEffectSO : ScriptableObject
{
    public abstract void CharacterEffect(CharacterStateMachine stateMachine, 
        AttackEffectInfo attackEffectInfo);
}
