using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ForceApplicator))]
public class DestructibleTarget : MouseTarget
{
    public override EMouseTarget MouseTargetType => EMouseTarget.Destructible;
    public event Action<GameObject> OnDestroyTargetEvent;

    public void DestroyTarget(GameObject handler)
    {
        if (!handler.CompareTag("Player")) return;

        OnDestroyTargetEvent?.Invoke(handler);
    }
}
