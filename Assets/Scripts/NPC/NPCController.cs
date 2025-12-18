using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, ICharacter
{
    public int Id { get; private set; }
    public NPCStateMachine NPCStateMachine { get; private set; }
    public NPCData NPCData { get; private set; }
    public NPCUI NPCUI { get; private set; }

    public CharacterStateMachine CharacterStateMachine => NPCStateMachine;
    public CharacterData CharacterData => NPCData;
    public CharacterUI CharacterUI => NPCUI;
    public GameObject CharacterGameObject => gameObject;

    public event Action<ICharacter> OnCharacterDieEvent;

    public void NPCControllerInit(int id)
    {
        Id = id;

        NPCStateMachine = GetComponent<NPCStateMachine>();
        NPCData = GetComponent<NPCData>();
        NPCUI = GetComponent<NPCUI>();

        NPCData.CharacterDataInit(this);
    }
}
