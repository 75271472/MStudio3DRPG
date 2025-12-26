using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDialogueData : MonoBehaviour
{
    private ConditionDialoguePiece currentPiece;
    private int startPieceId;

    public bool IsPlaying => startPieceId != -1;
    public event Action<KeyValuePair<string, List<string>>?> OnUpdateDialogueEvent;

    public void DialogueDataInit()
    {

    }

    public void StartDialogue(ConditionDialoguePiece piece)
    {
        if (piece == null) return;

        this.currentPiece = piece;
        this.startPieceId = piece.id;
        UpdateUI();
    }

    public void EndDialogue()
    {
        currentPiece = null;
        startPieceId = -1;
    }

    public void UpdateDialogue()
    {
        if (currentPiece == null) return;

        // ½áÊø¶Ô»°
        if (currentPiece.targetId == -1)
        {
            OnUpdateDialogueEvent?.Invoke(null);
        }
        else
        {
            int nextPieceId = startPieceId + currentPiece.targetId;
            if (DataManager.Instance.ConditionDialoguePieceMap.
                TryGetValue(nextPieceId, out var piece))
            {
                currentPiece = piece;
                UpdateUI();
                return;
            }
        }
    }

    private void UpdateUI()
    {
        var pair = new KeyValuePair<string, List<string>>(currentPiece.text, null);
        OnUpdateDialogueEvent?.Invoke(pair);
    }
}
