using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterPatroller : MonoBehaviour
{
    public int Id { get; private set; }

    // 巡逻半径
    [SerializeField] private float patrolRange;
    [SerializeField] private List<MonsterPatrol> patrolList = 
        new List<MonsterPatrol>();
    [SerializeField] private Transform monsterPatrolTrans;

    private const string navMeshLayerMask = "Walkable";

    // 巡逻基点
    private Vector3 basePos;
    private int patrolIndex;
    private GameObject basePatrol;

    public void MonsterPatrollerInit(int id)
    {
        Id = id;
        basePos = transform.position;
        patrolIndex = 0;

        CreateBasePatrol();
    }

    public GameObject GetPatrolPos()
    {
        // 如果守卫为Guard状态，则永远返回巡逻基点
        if (MonsterManager.Instance.GetMonster(Id).MonsterData.MoveType ==
            EMonsterMoveType.Guard)
        {
            return basePatrol;
        }
            
        return patrolList.Count == 0 ? GetRandomPatrolPos() : UpdatePatrolPos();
    }

    private void CreateBasePatrol()
    {
        if (basePatrol != null) return;

        monsterPatrolTrans.position = transform.position;
        basePatrol = new GameObject("BasePatrol");
        basePatrol.transform.parent = monsterPatrolTrans;
        basePatrol.transform.position = transform.position;
        basePatrol.transform.rotation = transform.rotation;
    }

    private GameObject UpdatePatrolPos()
    {
        GameObject patrolObj = patrolList[patrolIndex].gameObject;
        patrolIndex = (patrolIndex + 1) % patrolList.Count;
        return patrolObj;
    }

    private GameObject GetRandomPatrolPos()
    {
        float radius = Random.Range(0, 1.0f);
        float angle = Random.Range(0, 360.0f) * Mathf.Deg2Rad;

        basePatrol.transform.position = 
            basePos + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * 
            radius * patrolRange;
        
        NavMeshHit hit = new NavMeshHit();
        // TODO:Monster位于山崖等垂直落差较大的地点时，随机目标点有可能生成在山内或空中
        // 导致Monster无法到达
        // 如果随机的目标区域不可达，则重新获取随机目标点
        if (!NavMesh.SamplePosition(basePatrol.transform.position, out hit,
            patrolRange, 1 << NavMesh.GetAreaFromName(navMeshLayerMask)))
            return GetRandomPatrolPos();

        return basePatrol;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(basePos, patrolRange);
    }
}
