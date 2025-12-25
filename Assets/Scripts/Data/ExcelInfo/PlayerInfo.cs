using System.Collections.Generic;
using System.Threading;
using Unity.AI.Navigation;

public abstract class CharacterIdInfo
{
    public int id;
    public string name;
    public int moveInfoId;
    public int stateInfoId;
    public int attackInfoId;
}

public class PlayerIdInfo : CharacterIdInfo
{
    public int defaultWeaponInfoId;
    public int characterDialogueId;
    //public int weaponInfoId;
}

public class MonsterIdInfo : CharacterIdInfo
{

}

public class NPCIdInfo
{
    public int id;
    public string name;
    public int characterDialogueId;
}

public class PlayerInfo
{
    public string playerName;
    public PlayerMoveInfo playerMoveInfo;
    public PlayerStateInfo playerStateInfo;
    public CharacterTransInfo playerTransInfo;
    public AttackInfo playerAttackInfo;
    public InventoryItemInfo defaultWeaponInfo;
    public int characterDialogueId;
    //public InventoryItemInfo playerWeaponInfo;
    //public InventoryItemInfo playerShieldInfo;

    public PlayerInfo() { }

    public PlayerInfo(string name, PlayerMoveInfo playerMoveInfo, 
        PlayerStateInfo playerStateInfo, CharacterTransInfo playerTransInfo, 
        AttackInfo playerAttackInfo, InventoryItemInfo defaultWeaponInfo, 
        int characterDialogueId)
    {
        this.playerName = name;
        this.playerMoveInfo = playerMoveInfo;
        this.playerStateInfo = playerStateInfo;
        this.playerTransInfo = playerTransInfo; 
        this.playerAttackInfo = playerAttackInfo;
        this.defaultWeaponInfo = defaultWeaponInfo;
        this.characterDialogueId = characterDialogueId;
        //this.playerWeaponInfo = playerWeaponInfo;
        //this.playerShieldInfo = playerShieldInfo;
    }
}

public class MonsterInfo
{
    public string monsterName;
    public MonsterMoveInfo monsterMoveInfo;
    public MonsterStateInfo monsterStateInfo;
    public AttackInfo monsterAttackInfo;

    public MonsterInfo() { }

    public MonsterInfo(string name, MonsterMoveInfo monsterMoveInfo, 
        MonsterStateInfo monsterStateInfo, AttackInfo monsterAttackInfo)
    {
        this.monsterName = name;
        this.monsterMoveInfo = monsterMoveInfo;
        this.monsterStateInfo = monsterStateInfo;
        this.monsterAttackInfo = monsterAttackInfo;
    }
}

public class NPCInfo
{
    public string npcName;
    public int characterDialogueId;

    public NPCInfo(string npcName , int characterDialogueId)
    {
        this.npcName = npcName;
        this.characterDialogueId = characterDialogueId;
    }
}