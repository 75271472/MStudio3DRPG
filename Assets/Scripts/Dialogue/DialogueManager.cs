using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviourManager<DialogueManager>
{
    public DialogueData DialogueData { get; private set; }
    public ConditionDialogueData ConditionDialogueData { get; private set; }
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
        ConditionDialogueData = GetComponent<ConditionDialogueData>();

        DialogueData.OnUpdateDialogueEvent += OnUpdateDialogueHandler;
        ConditionDialogueData.OnUpdateDialogueEvent += OnUpdateDialogueHandler;

        DialogueData.DialogueDataInit();
        ConditionDialogueData.DialogueDataInit();
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
    /// 触发内心独白，需要传入triggerId指定触发哪一个内心独白
    /// </summary>
    /// <param name="npcId"></param>
    /// <param name="triggerId"></param>
    /// <param name="profile"></param>
    /// <param name="name"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public bool BeginConditionDialogue(int npcId, int triggerId, Texture profile, 
        string name, Action action = null)
    {
        int pieceId = CheckStartPieceId(npcId, triggerId);

        if (pieceId == -1)
        {
            Debug.LogWarning($"NPC {npcId} 没有可播放的对话");
            return false;
        }

        // 2. 从 DataManager 获取实际数据对象
        ConditionDialoguePiece piece = DataManager.Instance.
            GetConditionDialoguePieceById(pieceId);
        if (piece == null) return false;

        // 3. 设置 UI 并启动会话
        DialoguePanel.SetProfile(profile, name);
        DialoguePanel.ShowMe();

        // 注意：DialogueData 现在应该只负责播放，不负责查找
        ConditionDialogueData.StartDialogue(piece, action);

        return true;
    }

    /// <summary>
    /// 新的入口：只需要 NPC ID
    /// </summary>
    public void BeginDialogue(int npcId, Texture profile, string name)
    {
        // 1. 计算应该播放哪个 Piece ID
        Tuple<int, int> pieceTuple = CalculateStartPieceId(npcId);

        if (pieceTuple == null)
        {
            Debug.LogWarning($"NPC {npcId} 没有可播放的对话");
            return;
        }

        // 2. 从 DataManager 获取实际数据对象
        DialoguePiece piece = DataManager.Instance.GetDialoguePieceById(pieceTuple.Item1);
        if (piece == null) return;

        // 3. 设置 UI 并启动会话
        DialoguePanel.SetProfile(profile, name);
        DialoguePanel.ShowMe();

        // 注意：DialogueData 现在应该只负责播放，不负责查找
        DialogueData.StartDialogue(piece, pieceTuple.Item2);
    }

    public void SaveConditionInfo() => 
        DataManager.Instance.SaveConditionInfoList();

    private Tuple<int, int> CalculateStartPieceId(int npcId)
    {
        // 1. 获取该 NPC 所有的任务绑定
        if (!DataManager.Instance.BindingMap.TryGetValue(npcId, out var bindingList))
        {
            // 如果没有绑定任务，可能有默认对话逻辑，这里暂时返回 -1 或默认ID
            return null;
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
                return new Tuple<int, int>(candidateId, binding.questId);
            }
        }

        // 3. 如果所有任务状态都没话可说，返回保底对话
        return null;
    }

    private int CheckStartPieceId(int npcId, int triggerId)
    {
        // 1. 获取该 NPC 所有的任务绑定
        if (!DataManager.Instance.ConditionBindingMap.TryGetValue
            (npcId, out var bindingList))
        {
            return -1;
        }

        ConditionDialogueBinding binding = DataManager.Instance.
            ConditionBindingMap[npcId][triggerId];

        ConditionInfo conditionInfo = DataManager.Instance.
            ConditionInfoList[binding.conditionId];

        if (!conditionInfo.isTriggered)
        {
            // 标记ConditionInfo.isTriggered为已触发
            // 同时DataManager中的数据也会被记录为已触发
            conditionInfo.isTriggered = true;
            return binding.dialogueId;
        }

        return -1;
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
        DialogueData.EndDialogue();
        ConditionDialogueData.EndDialogue();
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
        ConditionDialogueData.UpdateDialogue();
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

        // 当没有Option，hasNext = true，直接显示Next按钮
        bool hasNext = dialoguePair.Value.Value == null ||
            dialoguePair.Value.Value.Count == 0;

        DialoguePanel.SetDialogueContent(dialoguePair.Value.Key, hasNext);
        DialoguePanel.SetDialogueOption(dialoguePair.Value.Value);
    }
}
