using UnityEngine;
using UnityEngine.UI;

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
    //public static bool IsTargetInAreaByRay(this Transform transform, GameObject target,
    //    float distance, float angle, string isDebug = null)
    //{
    //    Vector3 direction = target.transform.position - transform.position;
    //    Ray ray = new Ray(transform.position, direction);
    //    Debug.DrawRay(transform.position, direction.normalized * distance, Color.red, 0.1f);

    //    // 是否检测到物体（检测所有层）
    //    if (!Physics.Raycast(ray, out var hitInfo, distance, GetLayer()))
    //    {
    //        return false;
    //    }

    //    if (isDebug != null)
    //    {
    //        Debug.Log($"{isDebug} Distance: " +
    //            $"{Vector3.Distance(transform.position, hitInfo.point)} " +
    //            $"TargetDistance: {distance}");

    //        Debug.Log($"{isDebug} {hitInfo.collider.name} {target.name}");
    //    }

    //    // 检测到的物体是否和target是同一个
    //    // 如果collider为target的子物体，会判定失败！！！
    //    if (!hitInfo.collider.gameObject.Equals(target)) return false;
    //    // 都满足参考角度是否在范围内
    //    return IsTargetInAngle(transform, target, angle, isDebug);
    //}

    public static bool IsTargetInAreaByRay(this Transform transform, GameObject target,
        float maxDistance, float angle, string isDebug = null, float heightOffset = 1.0f)
    {
        return IsTargetInAreaByRay(transform, target, maxDistance, angle, out var distance, 
            isDebug, heightOffset);
    }

    // 增加 heightOffset 参数，通常设为 1.0f - 1.5f (根据角色身高)
    /// <summary>
    /// 检测目标是否在扇形区域内，并返回表面最短距离
    /// </summary>
    /// <param name="transform">发起者</param>
    /// <param name="target">目标</param>
    /// <param name="maxDistance">最大检测距离</param>
    /// <param name="angle">扇形角度</param>
    /// <param name="surfaceDistance">【输出】计算出的表面最短距离（如果失败可能为 Infinity）</param>
    /// <param name="heightOffset">发起者的高度偏移（如眼睛高度），防止脚底被遮挡</param>
    /// <param name="isDebug">Debug标签</param>
    /// <returns>是否在范围内且可见</returns>
    public static bool IsTargetInAreaByRay(this Transform transform, GameObject target,
        float maxDistance, float angle, out float surfaceDistance,
        string isDebug = null, float heightOffset = 1.0f)
    {
        // 初始化输出参数 (必须在任何 return 前赋值)
        surfaceDistance = Mathf.Infinity;

        if (target == null) return false;

        // 1. 确定起点：加上高度偏移，从“胸口/眼睛”发射，避免被地面的微小起伏遮挡
        Vector3 startPos = transform.position + Vector3.up * heightOffset;

        // 2. 计算目标表面最近点
        Vector3 targetPoint;
        Collider targetCollider = target.GetComponent<Collider>();

        if (targetCollider != null)
        {
            // 获取碰撞体表面距离 startPos 最近的点
            targetPoint = targetCollider.ClosestPoint(startPos);
        }
        else
        {
            // 如果目标没有Collider，回退到使用目标中心点（并加上同样的高度偏移以防只检测脚底）
            targetPoint = target.transform.position + Vector3.up * heightOffset;
        }

        // 3. 【核心修改】计算并赋值表面距离
        Vector3 direction = targetPoint - startPos;
        surfaceDistance = direction.magnitude;

        // 4. 距离预判：如果几何距离已经超过最大距离，直接返回 false
        if (surfaceDistance > maxDistance)
        {
            return false;
        }

        // 5. 射线检测（检测遮挡）
        Ray ray = new Ray(startPos, direction.normalized);

        // 稍微加一点距离容差 (0.05f) 防止浮点数精度问题导致打不到自身表面
        if (Physics.Raycast(ray, out var hitInfo, surfaceDistance + 0.05f, GetLayer()))
        {
            // 判定击中物体是否属于目标 (包括自身或是子物体)
            bool isTarget = hitInfo.collider.gameObject == target ||
                            hitInfo.collider.transform.IsChildOf(target.transform);

            if (isDebug != null)
            {
                Debug.DrawLine(startPos, targetPoint, isTarget ? Color.green : Color.red, 0.1f);
                //if (!isTarget) Debug.Log($"{isDebug} Blocked by: {hitInfo.collider.name}");
                Debug.Log($"{isDebug} distance {surfaceDistance} maxDistance {maxDistance}");
            }

            if (!isTarget) return false; // 被墙体遮挡

            // 6. 角度判定 (最后做，因为前面的 Raycast 也就是一条，开销不大)
            return IsTargetInAngle(transform, target, angle, isDebug);
        }

        // 如果 Raycast 没打中任何东西（极其罕见，可能是ClosestPoint在内部），
        // 或者距离极近，通常视为可见，继续判断角度
        return IsTargetInAngle(transform, target, angle, isDebug);
    }

    // 备用：如果目标没有Collider，使用中心点对射
    private static bool CheckByPosition(Transform transform, GameObject target, 
        Vector3 startPos, Vector3 targetPos, float distance, float angle, 
        string isDebug)
    {
        Vector3 direction = targetPos - startPos;
        if (direction.magnitude > distance) return false;

        if (Physics.Raycast(startPos, direction.normalized, out var hitInfo, 
            direction.magnitude, GetLayer()))
        {
            bool isTarget = hitInfo.collider.gameObject == target ||
                           hitInfo.collider.transform.IsChildOf(target.transform);
            if (!isTarget) return false;
        }
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
        Vector3 direction = pos - transform.position;
        // 忽略y轴影响
        direction.y = 0;

        if (isDebug != null)
        {
            float a = Vector3.Dot(direction.normalized, transform.forward);
            float b = Mathf.Cos(angle * Mathf.Deg2Rad);
            Debug.Log($"{isDebug} Angle Again {a} {b}");
        }

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
        pos.y = transform.position.y;
        if (isDebug != null)
        {
            Debug.Log($"{isDebug} Distance: {Vector3.Distance(pos, transform.position)} " +
                $"TargetDistance: {distance}");
        }
        // 计算水平面内距离
        
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

    public static float GetTargetDistanceInSaveHeight(this Transform transform, GameObject targetObj)
    {
        Vector3 targetPos = targetObj.transform.position;
        targetPos.y = transform.position.y;
        //Debug.Log($"GetTargetDistance: {Vector3.Distance(transform.position, targetPos)}");

        return Vector3.Distance(transform.position, targetPos);
    }

    // 射线检测攻击点，只对Player和Monster层进行检测
    private static int GetLayer()
    {
        return 1 << LayerMask.NameToLayer("Player") | 
            1 << LayerMask.NameToLayer("Monster") | 
            1 << LayerMask.NameToLayer("Rock");
    }
}
