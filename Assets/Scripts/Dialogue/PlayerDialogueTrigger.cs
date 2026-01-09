using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueTrigger : DialogueTrigger
{
    public void ConditionDialogueTrigger(int triggerId, Action callback)
    {
        DialogueManager.Instance.BeginConditionDialogue(characterId, triggerId,
            profileTexture, characterName, callback);
    }

    public void ConditionDialogueTrigger(int triggerId)
    {
        ConditionDialogueTrigger(triggerId, null);
    }
}
