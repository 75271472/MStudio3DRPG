using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInitTrans : MonoBehaviour
{
    public Vector3 Pos => transform.position;
    public Quaternion Rot => transform.rotation;
}
