using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[System.Serializable]
public class AttackComboList
{
    public float MaxDiatance => attackComboList.Last().AttackRange * 
        attackRangeFactor;
    public float MaxAngle => attackComboList.Last().AttackAngle;

    [SerializeField] private List<AttackComboSO> attackComboList = new List<AttackComboSO>();
    
    private AttackComboSO currentCombo;
    // 攻击距离系数，切换武器时更新
    private float attackRangeFactor = 1;

    public void AttackComboListInit()
    {
        foreach (var combo in attackComboList)
        {
            combo.AttackComboInit();
        }

        // 将combo按AttackRange连招判定距离从小到大排序
        attackComboList.Sort((comboA, comboB) =>
            comboA.AttackRange.CompareTo(comboB.AttackRange));
    }

    public void UpdateAttackRangeFactor(float attackRangeFactor)
    {
        this.attackRangeFactor = attackRangeFactor;
    }

    // 传入判定距离，返回最小的，大于判定距离的连招
    public AttackSO GetAttack(int attackIndex)
    {
        return currentCombo?.GetAttack(attackIndex);
    }

    public void ResetAttackCD()
    {
        //Debug.Log($"ResetAttackCD Call {attackComboList.IndexOf(currentCombo)}");
        currentCombo?.ResetAttackCD();
    }

    public bool CanAttackByComboIndex(int comboIndex)
    {
        if (comboIndex < 0 || comboIndex >= attackComboList.Count) return false;
        currentCombo = attackComboList[comboIndex];
        return attackComboList[comboIndex].CanAttack();
    }

    public bool CanAttack(float distance)
    {
        UpdateCurrentAttack(distance);
        //Debug.Log(currentCombo == null);
        if (currentCombo == null)
            return false;
        //Debug.Log($"CanAttack Call: {attackComboList.IndexOf(currentCombo)}");
        return currentCombo.CanAttack();
    }

    // 返回值，返回更新后的currentCombo与之前是否相同，相同返回true
    // 并判断currentCombo是否为null，为null返回false
    private bool UpdateCurrentAttack(float distance)
    {
        if (distance > MaxDiatance)
        {
            currentCombo = null;
        }

        AttackComboSO attackCombo = null;
        // 寻找判定距离内，攻击范围最大的最佳连招
        foreach (var combo in attackComboList)
        {
            if (combo.AttackRange * attackRangeFactor >= distance)
            {
                attackCombo = combo;
                break;
            }
        }

        // 与当前连招比较，如果当前连招就是最佳连招，或者当前连招的攻击范围大于最佳连招
        // 返回当前连招的下一个攻击
        if (currentCombo != null && attackCombo.Equals(currentCombo))
        {
            return true;
        }
        // 当前连招为空或者最佳连招的攻击距离大于当前连招
        // 返回最佳连招的第一个攻击，并更新当前连招
        else
        {
            currentCombo = attackCombo;
            return false;
        }
    }
}

[CreateAssetMenu(fileName = "New Data", menuName = "Data/Attack/AttackCombo")]
public class AttackComboSO : ScriptableObject
{
    // 连招判定距离
    public float AttackRange {  get; private set; }
    public float AttackAngle { get; private set; }

    public List<AttackSO> comboList;
    public float attackCD;
    
    private float nowAttackCD;

    public AttackSO GetAttack(int index)
    {
        if (comboList != null && index >= 0 && index < comboList.Count)
            return comboList[index];
        return null;
    }

    public void AttackComboInit()
    {
        nowAttackCD = attackCD;
        // 将最远的攻击距离作为连招的判定距离
        AttackSO maxRangeAttackSO = GetMaxRange();
        AttackRange = maxRangeAttackSO.attackRange;
        AttackAngle = maxRangeAttackSO.attackAngle;

        MonoManager.Instance.StartCoroutine(NoAttackCDMonitor());
    }

    public void ResetAttackCD()
    {
        MonoManager.Instance.StopCoroutine(AttackCDTimer());
        nowAttackCD = 0;
        MonoManager.Instance.StartCoroutine(AttackCDTimer());
    }

    public bool CanAttack()
    {
        //Debug.Log($"CanAttack Call: {nowAttackCD} {attackCD}");
        if (nowAttackCD < attackCD) return false;

        return true;
    }

    // 获取连招中技能攻击范围最远的攻击距离
    private AttackSO GetMaxRange()
    {
        float range = 0;
        AttackSO maxRangeAttackSO = null;

        foreach (var attack in comboList)
        {
            if (attack.attackRange > range)
            {
                range = attack.attackRange;
                maxRangeAttackSO = attack;
            }
                
        }

        return maxRangeAttackSO;
    }

    private IEnumerator AttackCDTimer()
    {
        while (nowAttackCD < attackCD)
        {
            nowAttackCD += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator NoAttackCDMonitor()
    {
        while (true)
        {
            //Debug.Log(nowAttackCD);
            yield return null;
        }
    }
}
