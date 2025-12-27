using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[System.Serializable]
public class DialoguePiece
{
    public int id;
    [TextArea] public string text;

    public List<DialogueOption> dialogueOptionList;
    public int  targetId;
}

public class ConditionDialoguePiece
{
    public int id;
    public string text;
    public int targetId;
}

public class ConditionInfo
{
    public int conditionId;
    public string description;
    public bool isTriggered;
}
