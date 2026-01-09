using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health), typeof(WeaponHandler), typeof(ForceApplicator)),
    RequireComponent(typeof(Animator), typeof(NavMeshAgent)),
    RequireComponent(typeof(EffectApplicator))]
public abstract class CharacterStateMachine : StateMachine
{
    public Animator Animator { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public Health Health { get; private set; }
    public WeaponHandler WeaponHandler { get; private set; }
    public ForceApplicator ForceApplicator { get; private set; }
    public EffectApplicator EffectApplicator { get; private set; }
    public ICharacter Character { get; protected set; }

    //public Rigidbody Rigidbody { get; private set; }

    public void CharacterStateMachineInit(ICharacter character)
    {
        this.Character = character;
        GetComponentInit();
        ComponentInit();
    }

    protected virtual void GetComponentInit()
    {
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        Health = GetComponent<Health>();
        WeaponHandler = GetComponent<WeaponHandler>();
        ForceApplicator = GetComponent<ForceApplicator>();
        EffectApplicator = GetComponent<EffectApplicator>();
        //Rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void ComponentInit()
    {
        ForceApplicator.ForceApplicatorInit();
        Health.HealthInit();
        EffectApplicator.EffectApplicatorInit(this);

        //ForceApplicator.OnAddForceEvent += Pause;
        //ForceApplicator.OnAddForceEvent += () => print(name + "AddForce");
        ForceApplicator.OnImpactEvent += OnImpactHandler;
        //ForceApplicator.OnRemoveForceEvent += UnPause;
        Health.OnTakeDamageEvent += OnTakeDamageHandler;
        Health.OnDieEvent += OnDieHandler;
    }

    public void DieDataRegist(CharacterData characterData)
    {
        // TODO:没有 -=
        characterData.OnDieEvent += Health.OnDieHandler;
    }

    protected virtual void OnTakeDamageHandler(int damage, GameObject attackter)
    {

    }

    protected virtual void OnDieHandler(GameObject attacker)
    {

    }

    //protected virtual void OnImpactHandler(Vector3 impact)
    //{
    //    //print(name + impact);
    //    transform.Translate(impact * Time.deltaTime, Space.World);
    //}

    protected virtual void OnImpactHandler(Vector3 impact)
    {
        // 方案 A：如果你确定 Agent 始终开启且在 NavMesh 上（推荐）
        if (Agent != null && Agent.enabled)
        {
            // agent.Move 会自动处理碰撞检测和贴地
            Agent.Move(impact * Time.deltaTime);
        }
        // 方案 B：如果你的角色可能处于死掉或其他关闭 Agent 的状态
        else
        {
            // 只有在没 Agent 的时候才用 Translate，但要加射线检测防止穿墙（简易版）
            transform.Translate(impact * Time.deltaTime, Space.World);
        }
    }

    protected virtual void OnDestroy()
    {
        if (Health != null)
            Health.OnTakeDamageEvent -= OnTakeDamageHandler;
        if (ForceApplicator != null)
        {
            ForceApplicator.OnAddForceEvent -= Pause;
            ForceApplicator.OnImpactEvent -= OnImpactHandler;
            ForceApplicator.OnRemoveForceEvent -= UnPause;
        }
    }

    protected virtual void NavMeshInit(NavMeshAgent agent, CharacterMoveInfo moveSO)
    {
        agent.speed = moveSO.defaultSpeed;
        agent.stoppingDistance = moveSO.stopDistance;
        agent.angularSpeed = moveSO.angularSpeed;
        agent.acceleration = moveSO.acceleration;
    }
}
