using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RockWeaponLogic : WeaponLogic
{
    private Rock rock;

    public override void SetEnable()
    {
        GameObject rockObj = PoolManager.Instance.PullObj(DataManager.ROCK);
        rockObj.transform.position = transform.position;

        rock = rockObj.GetComponent<Rock>();
        rock.RockInit(handlerObj, targetObj);
        rock.OnCollidingObjectEvent += EffectApply;
    }

    public override void SetDisable() 
    {
        //rock.OnCollidingObjectEvent -= EffectApply;
    }
}
