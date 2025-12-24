using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Health : MonoBehaviour
{
    public event Action<int, GameObject> OnTakeDamageEvent;
    public event Action<int> OnRecoveryEvent;
    public event Action<GameObject> OnDieEvent;
    // 可以被哪些层级的Character攻击
    public LayerMask attackMask;

    // TODO:整个项目应该统一使用Manager中CharachterData中的IsDie
    // 这里先简单写
    public bool IsDie {  get; private set; }

    public void ResetIsDie()
    {
        IsDie = false;
    }

    public void HealthInit()
    {
        OnRecoveryEvent = null;
        OnTakeDamageEvent = null;
        OnDieEvent = null;

        OnTakeDamageEvent += (damage, obj) => print(name + " Get Damage " + damage
            + " By " + obj.name);
    }

    // 自身伤害函数调用
    // TakeDamage(10, null, true)
    public void TakeDamage(int damage, GameObject obj)
    {
        // 当前死亡、攻击者为自身且isSelfDamage为假，退出伤害函数
        if (IsDie || obj == null) return;
        if ((attackMask.value & (1 << obj.layer)) == 0) return;
        //print($"TakeDamage {damage} {obj.name} {gameObject.name}");
        OnTakeDamageEvent?.Invoke(damage, obj);
    }

    public void Recovery(int recovery)
    {
        if (IsDie) return;

        OnRecoveryEvent?.Invoke(recovery);
    }

    public void OnDieHandler(GameObject attacker)
    {
        IsDie = true;
        OnDieEvent?.Invoke(attacker);
    }
}
