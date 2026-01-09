using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;

public class AttackInfo
{
    // 伤害
    public int damage;
    // 暴击时相当普通攻击的几倍
    public int criticalMultipler;
    // 暴击率(0,1)之间
    public float criticalRate;

    public int GetDamage()
    {
        float damageResult = Random.value < criticalRate ?
            damage * criticalMultipler : damage;
        return Mathf.RoundToInt(damageResult);
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("Damage: " + damage);
        stringBuilder.AppendLine("CriticalMultipler: " + criticalMultipler);
        stringBuilder.AppendLine("CriticalRate: " + criticalRate);

        return stringBuilder.ToString().Trim();
    }

    /// <summary>
    /// AttackInfo攻击数据类型加法规则
    /// 结果攻击数据 为 两攻击数据伤害之和
    /// 暴击伤害倍数为两攻击数据类型中的较大者
    /// 暴击率为两攻击数据类型之中的较大者
    /// </summary>
    /// <param name="attackInfoA"></param>
    /// <param name="attackInfoB"></param>
    /// <returns></returns>
    public static AttackInfo operator +(AttackInfo attackInfoA, 
        AttackInfo attackInfoB)
    {
        if (attackInfoA == null) return null;
        if (attackInfoB == null) return attackInfoA;

        return new AttackInfo()
        {
            damage = attackInfoA.damage + attackInfoB.damage,
            criticalMultipler = Mathf.Max(attackInfoA.criticalMultipler,
            attackInfoB.criticalMultipler),
            criticalRate = Mathf.Max(attackInfoA.criticalRate, 
            attackInfoB.criticalRate)
        };
    }
}
