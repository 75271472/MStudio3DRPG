using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacter
{
    public CharacterStateMachine CharacterStateMachine { get; }
    public CharacterData CharacterData { get; }
    public CharacterUI CharacterUI { get; }
    public GameObject CharacterGameObject { get; }

    public event Action<ICharacter> OnCharacterDieEvent;
}
