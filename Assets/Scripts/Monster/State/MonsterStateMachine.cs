using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(MonsterPatroller))]
public class MonsterStateMachine : CharacterStateMachine
{
    public int Id { get; private set; }
    public MonsterTargeter MonsterTargeter { get; private set; }
    public MonsterPatroller MonsterPatroller { get; private set; }
    public EnemyTarget EnemyTarget { get; private set; }
    public MonsterStateInfo MonsterStateSO { get; private set; }
    public MonsterMoveInfo MonsterMoveSO { get; private set; }
    public AttackComboList MonsterComboList { get; private set; }
    public EMonsterMoveType MonsterMoveType { get; private set; }

    public bool IsActiveTarget => MonsterTargeter != null && MonsterTargeter.IsActive;

    public void MonsterStateMachineInit(ICharacter character, int id)
    {
        Id = id;

        base.CharacterStateMachineInit(character);
    }

    protected override void GetComponentInit()
    {
        base.GetComponentInit();

        MonsterTargeter = GetComponentInChildren<MonsterTargeter>();
        MonsterPatroller = GetComponent<MonsterPatroller>();
        EnemyTarget = GetComponent<EnemyTarget>();

        MonsterStateSO = MonsterManager.Instance.GetMonster(Id).
            MonsterData.MonsterStateInfo;
        MonsterMoveType = MonsterManager.Instance.GetMonster(Id).
            MonsterData.MoveType;
        MonsterMoveSO = MonsterManager.Instance.GetMonster(Id).
            MonsterData.MonsterMoveInfo;
        MonsterComboList = MonsterManager.Instance.GetMonster(Id).
            MonsterData.AttackComboList;
    }

    protected override void ComponentInit()
    {
        base.ComponentInit();

        WeaponHandler.UpdateWeaponLogicList(Character);
        MonsterTargeter.MonsterTargeterInit(Id);
        MonsterPatroller.MonsterPatrollerInit(Id);
        EnemyTarget.EnemyTargetInit(Id);

        SwitchState(new MonsterGuardState(this));
    }

    public override void Pause()
    {
        IsPause = true;
        // TODO:在暂停状态强制转换为默认的Gurad状态
        // 当Monster收到攻击时调用Pause暂停状态机，
        // Gurad会覆盖GetHit受击状态
        SwitchState(new MonsterGuardState(this));
    }

    public override void UnPause()
    {
        IsPause = false;
    }

    public override void SwitchState(State newState)
    {
        //Debug.Log(
        //    (currentState?.GetType().Name ?? "null") +
        //    " to " +
        //    (newState?.GetType().Name ?? "null")
        //);

        base.SwitchState(newState);
    }

    protected override void OnTakeDamageHandler(int damage, GameObject attacker)
    {
        // 判断是否在TargetList列表中
        //if (!MonsterTargeter.ContainsTarget(attacker)) return;

        // 受击时切换到GetHit状态
        //SwitchState(new MonsterGetHitState(this));
    }

    protected override void OnDieHandler(GameObject attacker)
    {
        if (attacker.TryGetComponent<PlayerManager>(out var player))
        {
            player.PlayerData.UpdateExp(MonsterStateSO.point);
        }

        MonsterTargeter.SetDisable();
        SwitchState(new MonsterDieState(this));
    }
}
