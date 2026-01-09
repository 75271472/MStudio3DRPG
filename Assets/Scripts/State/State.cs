using System;
//using UnityEditor.Animations;
using UnityEngine;

// 状态机默认状态
public interface IDefaultState { }
public abstract class State
{
    public abstract void Enter();
    public abstract void Tick(float deltaTime);
    public abstract void Exit();

    /// <summary>
    /// 返回指定标签的动画的标准化时间值
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    protected float GetNormalizedTime(Animator animator, string tag)
    {
        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);

        if (animator.IsInTransition(0) && nextInfo.IsTag(tag))
        {
            return nextInfo.normalizedTime;
        }
        else if (currentInfo.IsTag(tag))
        {
            return currentInfo.normalizedTime;
        }
        else
        {
            return 0;
        }
    }

    // 设置动画时间，让动画在设置时间内能够恰好播放完毕
    //protected void SpeedNormalizedAnimation(
    //    Animator animator, string animationName, float time)
    //{
    //    float animationTime = GetAnimationTimeByName(animator, animationName);
    //    AnimatorState state = GetStateByName(animator, animationName);
    //    if ( state == null )
    //    {
    //        Debug.LogWarning($"Not Find AnimatorState By {animationName}");
    //    }
    //    else
    //    {
    //        state.speed = animationTime / time;
    //    }
    //}

    //private float GetAnimationTimeByName(Animator animator, string animationName)
    //{
    //    AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
    //    AnimationClip clip = Array.Find(clips, clip => clip.name.Contains(animationName));
    //    return clip.length;
    //}

    //private AnimatorState GetStateByName(Animator animator, string animationName)
    //{
    //    //获取AnimatorController中的0层级
    //    AnimatorController controller = GetAnimatorController(
    //        animator.runtimeAnimatorController);

    //    foreach (AnimatorControllerLayer layer in controller.layers)
    //    {
    //        AnimatorStateMachine stateMachine = layer.stateMachine;
    //        //获取层级中的状态机
    //        ChildAnimatorState[] states = stateMachine.states;
    //        ChildAnimatorState state = 
    //            Array.Find(states, state => state.state.name.Contains(animationName));
    //        return state.state;
    //    }
    //    return null;
    //}

    //private AnimatorController GetAnimatorController(
    //    RuntimeAnimatorController runtimeAnimatorController)
    //{
    //    if (runtimeAnimatorController == null)
    //        return null;

    //    // 如果是覆写控制器，获取它的基础控制器
    //    if (runtimeAnimatorController is AnimatorOverrideController overrideController)
    //    {
    //        return overrideController.runtimeAnimatorController as AnimatorController;
    //    }

    //    // 否则直接返回
    //    return runtimeAnimatorController as AnimatorController;
    //}
}
