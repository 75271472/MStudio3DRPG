using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public static class ExtensionTool
{
    public static void UpdateUI(Transform transform)
    {
        UpdateUI(transform as RectTransform);
    }

    public static void UpdateUI(RectTransform rectTransform)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    /// <summary>
    /// 使用GetPosByRay获取Pos
    /// 从Pos向下发射长度为distance的射线，
    /// 射线碰撞到物体返回物体碰撞点position，
    /// 否则返回Pos + Vector3.down * distance
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <param name="isDebug"></param>
    /// <returns></returns>
    public static Vector3 GetDownPosByRay(this Transform transform, Vector3 direction,
        float distance, string isDebug = null)
    {
        Vector3 pos = GetPosByRay(transform, direction, distance, isDebug);
        Ray ray = new Ray(pos, Vector3.down * distance);
        Debug.DrawRay(pos, Vector3.down * distance, Color.red, 0.1f);
        if (!Physics.Raycast(ray, out var hitInfo, distance))
            return pos + Vector3.down * distance;
        return hitInfo.point;
    }

    /// <summary>
    /// 从transform.positon向direction方向发射长度为distance的射线
    /// 射线碰撞到物体返回物体碰撞点position，
    /// 否则返回transform.position + transform.forward * distance
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="distance"></param>
    /// <param name="isDebug"></param>
    /// <returns></returns>
    public static Vector3 GetPosByRay(this Transform transform, Vector3 direction,
        float distance, string isDebug = null)
    {
        direction.Normalize();
        Ray ray = new Ray(transform.position, direction);
        Debug.DrawRay(transform.position, direction * distance, Color.red, 0.1f);
        if (!Physics.Raycast(ray, out var hitInfo, distance))
            return transform.position + direction * distance;
        return hitInfo.point;
    }

    /// <summary>
    /// 检测射线与Target的碰撞点是否在Area内
    /// 而非直接比较Target.position是否在Area内
    /// </summary>
    /// <param name="transform">调用transform</param>
    /// <param name="target"></param>
    /// <param name="distance">检测半径</param>
    /// <param name="angle">检测点与自身position连线与自身forward水平夹角最大值</param>
    /// <param name="isDebug">传入非null字符串打印调试信息</param>
    /// <returns></returns>
    public static bool IsTargetInAreaByRay(this Transform transform, GameObject target,
        float distance, float angle, string isDebug = null)
    {
        Vector3 direction = target.transform.position - transform.position;
        Ray ray = new Ray(transform.position, direction);
        Debug.DrawRay(transform.position, direction.normalized * distance, Color.red, 0.1f);

        // 是否检测到物体（检测所有层）
        if (!Physics.Raycast(ray, out var hitInfo, distance, GetLayer()))
        {
            return false;
        }

        if (isDebug != null)
        {
            Debug.Log($"{isDebug} Distance: " +
                $"{Vector3.Distance(transform.position, hitInfo.point)} " +
                $"TargetDistance: {distance}");

            Debug.Log($"{isDebug} {hitInfo.collider.name} {target.name}");
        }

        // 检测到的物体是否和target是同一个
        if (!hitInfo.collider.gameObject.Equals(target)) return false;
        // 都满足参考角度是否在范围内
        return IsTargetInAngle(transform, target, angle, isDebug);
    }

    /// <summary>
    /// 直接比较Target.position是否在Area内
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="target"></param>
    /// <param name="distance"></param>
    /// <param name="angle"></param>
    /// <param name="isDebug"></param>
    /// <returns></returns>
    public static bool IsTargetInArea(this Transform transform, GameObject target, 
        float distance, float angle, string isDebug = null)
    {
        return IsTargetInDistance(transform, target, distance, isDebug) &&
            IsTargetInAngle(transform, target, angle, isDebug);
    }

    public static bool IsTargetInAngle(this Transform transform, 
        GameObject targetObj, float angle, string isDebug = null)
    {
        return IsTargetInAngle(transform, targetObj.transform.position, angle, isDebug);
    }

    public static bool IsTargetInAngle(this Transform transform, 
        Vector3 pos, float angle, string isDebug = null)
    {
        if (isDebug != null)
        {
            Debug.Log($"{isDebug} Angle: " +
                $"{Vector3.Angle((pos - transform.position).normalized, transform.forward)} " +
                $"TargetAngle: {angle}");

            Vector3 d = pos - transform.position;
            float a = Vector3.Dot(d.normalized, transform.forward);
            float b = Mathf.Cos(angle * Mathf.Deg2Rad);
            Debug.Log($"{isDebug} Angle Again {a} {b}");
        }

        Vector3 direction = pos - transform.position;
        // 忽略y轴影响
        direction.y = 0;
        return Vector3.Dot(direction.normalized, transform.forward)
             >= Mathf.Cos(angle * Mathf.Deg2Rad);
    }

    public static bool IsTargetInDistance(this Transform transform, 
        GameObject targetObj, float distance, string isDebug = null)
    {
        return IsTargetInDistance(transform, targetObj.transform.position, distance, 
            isDebug);
    }

    public static bool IsTargetInDistance(this Transform transform, 
        Vector3 pos, float distance, string isDebug = null)
    {
        if (isDebug != null)
        {
            Debug.Log($"{isDebug} Distance: {Vector3.Distance(pos, transform.position)} " +
                $"TargetDistance: {distance}");
        }
        // 计算水平面内距离
        pos.y = transform.position.y;
        return Vector3.SqrMagnitude(pos - transform.position) <
            distance * distance;
    }

    // 朝向target的Rotation
    public static void UpdateRotateToTarget(this Transform transform,
        GameObject targetObj, float rotateSpeed)
    {
        // 每帧旋转固定角度，更精确控制旋转速度
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            targetObj.transform.rotation, rotateSpeed * Time.deltaTime);
    }

    // 朝向target
    public static void UpdateLookToTarget(this Transform transform, 
        GameObject targetObj, float rotateSpeed)
    {
        // 每帧旋转固定角度，更精确控制旋转速度
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(targetObj.transform.position - transform.position),
            rotateSpeed * Time.deltaTime
        );
    }

    public static float GetTargetDistance(this Transform transform, GameObject targetObj)
    {
        return Vector3.Distance(transform.position, targetObj.transform.position);
    }

    // 射线检测攻击点，只对Player和Monster层进行检测
    private static int GetLayer()
    {
        return 1 << LayerMask.NameToLayer("Player") | 
            1 << LayerMask.NameToLayer("Monster") | 
            1 << LayerMask.NameToLayer("Rock");
    }
}
