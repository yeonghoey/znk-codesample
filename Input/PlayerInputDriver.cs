using System;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IMITPlayerInputDriver : IMessageInterchangeTarget
{
    void OnMove(Vector2 inputDir);
    void OnAttack(Vector2 inputDir);
    void OnRoll(bool isPressing);
    void OnTake(bool isPressing);
    void OnDrop(bool isPressing);
}

public interface IMITPlayerInputDriverForPause : IMessageInterchangeTarget
{
    void OnPause();
}

public class PlayerInputDriver : MonoBehaviour,
    IMITModalPause,
    IMITLevelEnd,
    IMITEnding
{
    [SerializeField] private MessageInterchange messageInterchange;
    [SerializeField] private PlayerInput playerInput;

    private static readonly Action<IMITPlayerInputDriver, Vector2> callOnMove = (t, v) => t.OnMove(v);
    private static readonly Action<IMITPlayerInputDriver, Vector2> callOnAttack = (t, v) => t.OnAttack(v);
    private static readonly Action<IMITPlayerInputDriver, bool> callOnRoll = (t, b) => t.OnRoll(b);
    private static readonly Action<IMITPlayerInputDriver, bool> callOnTake = (t, b) => t.OnTake(b);
    private static readonly Action<IMITPlayerInputDriver, bool> callOnDrop = (t, b) => t.OnDrop(b);
    private static readonly Action<IMITPlayerInputDriverForPause> callOnPause = (t) => t.OnPause();

    private bool isOnResult;

    void OnEnable()
    {
        messageInterchange.Register(this);
    }

    void OnDisable()
    {
        messageInterchange.Deregister(this);
    }

    void Start()
    {
        isOnResult = false;
        playerInput.SwitchCurrentActionMap("UI");
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var inputDir = context.ReadValue<Vector2>();
        messageInterchange.Invoke(callOnMove, inputDir);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        var inputDir = context.ReadValue<Vector2>();
        messageInterchange.Invoke(callOnAttack, inputDir);
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            messageInterchange.Invoke(callOnRoll, true);
        }
        if (context.canceled)
        {
            messageInterchange.Invoke(callOnRoll, false);
        }
    }

    public void OnTake(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            messageInterchange.Invoke(callOnTake, true);
        }
        if (context.canceled)
        {
            messageInterchange.Invoke(callOnTake, false);
        }
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            messageInterchange.Invoke(callOnDrop, true);
        }
        if (context.canceled)
        {
            messageInterchange.Invoke(callOnDrop, false);
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            messageInterchange.Invoke(callOnPause);
        }
    }

    public void OnDeviceLost(PlayerInput playerInput)
    {
        messageInterchange.Invoke(callOnPause);
    }

    // NOTE: To make sure to not send Player Character Input when loading next scene,
    // ActionMap should be switched to UI before chaning scenes.

    void IMITLevelEnd.OnEndPhase(bool isGoalArrived)
    {
        playerInput.SwitchCurrentActionMap("UI");
        isOnResult = true;
    }

    void IMITModalPause.OnPause()
    {
        playerInput.SwitchCurrentActionMap("UI");
    }

    void IMITModalPause.OnResume()
    {
        string actionMap = isOnResult ? "UI" : "Player";
        playerInput.SwitchCurrentActionMap(actionMap);
    }

    void IMITEnding.OnAfterFadeIn()
    {
        playerInput.SwitchCurrentActionMap("Player");
    }

    void IMITEnding.OnBeforeCredits()
    {
        playerInput.SwitchCurrentActionMap("UI");
    }
}
