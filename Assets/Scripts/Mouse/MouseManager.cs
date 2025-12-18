using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviourManager<MouseManager>
{
    public event Action<MouseTarget> OnMoveEvent;

    [SerializeField] private LayerMask rayLayerMask;

    private GroundTarget groundTarget;
    private MouseCursor mouseCursor;
    private RaycastHit hitInfo;

    public void MouseManagerInit()
    {
        InputManager.Instance.OnRunEvent += OnMoveHandler;

        mouseCursor = GetComponent<MouseCursor>();
        groundTarget = GetComponentInChildren<GroundTarget>();
    }

    public override void Init()
    {
        base.Init();
        if (IsNotSubManagerInit) return;

        MouseManagerInit();
    }

    private void Update()
    {
        UpdateHitInfo();
        UpdateMouseCursor();
    }

    private void OnDestroy()
    {
        OnMoveEvent = null;
    }

    private void UpdateHitInfo()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(InputManager.Instance.MousePos);
        // 可视化射线（在Scene视图中可见）
        Debug.DrawRay(mouseRay.origin, mouseRay.direction * 1000, Color.red);

        Physics.Raycast(mouseRay, out hitInfo, 1000, rayLayerMask);

        // 如果击中物体，绘制击中点和法线
        if (hitInfo.collider != null)
        {
            Debug.DrawRay(hitInfo.point, hitInfo.normal * 2, Color.green);
            Debug.DrawRay(hitInfo.point, Vector3.up * 1, Color.blue);
        }
    }

    private void UpdateMouseCursor()
    {
        if (hitInfo.collider == null) return;

        switch (hitInfo.collider.tag)
        {
            case "GroundTarget":
                mouseCursor.SwitchCursor(EMouseCursor.Target);
                break;
            case "Monster":
                if (hitInfo.collider.gameObject.
                    TryGetComponent<EnemyTarget>(out var target) && 
                    target.CheckMonster())
                {
                    mouseCursor.SwitchCursor(EMouseCursor.Attack);
                }
                break;
            case "DestructibleObj":
                mouseCursor.SwitchCursor(EMouseCursor.Attack);
                break;
            case "Portal":
                mouseCursor.SwitchCursor(EMouseCursor.Doorway);
                break;
            case "UI":
            case "ItemOnWord":
            default:
                mouseCursor.SwitchCursor(EMouseCursor.Point);
                break;
        }
    }

    private void OnMoveHandler()
    {
        if (hitInfo.collider == null) return;

        StopAllCoroutines();
        StartCoroutine(UpdateMouseTaregtCoroutine());
        OnMoveEvent?.Invoke(GetTargetObj());
    }

    private IEnumerator UpdateMouseTaregtCoroutine()
    {
        while (hitInfo.collider != null && InputManager.Instance.IsOnRun)
        {
            groundTarget.UpdatePos(hitInfo.point);
            yield return null;
        }
    }

    private MouseTarget GetTargetObj()
    {
        if (hitInfo.collider == null) return null;

        if (hitInfo.collider.TryGetComponent<MouseTarget>(out var mouseTarget))
            return mouseTarget;
        else
            return groundTarget;
    }
}
