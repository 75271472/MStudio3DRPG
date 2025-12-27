using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour, IInteractable
{
    [field: SerializeField] public string SceneName { get; private set; }
    [field: SerializeField] public int PortalId { get; private set; }
    [field: SerializeField] public string TargetSceneName { get; private set; }
    [field: SerializeField] public int TargetPortalID { get; private set; }

    public PortalPoint PortalPoint { get; private set; }

    public void PortalInit()
    {
        PortalPoint = GetComponentInChildren<PortalPoint>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CanInteract()) return;
        if (!other.CompareTag("Player")) return;
        // 当SceneName为空时不显现InputActionPanel，不进行传送
        if (string.IsNullOrEmpty(SceneName)) return;

        UIManager.Instance.ShowPanel<InputActionPanel>().AddInput(
            "Enter", PortalTransform);
        UIManager.Instance.GetPanel<InputActionPanel>().SetPosNextToPlayer();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (string.IsNullOrEmpty(SceneName)) return;

        UIManager.Instance.HidePanel<InputActionPanel>();
    }

    private void PortalTransform()
    {
        //print("OnOperate Invoke");
        UIManager.Instance.HidePanel<InputActionPanel>();
        PortalManager.Instance.PortalEnter(TargetSceneName, TargetPortalID);
    }

    public bool CanInteract()
    {
        if (gameObject.TryGetComponent<QuestCondition>(out var condition))
            return condition.CheckCondition();
        return true;
    }
}
