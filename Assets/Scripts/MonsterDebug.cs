using UnityEngine;

public class MonsterDebug : MonoBehaviour
{
    private Vector3 lastPosition;
    private Bounds lastBounds;

    void Update()
    {
        // 检查位置变化
        if (transform.position != lastPosition)
        {
            Debug.Log($"{name} moved from {lastPosition} to {transform.position}");
            lastPosition = transform.position;
        }

        // 检查碰撞体边界变化
        if (TryGetComponent<Collider>(out var collider))
        {
            Bounds currentBounds = collider.bounds;
            if (currentBounds != lastBounds)
            {
                Debug.Log($"{name} bounds changed from {lastBounds} to {currentBounds}");
                lastBounds = currentBounds;
            }
        }
    }
}