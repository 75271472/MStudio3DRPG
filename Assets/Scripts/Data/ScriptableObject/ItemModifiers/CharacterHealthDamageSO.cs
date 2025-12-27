using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterHealthDamage",
    menuName = "CharacterModifier/CharacterHealthDamage")]
public class CharacterHealthDamageSO : CharacterModifierSO
{
    public override void AffectCharacter(ICharacter character, float value)
    {
        character.CharacterStateMachine.Health.SelfDamage((int)value, 
            character.CharacterGameObject);
    }
}
