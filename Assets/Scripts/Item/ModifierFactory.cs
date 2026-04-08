using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierFactory
{     
    public static void ApplyModifier(string modifierType, float value, 
        ICharacter character)
    {
        string fullPath = ResourcesManager.Instance.GetFullUrl(
            GetModifierPath(modifierType));
        IResource modifierRes = ResourcesManager.Instance.Load(fullPath, false);

        CharacterModifierSO modifier = modifierRes.GetAsset<CharacterModifierSO>();
        modifier.AffectCharacter(character, value);

        ResourcesManager.Instance.Unload(modifierRes);
    }

    private static string GetModifierPath(string modifierType)
    {
        return $"{DataManager.MODIFIER}Character{modifierType}SO";
    }
}
