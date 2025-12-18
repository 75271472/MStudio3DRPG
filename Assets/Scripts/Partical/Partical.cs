using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Partical : MonoBehaviour
{
    private ParticleSystem particle;

    public void ParticalInit()
    {
        if (particle == null)
            particle = GetComponent<ParticleSystem>();

        StartCoroutine(PlayParticleSystem());
    }
    private IEnumerator PlayParticleSystem()
    {
        // 播放粒子系统
        particle.Play();

        // 等待粒子系统播放完成
        while (particle.isPlaying)
        {
            yield return null;
        }

        ParticalDestroy();
    }

    private void ParticalDestroy()
    {
        PoolManager.Instance.PushObj(DataManager.ROCKBREAKPARTICAL, gameObject);
    }
}
