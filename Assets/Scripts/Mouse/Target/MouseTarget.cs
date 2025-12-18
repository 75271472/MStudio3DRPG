using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public enum EMouseTarget
{
    Ground, 
    Enemy,
    Destructible,
}

public abstract class MouseTarget : MonoBehaviour
{
    public Vector3 Pos => transform.position;
    public virtual EMouseTarget MouseTargetType => EMouseTarget.Ground;
}
