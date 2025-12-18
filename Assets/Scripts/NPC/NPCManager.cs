using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCManager : MonoBehaviourManager<NPCManager>
{
    [field: SerializeField] private List<NPCController> NPCList;
    private Dictionary<int, NPCController> NPCDict =
        new Dictionary<int, NPCController>();

    public void NPCManagerInit()
    {
        NPCDict.Clear();
        NPCList = FindObjectsOfType<NPCController>().ToList();
        for (int i = 0; i < NPCList.Count; i++)
        {
            NPCDict.Add(i, NPCList[i]);
            NPCList[i].NPCControllerInit(i);
        }
    }

    public override void Init()
    {
        base.Init();
        if (IsNotSubManagerInit) return;

        NPCManagerInit();
    }
}
