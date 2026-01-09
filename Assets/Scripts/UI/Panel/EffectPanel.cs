using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class EffectPanel : BasePanel
{
    [SerializeField] private Image effectImg; // 拖入刚才创建的 FlashOverlay
    [SerializeField] private int defaultAlpha;
    [SerializeField] private CanvasGroup canvasGroup;

    public Color damageColor = new Color(1f, 0f, 0f, 1f);   // 红色
    public Color levelUpColor = new Color(1f, 0.92f, 0.016f, 1f); // 金黄色
    public Color recoveryColor = new Color(0.13f, 0.63f, 0.38f, 1f); // 翠绿色

    public float alphaTime = 2f; // 消失时间

    public override void ShowMe()
    {
        base.ShowMe();

        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 外部调用：角色受伤
    public void DamageEffect()
    {
        print("DamageEffect Trigger");

        StopAllCoroutines(); // 如果有正在播放的动画，打断它
        StartCoroutine(FlashRoutine(damageColor));
    }

    // 外部调用：角色升级
    public void LevelUpEffect()
    {
        print("LevelUpEffect Trigger");

        StopAllCoroutines();
        StartCoroutine(FlashRoutine(levelUpColor));
    }

    public void RecoveryEffect()
    {
        print("RecoveryUpEffect Trigger");

        StopAllCoroutines();
        StartCoroutine(FlashRoutine(recoveryColor));
    }

    // 渐变协程
    IEnumerator FlashRoutine(Color targetColor)
    {
        // 1. 瞬间变色并显示（Alpha设为1或你想要的最大透明度）
        effectImg.color = new Color(targetColor.r, targetColor.g, targetColor.b, 
            defaultAlpha * 1.0f / 255);

        canvasGroup.alpha = 1;

        float elapsedTime = 0;

        while (elapsedTime < alphaTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / alphaTime);

            yield return null;
        }

        canvasGroup.alpha = 0;
        UIManager.Instance.HidePanel<EffectPanel>();
    }
}