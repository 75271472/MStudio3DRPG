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
        // 增加一个超时保险，防止死循环导致逻辑永远卡在这里
        float timeout = 5f;
        float timer = 0f;

        while (Agent != null && !Agent.isOnNavMesh)
        {
            // 尝试将角色贴合到 NavMesh 上
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, 
                NavMesh.AllAreas))
            {
                Agent.Warp(hit.position);
                // 找到了也不能立即退出，下一帧 isOnNavMesh 才会更新，所以这里自然也会走到下面的 yield
            }

            timer += Time.deltaTime;
            if (timer > timeout)
            {
                Debug.LogError("严重警告：在 5 秒内未能找到有效的 NavMesh！请检查场景是否已烘焙 NavMesh。强制退出等待。");
                break;
            }

            // 【关键修改】将 yield 移到 if 外面
            // 确保无论 SamplePosition 成功还是失败，都会让出主线程，等待下一帧
            yield return null;
        }

        SwitchState(new PlayerIdleState(this));
    }

    //private IEnumerator WaitForNavMeshAgent()
    //{
    //    // 等待直到NavMeshAgent就绪
    //    while (Agent != null && !Agent.isOnNavMesh)
    //    {
    //        // 强制放置在NavMesh上
    //        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
    //        {
    //            Agent.Warp(hit.position);
    //            yield return null; // 等待一帧让Warp生效
    //        }
    //        //print($"WaitForNavMeshAgent {Agent == null} {Agent.isOnNavMesh}");
    //    }

    //    SwitchState(new PlayerIdleState(this));
    //}

    public override void Pause()
    {
        // 为null表示未初始化不再继续执行
        if (Agent == null) return;

        Agent.enabled = false;
        SwitchState(new PlayerIdleState(this));
    }

    public override void UnPause()
    {
        // 为null表示未初始化不再继续执行
        if (Agent == null) return;

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
