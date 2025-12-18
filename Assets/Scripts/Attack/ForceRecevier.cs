using UnityEngine;

public class ForceReciver : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject targetObj;

    [Header("Force Settings")]
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;

    // 当前施加的力
    private Vector3 currentImpact;

    /// <summary>
    /// 计算并施加斜抛力，使物体抛向目标位置
    /// </summary>
    /// <param name="targetPos">目标位置</param>
    /// <param name="launchAngle">初始发射角度（绕X轴旋转角，度）</param>
    /// <returns>是否成功施加力</returns>
    public bool ThrowToTarget(Vector3 targetPos, float launchAngle = 45f)
    {
        Vector3 currentPos = transform.position;

        // 计算水平距离和高度差
        Vector3 toTarget = targetPos - currentPos;
        float horizontalDistance = new Vector3(toTarget.x, 0, toTarget.z).magnitude;
        float heightDifference = toTarget.y;

        // 重力大小（取负值，因为重力向下）
        float gravity = Mathf.Abs(Physics.gravity.y);

        // 尝试使用指定角度计算所需初速度
        float angleRad = launchAngle * Mathf.Deg2Rad;

        // 斜抛运动公式：y = x * tanθ - (g * x²) / (2 * v² * cos²θ)
        // 解出初速度 v
        float cosAngle = Mathf.Cos(angleRad);
        float tanAngle = Mathf.Tan(angleRad);

        // 检查角度是否可行（避免除零和虚数解）
        if (cosAngle <= 0.001f || (horizontalDistance * tanAngle - heightDifference) <= 0)
        {
            // 如果指定角度不可行，自动计算最佳角度
            return ThrowToTargetWithOptimalAngle(targetPos);
        }

        // 计算所需初速度
        float velocity = Mathf.Sqrt((gravity * horizontalDistance * horizontalDistance) /
                                   (2 * cosAngle * cosAngle * (horizontalDistance * tanAngle - heightDifference)));

        // 检查速度是否合理
        if (float.IsNaN(velocity) || velocity > 1000f) // 速度过大说明角度不合适
        {
            return ThrowToTargetWithOptimalAngle(targetPos);
        }

        // 计算方向向量
        Vector3 direction = toTarget;
        direction.y = 0; // 水平方向
        direction.Normalize();

        // 应用角度到方向向量
        direction = Quaternion.Euler(launchAngle, 0, 0) * direction;

        rb.velocity = direction * velocity;
        // 施加力（F = m * a，但使用Impulse模式时直接是 m * v）
        //Vector3 force = direction * velocity * rb.mass;
        //rb.AddForce(force, ForceMode.Impulse);
        //currentImpact = force;

        return true;
    }

    /// <summary>
    /// 使用最优角度抛向目标
    /// </summary>
    private bool ThrowToTargetWithOptimalAngle(Vector3 targetPos)
    {
        Vector3 currentPos = transform.position;
        Vector3 toTarget = targetPos - currentPos;
        float horizontalDistance = new Vector3(toTarget.x, 0, toTarget.z).magnitude;
        float heightDifference = toTarget.y;

        float gravity = Mathf.Abs(Physics.gravity.y);

        // 计算最优发射角度（使所需初速度最小）
        // 最优角度公式：θ = arctan(v_y/v_x) 其中 v_y/v_x 满足特定条件
        // 简化计算：尝试几个角度找到可行的最小速度

        float bestAngle = 45f;
        float minVelocity = float.MaxValue;
        bool foundSolution = false;

        // 在30-75度范围内寻找最优角度
        for (float testAngle = 30f; testAngle <= 75f; testAngle += 5f)
        {
            float angleRad = testAngle * Mathf.Deg2Rad;
            float cosAngle = Mathf.Cos(angleRad);
            float tanAngle = Mathf.Tan(angleRad);

            if (cosAngle > 0.001f && (horizontalDistance * tanAngle - heightDifference) > 0)
            {
                float velocity = Mathf.Sqrt((gravity * horizontalDistance * horizontalDistance) /
                                           (2 * cosAngle * cosAngle * (horizontalDistance * tanAngle - heightDifference)));

                if (!float.IsNaN(velocity) && velocity < minVelocity && velocity > 0.1f)
                {
                    minVelocity = velocity;
                    bestAngle = testAngle;
                    foundSolution = true;
                }
            }
        }

        if (!foundSolution)
        {
            Debug.LogWarning("无法找到合适的发射角度到达目标位置");
            return false;
        }

        // 使用最优角度施加力
        Vector3 direction = toTarget;
        direction.y = 0;
        direction.Normalize();
        direction = Quaternion.Euler(bestAngle, 0, 0) * direction;

        Vector3 force = direction * minVelocity * rb.mass;
        rb.AddForce(force, ForceMode.Impulse);
        currentImpact = force;

        Debug.Log($"使用最优角度: {bestAngle}°, 初速度: {minVelocity}m/s");
        return true;
    }

    /// <summary>
    /// 添加自定义作用力
    /// </summary>
    public void AddForce(Vector3 forceVector)
    {
        rb.AddForce(forceVector, forceMode);
        currentImpact = forceVector;
    }

    /// <summary>
    /// 获取当前施加的力
    /// </summary>
    public Vector3 GetCurrentImpact()
    {
        return currentImpact;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            AddForce(Vector3.forward);
    }
}