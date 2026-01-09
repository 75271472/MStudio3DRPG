using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockKickWeaponLogic : WeaponLogic
{
    private Rock rock;

    protected override void DestructibleEffect(Collider target)
    {
        if (target.TryGetComponent<Rock>(out var rock))
        {
            this.rock = rock;
            print("RockKickWeaponLogic: " + handlerObj.name + rock.handlerObj.name);
            rock.OnCollidingObjectEvent += OnCollidingObjectHandler;
            rock.RockInit(handlerObj, rock.transform.GetDownPosByRay
                (handlerObj.transform.forward, 
                handler.CharacterData.AttackInfo.damage));
        }
    }

    /// <summary>
    /// 重写父类WeaponLogic的EffectApply逻辑为空
    /// 避免普通攻击时对敌人进行EffectApply
    /// </summary>
    /// <param name="target"></param>
    protected override void EffectApply(Collider target) 
    {
        DestructibleEffect(target); 
    }

    private void OnCollidingObjectHandler(Collider collider)
    {
        print("OnCollidingObjectHandler Invoke!!!");

        HealthEffect(collider);
        ForceStandEffect(collider);
        TryDestroyRock(collider);
    }

    protected override void HealthEffect(Collider target)
    {
        if (target.TryGetComponent<Health>(out var health))
        {
            //print("attack");
            // 重写父类收到伤害逻辑，让石头造成的伤害为玩家造成伤害的两倍
            health.TakeDamage(handler.CharacterData.AttackInfo.GetDamage() * 2,
                handlerObj);
        }
    }

    // TODO:这里直接让MonsterStateMachine转到GetHit状态，而没有使用RepelEffect
    // 原因：WeaponLogic中的AttackSO是通过当前播放的Attack动画设置的，
    // 没有办法对Rock设置一个让击中对象硬直的AttackSO，
    // 要么就得修改Player的AttackSO，那样会让Player攻击到的所有Obj都硬直
    private void ForceStandEffect(Collider collider)
    {
        if (collider.TryGetComponent<MonsterStateMachine>(out var stateMachine))
        {
            if (stateMachine.Health.IsDie) return;
            stateMachine.SwitchState(new MonsterGetHitState(stateMachine));
        }
    }

    private void TryDestroyRock(Collider collider)
    {
        if (collider.TryGetComponent<MonsterStateMachine>(out var stateMachine))
            rock.RockDestroy();
    }
}
