using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviourManager<DialogueManager>
{
    public DialogueData DialogueData { get; private set; }
    public DialoguePanel DialoguePanel { get; private set; }

    public void DialogueControllerInit()
    {
        PerpareData();
        PreparePanel();
    }

    public override void Init()
    {
        base.Init();

        if (IsNotSubManagerInit) return;
        
        DialogueControllerInit();
    }

    private void PerpareData()
    {
        DialogueData = GetComponent<DialogueData>();

        DialogueData.OnUpdateDialogueEvent += OnUpdateDialogueHandler;

        DialogueData.DialogueDataInit();
    }

    private void PreparePanel()
    {
        DialoguePanel = UIManager.Instance.ShowPanel<DialoguePanel>();
        DialoguePanel.HideMe();

        DialoguePanel.OnEndDialogueEvent += OnEndDialogueHandler;
        DialoguePanel.OnUpdateDialogueContentEvent += OnUpdateDialogueContentHandler;
        DialoguePanel.OnOptionSelectEvent += OnOptionSelectHandler;

        DialoguePanel.DialoguePanelInit();
    }

    /// <summary>
    /// 新的入口：只需要 NPC ID
    /// </summary>
    public void BeginDialogue(int npcId, Texture profile, string name)
    {
        // 1. 计算应该播放哪个 Piece ID
        int pieceId = CalculateStartPieceId(npcId);

        if (pieceId == -1)
        {
            Debug.LogWarning($"NPC {npcId} 没有可播放的对话");
            return;
        }

        // 2. 从 DataManager 获取实际数据对象
        DialoguePiece piece = DataManager.Instance.GetDialoguePieceById(pieceId);
        if (piece == null) return;

        // 3. 设置 UI 并启动会话
        DialoguePanel.SetProfile(profile, name);
        DialoguePanel.ShowMe();

        // 注意：DialogueData 现在应该只负责播放，不负责查找
        DialogueData.StartDialogue(piece);
    }

    private int CalculateStartPieceId(int npcId)
    {
        // 1. 获取该 NPC 所有的任务绑定
        if (!DataManager.Instance.BindingMap.TryGetValue(npcId, out var bindingList))
        {
            // 如果没有绑定任务，可能有默认对话逻辑，这里暂时返回 -1 或默认ID
            return GetDefaultDialogueId(npcId);
        }

        // 2. 遍历绑定，寻找最高优先级的有效对话
        // 建议按优先级排序，或者按列表顺序
        foreach (var binding in bindingList)
        {
            // 向 QuestManager 询问状态
            EQuestState state = QuestManager.Instance.GetQuestState(binding.questId);

            // 获取该状态对应的 Piece ID
            int candidateId = binding.GetDialogueIndexByState(state);

            // 如果找到了有效的对话ID (不是 -1)，直接返回
            // 这里实现了“任务状态拦截对话”的逻辑
            if (candidateId != -1)
            {
                return candidateId;
            }
        }

        // 3. 如果所有任务状态都没话可说，返回保底对话
        return GetDefaultDialogueId(npcId);
    }

    private int GetDefaultDialogueId(int npcId)
    {
        // 这里可以去查 NPCInfo 表里的 defaultDialogueId
        // 假设 DataManager.Instance.GetNPCDefaultDialogueId(npcId);
        return -1;
    }

    // 由UI层出传入用户信号时判断当前是否处于正在对话状态
    private void OnEndDialogueHandler()
    {
        DialoguePanel.HideMe();
    }

    private void OnUpdateDialogueContentHandler()
    {
        OnOptionSelectHandler();
    }

    private void OnOptionSelectHandler(int index = -1)
    {
        //print($"DialogueController: OnOptionSelectHandloer");

        // 重置选项
        DialoguePanel.ResetDialogueOption();
        DialogueData.UpdateDialogue(index);
    }

    private void OnUpdateDialogueHandler(
        KeyValuePair<string, List<string>>? dialoguePair)
    {
        //print($"{name} OnUpdateDialogueHandler");

        if (!dialoguePair.HasValue)
        {
            OnEndDialogueHandler();
            return;
        }

        bool hasNext = dialoguePair.Value.Value.Count == 0;
        DialoguePanel.SetDialogueContent(dialoguePair.Value.Key, hasNext);
        DialoguePanel.SetDialogueOption(dialoguePair.Value.Value);
    }
}
