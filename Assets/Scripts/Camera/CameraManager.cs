using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviourManager<CameraManager>
{
    public CameraController CameraController { get; private set; }

    public void CameraManagerInit()
    {
        CameraController = GetComponentInChildren<CameraController>();

        CameraController.CameraControllerInit();
    }

    public override void Init()
    {
        base.Init();
        if (IsNotSubManagerInit) return;

        CameraManagerInit();
    }

    public void SwitchPause(bool isPause)
    {
        CameraController.SwitchPause(isPause);
    }

    public void ShakeCamera(float intensity, float time)
    {
        CameraController.ShakeCamera(intensity, time);
    }
}
