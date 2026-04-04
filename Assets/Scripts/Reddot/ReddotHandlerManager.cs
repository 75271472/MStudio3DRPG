using UnityEngine;

public class ReddotHandlerManager : MonoBehaviourManager<ReddotHandlerManager>
{
    public QuestReddotHandler QuestReddotHandler { get; private set; }
    public InventoryReddotHandler InventoryReddotHandler { get; private set; }

    public override void Init()
    {
        base.Init();
        if (IsNotSubManagerInit) return;
        print("ReddotHandlerManagerInit");
        QuestReddotHandler = new QuestReddotHandler();
        QuestReddotHandler.Init();
        InventoryReddotHandler = new InventoryReddotHandler();
        InventoryReddotHandler.Init();
    }

    private void Update()
    {
        ReddotManager.Instance.Update();
    }

    public override void Destroy()
    {
        base.Destroy();
        
        if (IsNotSubManagerInit) return;
        QuestReddotHandler?.OnDestroy();
        InventoryReddotHandler?.OnDestroy();
    }

    private void OnDestroy()
    {
        Destroy();
    }
}
