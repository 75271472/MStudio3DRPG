using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Excel表中对应的 NPC ID")]
    [SerializeField] private int npcId;

    [Header("UI Display")]
    [SerializeField] private Texture profileTexture;
    [SerializeField] private string npcName;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 绑定输入按键
        UIManager.Instance.ShowPanel<InputActionPanel>().AddInput(
            "Talk", () => {
                // 修改点：不再自己计算 Index，而是直接把 NPC ID 扔给 Manager
                // Manager 会去 DataManager 里查表计算
                DialogueManager.Instance.BeginDialogue(npcId, profileTexture, npcName);

                UIManager.Instance.HidePanel<InputActionPanel>();
            });

        UIManager.Instance.GetPanel<InputActionPanel>().SetPosNextToPlayer();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        UIManager.Instance.HidePanel<InputActionPanel>();
    }
}