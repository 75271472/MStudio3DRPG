/// <summary>
/// 状态与对话的映射
/// 一个任务对应一Binding
/// </summary>
[System.Serializable]
public class QuestDialogueBinding
{
    public int characterId;
    public int questId;              // 绑定的任务ID

    public int notAcceptedDialogueId = -1;
    public int startDialogueId = -1;
    public int completeDialogueId = -1;

    public int GetDialogueIndexByState(EQuestState state)
    {
        return state switch
        {
            EQuestState.NotAccepted => notAcceptedDialogueId,
            EQuestState.Start => startDialogueId,
            EQuestState.Complete => completeDialogueId,
            _ => -1
        };
    }
}