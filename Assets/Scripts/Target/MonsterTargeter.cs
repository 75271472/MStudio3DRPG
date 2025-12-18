using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterTargeter : MonoBehaviour
{
    public bool IsActive { get; private set; } = true;

    public int Id { get; private set; }
    public event Action<GameObject> OnTargetEnter, OnTargetExit;

    [SerializeField] private List<MonsterTarget> targetList = 
        new List<MonsterTarget>();
    [SerializeField] private float targetSphereRange;

    private SphereCollider sphereCollider;
    
    public void MonsterTargeterInit(int id)
    {
        Id = id;

        sphereCollider = GetComponentInChildren<SphereCollider>();
        sphereCollider.radius = targetSphereRange;

        //OnTargetEnter += (obj) => print($"OnTargetEnter: {obj.name}");
        //OnTargetExit += (obj) => print($"OnTargetExit: {obj.name}");
    }

    public bool TryGetTarget(out GameObject targetObj)
    {
        targetObj = GetNearestTarget();
        return targetObj != null;
    }

    public void AddTarget(GameObject targetObj)
    {
        if (!IsTarget(targetObj, out var target)) return;

        target.AddTargeter(this);
        targetList.Add(target);
        OnTargetEnter?.Invoke(GetNearestTarget());
    }

    public void RemoveTarget(GameObject targetObj)
    {
        if (!ContainsTarget(targetObj, out var target)) return;

        // 从Target的TargeterList列表中移除自己
        target.RemoveTargeter(this);
        targetList.Remove(targetObj.GetComponent<MonsterTarget>());
        OnTargetExit?.Invoke(targetObj);
    }

    public void SetDisable()
    {
        IsActive = false;
        gameObject.SetActive(false);
        Clear();
    }

    // 执行逻辑: 遍历target，在targetList中移除该target
    public void Clear()
    {
        // 遍历过程中会执行targetList.Remove，修改target列表
        // 因此从后向前移除
        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            RemoveTarget(targetList[i].gameObject);
        }
        targetList.Clear();
    }

    private GameObject GetNearestTarget()
    {
        float distance = -1;
        GameObject targetObj = null;

        foreach (MonsterTarget target in targetList)
        {
            if (distance == -1 || distance * distance > Vector3.SqrMagnitude(
                target.transform.position - this.transform.position))
                targetObj = target.gameObject;
        }

        return targetObj;
    }

    // 判断targetObj是否含有MonsterTarget，并且是否在targetList列表中
    public bool ContainsTarget(GameObject targetObj, out MonsterTarget target)
    {
        // 不使用IsTarget判断，防止无法判断IsVisible = false的Target
        if (targetObj.TryGetComponent<MonsterTarget>(out target))
        {
            return targetList.Contains(target);
        }
        return false;
    }

    private bool IsTarget(GameObject targetObj, out MonsterTarget target)
    {
        if (targetObj.TryGetComponent<MonsterTarget>(out target))
            return target.IsVisible;
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        AddTarget(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        RemoveTarget(other.gameObject);
    }
}
