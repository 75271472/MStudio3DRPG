using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterHealth", 
    menuName = "CharacterModifier/CharacterHealth")]
public class CharacterHealthSO : CharacterModifierSO
{
    public override void AffectCharacter(ICharacter character, float value)
    {
        character.CharacterStateMachine.Health.Recovery((int)value);
    }
}
