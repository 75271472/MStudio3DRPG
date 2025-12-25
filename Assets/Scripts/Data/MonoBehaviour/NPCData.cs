using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCData : CharacterData
{
    [field: SerializeField] private int NPCIdInfoId;

    public DialogueTrigger DialogueTrigger { get; private set; }

    public NPCInfo NPCInfo { get; private set; }
    public string NPCName => NPCInfo.npcName;

    public override void CharacterDataInit(ICharacter character)
    {
        NPCInfo = DataManager.Instance.NPCInfoList[NPCIdInfoId];

        DialogueTrigger = GetComponentInChildren<DialogueTrigger>();
        DialogueTrigger.DialogueTriggerInit(NPCInfo.characterDialogueId, NPCInfo.npcName);

        base.CharacterDataInit(character);
    }

    public override CharacterStateData GetCharacterStateData()
    {
        throw new System.NotImplementedException();
    }

    public override void OnRecoveryHandler(int recovery)
    {
        throw new System.NotImplementedException();
    }

    public override void OnTakeDamageHandler(int damage, GameObject attacker)
    {
        throw new System.NotImplementedException();
    }
}
