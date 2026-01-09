using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MonsterStateMachine), typeof(MonsterData))]
public class MonsterController : MonoBehaviour, ICharacter
{
    public int Id { get; private set; }
    public MonsterStateMachine MonsterStateMachine { get; private set; }
    public MonsterData MonsterData { get; private set; }
    public MonsterUI MonsterUI { get; private set; }

    public CharacterStateMachine CharacterStateMachine => MonsterStateMachine;
    public CharacterData CharacterData => MonsterData;
    public CharacterUI CharacterUI => MonsterUI;
    public GameObject CharacterGameObject => gameObject;

    public event Action<ICharacter> OnCharacterDieEvent;

    public void MonsterControllerInit(int id)
    {
        Id = id;

        MonsterStateMachine = GetComponent<MonsterStateMachine>();
        MonsterData = GetComponent<MonsterData>();
        MonsterUI = GetComponent<MonsterUI>();

        // 先数据初始化，其次StateMachine初始化
        MonsterData.MonsterDataInit(id);
        MonsterStateMachine.MonsterStateMachineInit(this, id);
        MonsterUI.MonsterUIInit(this, MonsterData, id);

        MonsterData.HealthEventRegist(MonsterStateMachine.Health);
        // 当Player对Monster造成伤害时，摄像机抖动
        MonsterData.OnTakeDamageEvent += (a, b, c) =>
            CameraManager.Instance.ShakeCamera(0.8f, 0.1f);
        MonsterStateMachine.DieDataRegist(MonsterData);
        MonsterUI.TakeDamageRegist(MonsterData);

        MonsterData.OnDieEvent += OnMonsterDieHandler;
    }

    private void OnMonsterDieHandler(GameObject obj)
    {
        //print(name + " Die");
        OnCharacterDieEvent?.Invoke(this);
    }
}
