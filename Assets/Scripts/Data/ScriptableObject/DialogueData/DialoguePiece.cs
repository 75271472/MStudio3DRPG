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
    public int questId = -1;

    //public void DialoguePieceInit()
    //{
    //    if (questId != -1)
    //    {
    //        foreach (var option in dialogueOptionList)
    //        {
    //            option.OnSelectEvent += OnSelectHandler;
    //        }
    //    }
    //}

    //public void OnSelectHandler(bool isTakeTask)
    //{
    //    if (!isTakeTask) return;

    //    QuestManager.Instance.SelectQuest(questId);
    //}
}

public class ConditionDialoguePiece
{
    public int id;
    public string text;
    public int targetId;
}
