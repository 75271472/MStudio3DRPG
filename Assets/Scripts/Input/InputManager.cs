using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManager : BaseManager<InputManager>, InputControler.IPlayerActions
{
    public Vector2 MousePos { get; private set; }
    public bool IsOnWalk { get; private set; }
    public bool IsOnRun { get; private set; }
    public float VerticalValue { get; private set; }
    public float ScrollValue { get; private set; }

    public event Action<string> OnHotItemEvent;
    public event Action<float> OnHorizontalEvent, OnVerticalEvent, OnScrollEvent;
    public event Action OnWalkEvent, OnRunEvent, OnOperateEvent, OnSaveEvent, 
        OnOpenBagEvent, OnOpenQuestEvent, OnPauseGameEvent;

    private InputControler inputControler = null;

    public InputManager()
    {
        ResetInputAction();
        InputManagerInit();
    }

    public void InputManagerInit()
    {
        inputControler = new InputControler();
        inputControler.Player.SetCallbacks(this);
        inputControler.Enable();
    }

    // 每次添加事件后，记得在此时添加置空语句
    public void ResetInputAction()
    {
        OnHorizontalEvent = null;
        OnVerticalEvent = null;
        OnScrollEvent = null;
        OnWalkEvent = null;
        OnRunEvent = null;
        OnOperateEvent = null;
        OnSaveEvent = null;
        OnOpenBagEvent = null;
        OnHotItemEvent = null;
        OnOpenQuestEvent = null;
        OnPauseGameEvent = null;
    }

    public void OnMovePos(InputAction.CallbackContext context)
    {
        MousePos = context.ReadValue<Vector2>();
    }

    public void OnHorizontal(InputAction.CallbackContext context)
    {
        OnHorizontalEvent?.Invoke(context.ReadValue<float>());
    }

    public void OnVertical(InputAction.CallbackContext context)
    {
        VerticalValue = context.ReadValue<float>();
        OnVerticalEvent?.Invoke(VerticalValue);
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        ScrollValue = context.ReadValue<float>();
        OnScrollEvent?.Invoke(ScrollValue);
    }

    public void OnWalk(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsOnWalk = true;
            OnWalkEvent?.Invoke();
        }
        else if (context.canceled) IsOnWalk = false;
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsOnRun = true;
            OnRunEvent?.Invoke();
        }
        else if (context.canceled) IsOnRun = false;
    }

    public void OnOperate(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnOperateEvent?.Invoke();
    }

    public void OnSave(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnSaveEvent?.Invoke();
    }

    public void OnOpenBag(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnOpenBagEvent?.Invoke();
    }

    public void OnHotItem(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnHotItemEvent?.Invoke(context.control.name);
    }

    public void OnOpenQuest(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnOpenQuestEvent?.Invoke();
    }

    public void OnPauseGame(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnPauseGameEvent?.Invoke();
    }
}
