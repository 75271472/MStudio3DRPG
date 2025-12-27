using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAffectCharacter
{
    public void AffectCharacter(ICharacter character, float value);
}

public abstract class CharacterModifierSO : ScriptableObject, IAffectCharacter
{
    public abstract void AffectCharacter(ICharacter character, float value);
}

[SerializeField]
public class ModifierInfo
{
    public string modifierType;
    public float value;
}
