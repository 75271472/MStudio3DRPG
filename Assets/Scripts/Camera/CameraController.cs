using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float scrollSpeed;

    [SerializeField] private float verticalDump;
    [SerializeField] private float scrollDump;

    private CinemachineFreeLook freeLookCamera;
    private CameraLookAtPoint lookAtPoint;
    private bool isPause = false;

    public void CameraControllerInit()
    {
        freeLookCamera = GetComponent<CinemachineFreeLook>();
        lookAtPoint = FindObjectOfType<CameraLookAtPoint>();

        freeLookCamera.m_Follow = lookAtPoint.transform;
        freeLookCamera.m_LookAt = lookAtPoint.transform;

        InputManager.Instance.OnHorizontalEvent += HorizontalHandler;

        isPause = false;
    }

    public void SwitchPause(bool isPause)
    {
        this.isPause = isPause;
    }

    private void Update()
    {
        UpdateHeight(InputManager.Instance.VerticalValue);
        UpdateFOV(InputManager.Instance.ScrollValue);
    }

    private void HorizontalHandler(float horzontalValue)
    {
        //print("HorizontalHandler");

        if (isPause) return;

        freeLookCamera.m_XAxis.m_InputAxisValue = horzontalValue * horizontalSpeed;
    }

    // InputManager中的回调，只有在输入值发生改变时才会调用
    // m_Orbits[1].m_Height高度设置与m_XAxis.m_InputAxisValue不同
    // m_InputAxisValue内部的实现应该是传入一个速度值，更新速度值后便会一直根据速度值进行转动
    // 因此m_InputAxisValue只在输入发生改变时修改即可，不用每帧更新
    // m_Orbits[1].m_Height需要根据InputManager中的输入值进行每帧更新
    // 不然使用回调函数进行更新会出现按住w键只更新一次的情况
    // m_Orbits[1].m_Radius同理
    private void UpdateHeight(float verticalValue)
    {
        if (isPause) return;

        freeLookCamera.m_Orbits[1].m_Height = Mathf.Lerp(
            freeLookCamera.m_Orbits[1].m_Height,
            Mathf.Max(freeLookCamera.m_Orbits[1].m_Height -
            verticalValue * verticalSpeed, 0), verticalDump);
    }

    private void UpdateFOV(float scrollValue)
    {
        if (isPause) return;

        freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(
            freeLookCamera.m_Lens.FieldOfView,
            Mathf.Max(freeLookCamera.m_Lens.FieldOfView -
            scrollValue * scrollSpeed, 0), scrollDump);
    }
}
