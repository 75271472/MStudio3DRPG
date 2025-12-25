using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterData : CharacterData
{
    private MonsterState monsterState;

    // MonsterController在MonsterManager列表中的id，
    // 方便通过id调用MosnterController中的组件
    public int Id { get; private set; }
    
    public MonsterInfo MonsterInfo { get; private set; }
    public string MonsterName => MonsterInfo.monsterName;
    public MonsterStateInfo MonsterStateInfo => MonsterInfo.monsterStateInfo;
    public MonsterMoveInfo MonsterMoveInfo => MonsterInfo.monsterMoveInfo;
    public override AttackInfo AttackInfo => MonsterInfo.monsterAttackInfo;

    [field: SerializeField] public EMonsterMoveType MoveType { get; set; }

    // Monster类型id，用于获取指定类型的MonsterInfo
    [field: SerializeField] public int MonsterInfoId { get; private set; }

    public MonsterState MonsterState
    {
        get
        {
            if (monsterState == null)
                monsterState = new MonsterState(MonsterStateInfo);
            return monsterState;
        }
    }

    public override void CharacterDataInit(ICharacter character)
    {
        MonsterInfo = DataManager.Instance.MonsterInfoList[MonsterInfoId];

        base.CharacterDataInit(character);

        // 重新动态生成一份StateSO数据
        //MonsterStateSO = Instantiate(MonsterStateSO);
    }

    public void MonsterDataInit(int id)
    {
        CharacterDataInit(Character);

        this.Id = id;
    }

    public override void OnTakeDamageHandler(int damage, GameObject attacker)
    {
        base.TakeDamage(MonsterState, damage, attacker);
    }

    public override void OnRecoveryHandler(int recovery)
    {
        base.Recovery(MonsterState, recovery);
    }

    public override CharacterStateData GetCharacterStateData()
    {
        throw new System.NotImplementedException();
    }
}
