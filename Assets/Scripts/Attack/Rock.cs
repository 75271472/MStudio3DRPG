using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(ParabolaApplicator))]
public class Rock : MonoBehaviour
{
    [SerializeField] public GameObject handlerObj { get; private set; }
    
    // 结束Parabola前，碰撞物体执行的事件
    public event Action<Collider> OnCollidingObjectEvent;
    // 结束Parabola时，执行的事件
    public event Action OnParabolaEvent;

    //[SerializeField] private GameObject rockBreakParticlePrefab;

    private ParabolaApplicator parabolaApplicator;
    private bool isBeingCollided = true;

    public void RockInit(GameObject handlerObj, GameObject targetObj)
    {
        RockInit(handlerObj, targetObj.transform.position);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handlerObj">投掷对象</param>
    /// <param name="targeterObj">目标对象</param>
    public void RockInit(GameObject handlerObj, Vector3 targetPos)
    {
        if (handlerObj == null || handlerObj.transform.position == targetPos) return;

        OnParabolaEvent += OnParabolaHandler;

        isBeingCollided = true;
        this.handlerObj = handlerObj;

        if (parabolaApplicator == null)
            parabolaApplicator = GetComponent<ParabolaApplicator>();

        // 外部务必在RockInit语句调用之前向OnParabolaEvent和OnCollidingObjectEvent
        // 两事件添加事件
        parabolaApplicator.StartParabola(targetPos, OnParabolaEvent);
    }

    public void RockReset()
    {
        OnCollidingObjectEvent = null;
        OnParabolaEvent = null;
    }

    // 第一次从投掷者向目标抛物线运动后执行事件
    private void OnParabolaHandler()
    {
        isBeingCollided = false;

        GenerateRockBreakParticle();
        RockReset();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isBeingCollided) return;

        //print(collision.gameObject.name);
        GenerateRockBreakParticle();
        OnCollidingObjectEvent?.Invoke(collision.collider);
    }

    private void GenerateRockBreakParticle()
    {
        GameObject rockBreakParticle = 
            PoolManager.Instance.PullObj(DataManager.ROCKBREAKPARTICAL);

        rockBreakParticle.transform.position = transform.position;
        rockBreakParticle.GetComponent<Partical>().ParticalInit();
    }

    public void RockDestroy()
    {
        RockReset();
        PoolManager.Instance.PushObj(DataManager.ROCK, gameObject);
    }
}
