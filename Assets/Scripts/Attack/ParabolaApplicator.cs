using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// 设计编写：常成功
/// 创建时间：2020/05/12 
/// 脚本功能：从A点(起始点), 到B点(目标点)的抛物线运动
/// 挂载位置：动态挂载, 即将运动的物体上
/// </summary>
/// 
/// ps. 关于箭的制作:
/// 1, 资源原点, 在箭头上(一般是箭头产生攻击力)
/// 2, 箭头朝向, 是z轴的增长方向


// 从A点(起始点), 到B点(目标点)的抛物线运动
public class ParabolaApplicator : MonoBehaviour
{
    // 运动速度
    public float speed = 10;
    // 目标点坐标
    private Vector3 targetPos;
    // 最小接近距离, 以停止运动
    public float minDistance = 0.5f;
    private float distanceToTarget;
    private bool moveFlag = true;

    public void StartParabola(Vector3 targetPos, Action action)
    {
        this.targetPos = targetPos;
        moveFlag = true;
        StopAllCoroutines();
        StartCoroutine(ParabolaCoroutine(action));
    }

    IEnumerator ParabolaCoroutine(Action action)
    {
        distanceToTarget = Vector3.Distance(transform.position,
            targetPos);

        while (moveFlag)
        {
            // 朝向目标, 以计算运动
            transform.LookAt(targetPos);
            // 根据距离衰减 角度
            float angle = Mathf.Min(1, Vector3.Distance(
                transform.position, targetPos) / distanceToTarget) * 45;
            // 旋转对应的角度（线性插值一定角度，然后每帧绕X轴旋转）
            Quaternion rotation = Quaternion.Euler(Mathf.Clamp(-angle, -42, 42), 0, 0);
            transform.rotation = transform.rotation * rotation;
            //print($"{angle} {rotation}");
            // 当前距离目标点
            float currentDist = Vector3.Distance(transform.position, targetPos);
            // 很接近目标了, 准备结束循环
            if (currentDist < minDistance)
            {
                moveFlag = false;
            }
            // 平移 (朝向Z轴移动)
            transform.Translate(Vector3.forward * Mathf.Min(speed * Time.deltaTime, 
                currentDist));
            // 暂停执行, 等待下一帧再执行while
            yield return null;
        }
        if (moveFlag == false)
        {
            // 使自己的位置, 跟[目标点]重合
            transform.position = targetPos;
            action?.Invoke();
            // [停止]当前协程任务,参数是协程方法名
            StopCoroutine(ParabolaCoroutine(action));
        }
    }
}