using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierFactory
{
    private static Dictionary<string, CharacterModifierSO> modifierDict = 
        new Dictionary<string, CharacterModifierSO>();

    public static void ApplyModifier(string modifierType, float value, 
        ICharacter character)
    {
        GetModifier(modifierType).AffectCharacter(character, value);
    }

    private static IAffectCharacter GetModifier(string modifierType)
    {
        if (!modifierDict.ContainsKey(modifierType))
        {
            CharacterModifierSO modifier = modifierType switch
            {
                "Health" =>
                Resources.Load<CharacterModifierSO>(GetModifierPath(modifierType)),
                _ => null,
            };

            modifierDict.Add(modifierType, modifier);
        }

        return modifierDict[modifierType];
    }

    private static string GetModifierPath(string modifierType)
    {
        return $"{DataManager.MODIFIER}Character{modifierType}SO";
    }
}
