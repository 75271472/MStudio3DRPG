using UnityEngine;


public class EffectApplicator
    : MonoBehaviour
{
    public CharacterStateMachine StateMachine { get; protected set; }

    public void EffectApplicatorInit(CharacterStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public void EffectApply()
    {

    }
}