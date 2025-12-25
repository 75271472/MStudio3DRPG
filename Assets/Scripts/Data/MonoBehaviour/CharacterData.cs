using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class CharacterData : MonoBehaviour
{
    [field: SerializeField]
    public AttackComboList AttackComboList { get; protected set; }
    public virtual AttackInfo AttackInfo { get; protected set; }

    // 供外部调用的收到伤害回调
    // 伤害值、当前血量、总血量
    public event Action<int, int, int> OnTakeDamageEvent;
    // 恢复值、当前血量、总血量
    public event Action<int, int, int> OnRecoveryEvent;

    public event Action<GameObject> OnDieEvent;

    public ICharacter Character { get; protected set; }

    public virtual void CharacterDataInit(ICharacter character)
    {
        this.Character = character;

        OnTakeDamageEvent = null;
        OnRecoveryEvent = null;
        OnDieEvent = null;

        AttackComboList.AttackComboListInit();
    }

    public void HealthEventRegist(Health health)
    {
        // TODO:没有-=
        health.OnTakeDamageEvent += OnTakeDamageHandler;
        health.OnRecoveryEvent += OnRecoveryHandler;
    }

    public abstract CharacterStateData GetCharacterStateData();

    public abstract void OnTakeDamageHandler(int damage, GameObject attacker);
    public abstract void OnRecoveryHandler(int recovery);

    // 修改MonsterState中的血量，MonsterState中的血量保存在内存中
    protected void TakeDamage(MonsterState state, int damage, GameObject attacker)
    {
        // CharacterStateData中手动设置IsDie为True时
        // 游戏时再次收到伤害不会进入if判断
        if (state.isDie || damage <= 0) return;

        print("CharacterData: " + name + " " + state.health);
        state.health = Mathf.Max(state.health - damage, 0);
        print("CharacterData: " + name + " " + state.health);
        OnTakeDamageEvent?.Invoke(damage, state.health, 
            state.monsterStateInfo.maxHealth);

        if (state.health == 0)
        {
            state.SetDie();
            OnDieEvent?.Invoke(attacker);

            OnDieEvent = null;
        }
    }

    // 修改PlayerStateInfo中的血量，PlayerStateInfo数据会写入外存
    protected void TakeDamage(PlayerStateInfo stateInfo,
        int damage, GameObject attacker)
    {
        // CharacterStateData中手动设置IsDie为True时
        // 游戏时再次收到伤害不会进入if判断
        if (stateInfo.isDie || damage <= 0) return;

        print("CharacterData: " + name + " " + stateInfo.health);
        stateInfo.health = Mathf.Max(stateInfo.health - damage, 0);
        print("CharacterData: " + name + " " + stateInfo.health);
        OnTakeDamageEvent?.Invoke(damage, stateInfo.health, stateInfo.maxHealth);

        if (stateInfo.health == 0)
        {
            stateInfo.SetDie();
            OnDieEvent?.Invoke(attacker);

            OnDieEvent = null;
        }
    }

    protected void Recovery(PlayerStateInfo statInfo, int recovery)
    {
        if (statInfo.isDie || recovery < 0) return;

        statInfo.health = Mathf.Min(statInfo.health + recovery,
            statInfo.maxHealth);

        OnRecoveryEvent?.Invoke(recovery, statInfo.health,
            statInfo.maxHealth);
    }

    protected void Recovery(MonsterState state, int recovery)
    {
        if (state.isDie || recovery < 0) return;

        state.health = Mathf.Min(state.health + recovery, 
            state.monsterStateInfo.maxHealth);

        OnRecoveryEvent?.Invoke(recovery, state.health, 
            state.monsterStateInfo.maxHealth);
    }
}
