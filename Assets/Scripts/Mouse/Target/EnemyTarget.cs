using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTarget : MouseTarget
{
    public override EMouseTarget MouseTargetType => EMouseTarget.Enemy;
    public int Id {  get; private set; }

    public void EnemyTargetInit(int id) => this.Id = id;

    public bool CheckMonster()
    {
        MonsterStateMachine monsterStateMachine = 
            MonsterManager.Instance.GetMonster(Id).MonsterStateMachine;

        if (monsterStateMachine != null)
            return monsterStateMachine.IsActiveTarget;
        
        return false;
    }
}