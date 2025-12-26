using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueTrigger : DialogueTrigger
{
    public void DialogueTrigger()
    {
        DialogueManager.Instance.BeginConditionDialogue(characterId, profileTexture, 
            characterName);
    }
}
