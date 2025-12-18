using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterManager : MonoBehaviourManager<MonsterManager>
{
    [field: SerializeField] private List<MonsterController> MonsterList;
    private Dictionary<int, MonsterController> MonsterDict = 
        new Dictionary<int, MonsterController>();

    public event Action<ICharacter> OnMonsterDieEvent;

    public void MonsterManagerInit()
    {
        MonsterDict.Clear();
        MonsterList = FindObjectsOfType<MonsterController>().ToList();
        for (int i = 0; i < MonsterList.Count; i++)
        {
            MonsterDict.Add(i, MonsterList[i]);
            MonsterList[i].MonsterControllerInit(i);
            MonsterList[i].OnCharacterDieEvent += OnMonsterDieHandler;
        }
    }

    public override void Init()
    {
        base.Init();
        if (IsNotSubManagerInit) return;

        MonsterManagerInit();
    }

    public MonsterController GetMonster(int id)
    {
        if (MonsterDict.ContainsKey(id))
            return MonsterDict[id];
        else
            return null;
    }

    private void OnMonsterDieHandler(ICharacter character)
    {
        OnMonsterDieEvent?.Invoke(character);
    }
}
