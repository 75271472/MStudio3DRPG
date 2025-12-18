using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ForceApplicator : MonoBehaviour
{
    public event Action OnAddForceEvent;
    public event Action<Vector3> OnImpactEvent;
    public event Action OnRemoveForceEvent;

    private Vector3 impact;
    private Vector3 dampingVelocity;
    // 越大速度降得越慢
    private float drag = 0.5f;

    public void ForceApplicatorInit()
    {
        OnAddForceEvent = null;
        OnImpactEvent = null;
        OnRemoveForceEvent = null;
    }

    public void AddForce(Vector3 forceVect)
    {
        impact += forceVect;
        OnAddForceEvent?.Invoke();
        //stateMachine.Pause();
        StartCoroutine(CheckForceCoroutine());
    }

    private IEnumerator CheckForceCoroutine()
    {
        while (impact.sqrMagnitude >= 0.2 * 0.2)
        {
            impact = Vector3.SmoothDamp(
                impact, Vector3.zero, ref dampingVelocity, drag);
            OnImpactEvent?.Invoke(impact);
            yield return null;
        }

        OnImpactEvent?.Invoke(Vector3.zero);
        OnRemoveForceEvent?.Invoke();
    }

    //private void Update()
    //{
    //    //impact = Vector3.SmoothDamp(impact, Vector3.zero, ref dampingVelocity, drag);

    //    //this.stateMachine.Rigidbody.velocity = impact;

    //    if (impact.sqrMagnitude < 0.2f * 0.2f)
    //    {
    //        //stateMachine.Agent.enabled = impact.sqrMagnitude < 0.2f * 0.2f;
    //        OnRemoveForceEvent?.Invoke();
    //        //if (stateMachine.IsPause && stateMachine.Agent.enabled)
    //        //    stateMachine.UnPause();
    //    }
    //}
}
