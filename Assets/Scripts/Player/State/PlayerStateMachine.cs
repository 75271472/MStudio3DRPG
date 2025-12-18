using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerStateMachine : CharacterStateMachine
{
    public MonsterTarget MonsterTarget { get; private set; }
    public PlayerMoveInfo PlayerMoveSO { get; private set; }
    public AttackComboList PlayerComboList { get; private set; }

    protected override void GetComponentInit()
    {
        base.GetComponentInit();

        MonsterTarget = GetComponent<MonsterTarget>();

        PlayerMoveSO = PlayerManager.Instance.PlayerData.PlayerMoveInfo;
        PlayerComboList = PlayerManager.Instance.PlayerData.AttackComboList;
    }

    protected override void ComponentInit()
    {
        //PlayerManager.Instance.PlayerData.SwitchWeaponRegist(WeaponHandler);

        //WeaponHandler.ResetWeapon();
        //WeaponHandler.SetWeapon(Character, PlayerManager.Instance.PlayerData.
        //    PlayerWeaponInfo);

        base.ComponentInit();

        NavMeshInit(Agent, PlayerMoveSO);
        // 如果没死，将IsVisible置真
        if (!PlayerManager.Instance.PlayerData.PlayerStateInfo.isDie)
        {
            Health.ResetIsDie();
            MonsterTarget.SetVisible();
        }
    }

    protected override void NavMeshInit(NavMeshAgent agent, CharacterMoveInfo moveSO)
    {
        base.NavMeshInit(agent, moveSO);

        StartCoroutine(WaitForNavMeshAgent());
    }

    private IEnumerator WaitForNavMeshAgent()
    {
        // 等待直到NavMeshAgent就绪
        while (Agent != null && !Agent.isOnNavMesh)
        {
            // 强制放置在NavMesh上
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                Agent.Warp(hit.position);
                yield return null; // 等待一帧让Warp生效
            }
            //print($"WaitForNavMeshAgent {Agent == null} {Agent.isOnNavMesh}");
        }

        SwitchState(new PlayerIdleState(this));
    }

    public override void Pause()
    {
        Agent.enabled = false;
        SwitchState(new PlayerIdleState(this));
    }

    public override void UnPause()
    {
        Agent.enabled = true;
    }

    public override void SwitchState(State newState)
    {
        //Debug.Log(
        //    (currentState?.GetType().Name ?? "null") +
        //    " to " +
        //    (newState?.GetType().Name ?? "null")
        //);

        base.SwitchState(newState);
    }

    public void SwitchEquipItemRegist(PlayerData playerData)
    {
        playerData.OnEquipItemEvent += (itemInfo) =>
            WeaponHandler.OnEquipWeaponHandler(Character, itemInfo);
        playerData.OnUnloadItemEvent += (itemInfo) =>
            WeaponHandler.OnUnloadItemHandler(itemInfo);
    }

    protected override void OnTakeDamageHandler(int damage, GameObject attacker)
    {
        //print("PlayerGet Hit: " + damage);
        //SwitchState(new PlayerGetHitState(this));
    }

    protected override void OnDieHandler(GameObject attacker)
    {
        MonsterTarget.SetUnvisible();
        SwitchState(new PlayerDieState(this));

        CameraManager.Instance.SwitchPause(true);
        UIManager.Instance.ShowPanel<DiePanel>();
    }
}
