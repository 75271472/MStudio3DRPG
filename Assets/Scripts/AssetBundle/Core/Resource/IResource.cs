using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public interface IResource
{
    string url { get; }
    Object GetAsset();

    T GetAsset<T>() where T : Object;
    
    GameObject Instantiate();

    GameObject Instantiate(Transform parnent, bool instantiateInWorldSpace);
}
