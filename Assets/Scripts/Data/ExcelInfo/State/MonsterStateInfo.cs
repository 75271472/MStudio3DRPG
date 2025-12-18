public class MonsterStateInfo : CharacterStateInfo
{
    public int monsterId;
    public int point;
}

public class MonsterState : CharacterState
{
    public MonsterStateInfo monsterStateInfo;

    public MonsterState(MonsterStateInfo monsterStateInfo) : 
        base(monsterStateInfo)
    {
        this.monsterStateInfo = monsterStateInfo;
    }
}