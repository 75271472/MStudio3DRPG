using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EMonsterMoveType
{
    Guard,
    Patrol,
}

public class MonsterMoveInfo : CharacterMoveInfo
{
    public EMonsterMoveType moveType;
    public float chaseSpeed;
    public float waitTime;
}
