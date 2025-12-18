using System;
using UnityEngine;

public class WeaponLogic : MonoBehaviour
{
    // 绑定的攻击动画名称
    // AttackName = AttackAll时，应用于所有攻击动画
    [field: SerializeField] public string AttackName {  get; private set; }

    protected ICharacter handler;

    protected GameObject handlerObj;
    protected GameObject targetObj;

    //protected AttackInfo attackInfo;
    protected AttackSO attackSO;

    public void WeaponLogicInit(ICharacter handler)
    {
        this.handler = handler;

        this.handlerObj = handler.CharacterGameObject;
        // 这里不通过记录attackInfo引用方式存储attackInfo
        // 而是直接调用handler.AttackInfo方式获取attackInfo
        // 避免handler中的attackInfo更新为新的值后，WeaponLogic还使用旧值的引用
        //this.attackInfo = handler.CharacterData.AttackInfo;
    }

    public void SetAttack(AttackSO attackSO, GameObject targetObj)
    {
        this.attackSO = attackSO;
        this.targetObj = targetObj;
    }

    public virtual void SetEnable()
    {
        gameObject.SetActive(true);
    }

    public virtual void SetDisable()
    {
        gameObject.SetActive(false);
    }

    // other 被攻击者 handler 攻击者
    protected void OnTriggerEnter(Collider other)
    {
        EffectApply(other);
    }

    protected virtual void EffectApply(Collider target)
    {
        //print(handler.name + target.name);

        HealthEffect(target);
        DestructibleEffect(target);
        EffectApplicatorEffect(target);
    }

    protected virtual void HealthEffect(Collider target)
    {
        if (target.TryGetComponent<Health>(out var health))
        {
            //print("attack");
            health.TakeDamage(handler.CharacterData.AttackInfo.GetDamage(), 
                handlerObj);
        }
    }

    protected virtual void DestructibleEffect(Collider target)
    {
        if (target.TryGetComponent<DestructibleTarget>(out var desTarget))
        {
            desTarget.DestroyTarget(handlerObj);
        }
    }

    protected virtual void EffectApplicatorEffect(Collider target)
    {
        if (target.TryGetComponent<EffectApplicator>(out var applicator))
        {
            // 施加力方向计算
            attackSO.attackEffectInfo.forceVect =
                target.transform.position - handlerObj.transform.position;
            // 竖直方向置零
            attackSO.attackEffectInfo.forceVect.y = 0;

            // 攻击者赋值
            attackSO.attackEffectInfo.attacker = handlerObj;

            //print(handler.name + applicator.gameObject.name);
            attackSO.ApplyEffect(applicator);
        }
    }
}
