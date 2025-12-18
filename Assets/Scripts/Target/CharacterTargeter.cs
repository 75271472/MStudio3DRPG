using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterTargeter : MonoBehaviour
{
    public bool IsActive { get; private set; } = true;
    public int Id { get; private set; }
    public event Action<CharacterTargeter> OnTargetEnter, OnTargetExit;

    [SerializeField]
    protected List<CharacterTargeter> targetList = new List<CharacterTargeter>();
    [SerializeField] protected float targetSphereRange;
    protected SphereCollider sphereCollider;
    [SerializeField] protected string[] tags;

    public void MonsterTargeterInit(int id)
    {
        Id = id;

        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = targetSphereRange;

        //OnTargetEnter += (obj) => print($"OnTargetEnter: {obj.name}");
        //OnTargetExit += (obj) => print($"OnTargetExit: {obj.name}");
    }

    public bool TryGetTarget(out CharacterTargeter targetObj)
    {
        targetObj = GetNearestTarget();
        return targetObj != null;
    }

    public void AddTarget(CharacterTargeter target)
    {
        if (target.IsActive) return;

        //target.AddTargeter(this);
        targetList.Add(target);
        OnTargetEnter?.Invoke(GetNearestTarget());
    }

    public void RemoveTarget(CharacterTargeter target)
    {
        if (!targetList.Contains(target)) return;

        // 从Target的TargeterList列表中移除自己
        //target.RemoveTargeter(this);
        targetList.Remove(target);
        OnTargetExit?.Invoke(target);
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
            RemoveTarget(targetList[i]);
        }
        targetList.Clear();
    }

    private CharacterTargeter GetNearestTarget()
    {
        float distance = -1;
        CharacterTargeter targetObj = null;

        foreach (var target in targetList)
        {
            if (distance == -1 || distance * distance > Vector3.SqrMagnitude(
                target.transform.position - this.transform.position))
                targetObj = target;
        }

        return targetObj;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!tags.Contains(other.tag)) return;
        if (!other.TryGetComponent<CharacterTargeter>(out var target)) return;

        AddTarget(target);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!tags.Contains(other.tag)) return;
        if (!other.TryGetComponent<CharacterTargeter>(out var target)) return;

        RemoveTarget(target);
    }
}
