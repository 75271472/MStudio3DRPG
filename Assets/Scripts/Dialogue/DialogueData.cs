using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueData : MonoBehaviour
{
    // 运行时状态：当前正在播放的对话链（如果有后续跳转）
    private DialoguePiece currentPiece;
    private int startPieceId;

    // 事件：通知 Manager 更新 UI
    public event Action<KeyValuePair<string, List<string>>?> OnUpdateDialogueEvent;

    public void DialogueDataInit()
    {
        // 绑定 Manager 的监听 (反向绑定也可以，看你架构习惯，这里假设 Manager 会来订阅)
        // 在 Manager 的 Init 里其实并没有订阅这个 Event，建议在 Manager 的 PrepareData 里订阅
        // 或者像上面 Manager 代码里那样，直接把 Manager 的 Handler 传进来，或者暴露 Event 给 Manager
    }

    public void StartDialogue(DialoguePiece piece)
    {
        if (piece == null) return;

        this.currentPiece = piece;
        this.startPieceId = piece.id;
        UpdateUI();
    }

    public void EndDialogue()
    {
        OnUpdateDialogueEvent?.Invoke(null);
        currentPiece = null;
        startPieceId = 0;
    }

    public void UpdateDialogue(int optionIndex)
    {
        if (currentPiece == null) return;

        // 1. 处理选项逻辑
        int nextPieceId = -1;

        if (optionIndex != -1 && currentPiece.dialogueOptionList != null &&
            currentPiece.dialogueOptionList.Count > optionIndex)
        {
            var option = currentPiece.dialogueOptionList[optionIndex];

            // 接任务逻辑
            if (option.isTakeTask)
            {
                // 这里的 questId 最好存在 Option 里或者 Piece 里
                QuestManager.Instance.SelectQuest(currentPiece.questId);
            }

            if (option.targetId != -1)
            {
                nextPieceId = startPieceId + option.targetId;
            }
        }
        else
        {
            // 点击继续：如果没有选项，通常 targetId 会存在 Piece 的某个字段，或者没有后续
            // 如果你的设计是 Piece 只有选项跳转，没有默认跳转，那这里就结束了
            // 或者你可以给 DialoguePiece 加一个 defaultTargetId
            // 假设：如果没有选项，就结束了，除非你有连续对话的设计
            nextPieceId = currentPiece.id + 1;
        }

        // 2. 跳转到下一句
        if (nextPieceId != -1)
        {
            // 去 DataManager 查下一句
            var nextPiece = DataManager.Instance.DialoguePieceMap[nextPieceId];
            if (nextPiece != null)
            {
                currentPiece = nextPiece;
                UpdateUI();
                return;
            }
        }

        // 3. 没有下一句，结束对话
        EndDialogue();
    }

    private void UpdateUI()
    {
        List<string> options = new List<string>();
        if (currentPiece.dialogueOptionList != null)
        {
            foreach (var opt in currentPiece.dialogueOptionList)
            {
                options.Add(opt.text);
            }
        }

        var pair = new KeyValuePair<string, List<string>>(currentPiece.text, options);
        OnUpdateDialogueEvent?.Invoke(pair);
    }
}