using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    protected int characterId;
    protected string characterName;

    [SerializeField] protected Texture profileTexture;

    public virtual void DialogueTriggerInit(int characterId, string characterName)
    {
        this.characterId = characterId;
        this.characterName = characterName;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 绑定输入按键
        UIManager.Instance.ShowPanel<InputActionPanel>().AddInput(
            "Talk", () => {
                // 修改点：不再自己计算 Index，而是直接把 NPC ID 扔给 Manager
                // Manager 会去 DataManager 里查表计算
                DialogueManager.Instance.BeginDialogue(characterId, profileTexture, characterName);

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