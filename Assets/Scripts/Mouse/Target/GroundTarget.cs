using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTarget : MouseTarget
{
    public override EMouseTarget MouseTargetType => EMouseTarget.Ground;


    public void UpdatePos(Vector3 pos)
    {
        this.transform.position = pos;
    }
}