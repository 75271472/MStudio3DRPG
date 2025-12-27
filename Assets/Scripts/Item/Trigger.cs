using UnityEngine;
using UnityEngine.TextCore.Text;

public class Trigger : MonoBehaviour, IInteractable
{
    [SerializeField] private int triggerId;
    [SerializeField] private string triggerStr;

    public void SetTrigger(int triggerId, string triggerStr)
    {
        this.triggerId = triggerId;
        this.triggerStr = triggerStr;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CanInteract()) return;
        if (!other.CompareTag("Player")) return;

        EventTrigger();
    }

    private void EventTrigger()
    {
        // ∞Û∂® ‰»Î∞¥º¸
        UIManager.Instance.ShowPanel<InputActionPanel>().AddInput(
            "Survery", () => {
                PlayerManager.Instance.PlayerData.PlayerDialogueTrigger.
                    ConditionDialogueTrigger(triggerId, () => 
                        QuestManager.Instance.TriggerRequireUpdate(
                        triggerStr, true));
                UIManager.Instance.HidePanel<InputActionPanel>();
            });

        UIManager.Instance.GetPanel<InputActionPanel>().SetPosNextToPlayer();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        UIManager.Instance.HidePanel<InputActionPanel>();
    }

    public bool CanInteract()
    {
        if (gameObject.TryGetComponent<QuestCondition>(out var condition))
            return condition.CheckCondition();
        return true;
    }
}
