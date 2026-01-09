using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class PlayerStateInfo : CharacterStateInfo
{
    public bool isDie;
    public int health;
    // 当前等级
    public int currentLevel;
    // 最大等级
    public int maxLevel;
    // 等级基础经验值
    public int baseExp;
    // 当前经验值
    public int currentExp;
    // 升级增益
    public float levelBuff;

    // 增益计算公式 增益后值 = 1 + （当前等级 - 1） * 升级增益
    //public float LevelMultiplier => 1 + (currentLevel - 1) * levelBuff;
    public float LevelMultiplier => 1 + levelBuff;

    public event Action<int, int, int> OnLevelUpUpdateHPEvent;
    public event Action OnLevelUpEvent;

    public void UpdateExp(int point)
    {
        currentExp += point;

        while (currentExp >= baseExp)
            LevelUp();
    }

    public void ResetEvent()
    {
        OnLevelUpEvent = null;
        OnLevelUpUpdateHPEvent = null;
    }

    private void LevelUp()
    {
        currentLevel++;

        currentExp -= baseExp;
        baseExp = (int)(baseExp * LevelMultiplier);

        maxHealth = (int)(maxHealth * LevelMultiplier);
        OnLevelUpUpdateHPEvent?.Invoke(maxHealth - health, maxHealth, maxHealth);
        health = maxHealth;

        //levelBuff *= LevelMultiplier;
        OnLevelUpEvent?.Invoke();
    }

    public void SetDie()
    {
        health = 0;
        isDie = true;
    }
}
