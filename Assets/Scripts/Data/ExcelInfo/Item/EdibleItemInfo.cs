using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class EdibleItemInfo : ItemInfo, IDestroyableItem, IItemAction
{
    public List<ModifierInfo> modifierInfos = new List<ModifierInfo>();

    public string ActionName => "Consume";

    public bool PerformAction(PerformActionInfo performActionInfo)
    {
        foreach (ModifierInfo modifierData in modifierInfos) 
        {
            ModifierFactory.ApplyModifier(
                modifierData.modifierType, modifierData.value, 
                performActionInfo.character);
        }

        return true; 
    }

    public override string GetDescription()
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(description);
        //Debug.Log(modifierInfos.Count);
        foreach (var modifierInfo in modifierInfos)
        {
            stringBuilder.AppendLine(
                $"{modifierInfo.modifierType}: {modifierInfo.value}");
        }

        return stringBuilder.ToString().Trim();
    }
}
