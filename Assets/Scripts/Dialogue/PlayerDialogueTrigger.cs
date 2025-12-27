using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueTrigger : DialogueTrigger
{
    public void ConditionDialogueTrigger(int triggerId, Action callback = null)
    {
        DialogueManager.Instance.BeginConditionDialogue(characterId, triggerId,
            profileTexture, characterName, callback);
    }
}
