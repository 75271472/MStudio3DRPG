public class QuestReddotHandler
{
    private const string QUEST_REDDOT_PREFIX = "Main/BagTask/Quest/";

    public void Init()
    {
        QuestManager.Instance.OnStartQusetEvent += OnQuestStarted;
        QuestManager.Instance.OnFinishedQuestEvent += OnQuestFinished;
    }

    public void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnStartQusetEvent -= OnQuestStarted;
            QuestManager.Instance.OnFinishedQuestEvent -= OnQuestFinished;
        }
    }

    private void OnQuestStarted(Quest quest)
    {
        ReddotManager.Instance.ChangeValue(QUEST_REDDOT_PREFIX + quest.id, 1);
    }

    private void OnQuestFinished(Quest quest)
    {
        ReddotManager.Instance.ChangeValue(QUEST_REDDOT_PREFIX + quest.id, 0);
    }

    public void MarkQuestRead(int questId)
    {
        ReddotManager.Instance.ChangeValue(QUEST_REDDOT_PREFIX + questId, 0);
    }
}
