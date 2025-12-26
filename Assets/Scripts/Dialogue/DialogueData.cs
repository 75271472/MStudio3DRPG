using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueData : MonoBehaviour
{
    // 运行时状态：当前正在播放的对话链（如果有后续跳转）
    // currentPiece也可用于判断当前是否处于播放状态
    private DialoguePiece currentPiece;
    private int startPieceId = -1;
    private int questId = -1;

    // 事件：通知 Manager 更新 UI
    // 对话数据 键为DialoguePiece.text，值为DialogueOption.text
    public event Action<KeyValuePair<string, List<string>>?> OnUpdateDialogueEvent;

    public void DialogueDataInit()
    {
        // 绑定 Manager 的监听 (反向绑定也可以，看你架构习惯，这里假设 Manager 会来订阅)
        // 在 Manager 的 Init 里其实并没有订阅这个 Event，建议在 Manager 的 PrepareData 里订阅
        // 或者像上面 Manager 代码里那样，直接把 Manager 的 Handler 传进来，或者暴露 Event 给 Manager
    }

    public void StartDialogue(DialoguePiece piece, int questId)
    {
        if (piece == null) return;

        this.currentPiece = piece;
        this.startPieceId = piece.id;
        this.questId = questId;
        UpdateUI();
    }

    public void EndDialogue()
    {
        currentPiece = null;
        startPieceId = -1;
    }

    public void UpdateDialogue(int optionIndex)
    {
        if (currentPiece == null) return;

        // 1. 处理选项逻辑
        int nextPieceId = -1;

        // 选择了DialoguePiece中的Option
        if (optionIndex != -1 && currentPiece.dialogueOptionList != null &&
            currentPiece.dialogueOptionList.Count > optionIndex)
        {
            var option = currentPiece.dialogueOptionList[optionIndex];

            // 接任务逻辑
            if (option.isTakeTask)
            {
                // 这里的 questId 最好存在 Option 里或者 Piece 里
                QuestManager.Instance.SelectQuest(questId);
            }

            if (option.targetId != -1)
            {
                nextPieceId = startPieceId + option.targetId;
            }
        }
        else if (currentPiece.targetId != -1)
        {
            // 点击继续：如果没有选项，则直接赋值
            // targetId = -1，结束对话
            // target != -1，进行跳转
            nextPieceId = startPieceId + currentPiece.targetId;
        }

        // 2. 跳转到下一句
        if (nextPieceId != -1)
        {
            // 去 DataManager 查下一句
            if (DataManager.Instance.DialoguePieceMap.
                TryGetValue(nextPieceId, out var piece))
            {
                currentPiece = piece;
                UpdateUI();
                return;
            }
        }

        // 3. 没有下一句，结束对话
        // 不能直接调用EndDialogue，否则会反复调用DialogueManager中的EndDialogue
        OnUpdateDialogueEvent?.Invoke(null);
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