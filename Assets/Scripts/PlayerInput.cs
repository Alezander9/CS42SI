// Handles all player input using Unity's Input System and provides events for movement actions.
// Manages jump, dash, grab, and directional inputs with proper event subscription cleanup.

using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInput : MonoBehaviour
{
    public event Action onJumpPressed;
    public event Action onJumpReleased;
    public event Action onDashPressed;

    private PlayerInputActions _inputActions;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Player.Enable();
    }

    private void Start()
    {
        _inputActions.Player.Jump.performed += JumpPerformed;
        _inputActions.Player.Jump.canceled += JumpCanceled;
        _inputActions.Player.Dash.performed += DashPerformed;
    }

    private void DashPerformed(InputAction.CallbackContext context)
    {
        onDashPressed?.Invoke();
    }

    private void JumpCanceled(InputAction.CallbackContext context)
    {
        onJumpReleased?.Invoke();
    }

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        onJumpPressed?.Invoke();
    }

    public float GetHorizontalInput()
    {
        return _inputActions.Player.Horizontal.ReadValue<float>();
    }

    public float GetVerticalInput()
    {
        return _inputActions.Player.Vertical.ReadValue<float>();
    }

    public bool IsJumpPressed()
    {
        return _inputActions.Player.Jump.inProgress;
    }

    public bool IsGrabPressed()
    {
        return _inputActions.Player.WallGrab.inProgress;
    }

    private void OnDestroy()
    {
        _inputActions.Player.Jump.performed -= JumpPerformed;
        _inputActions.Player.Jump.canceled -= JumpCanceled;
        _inputActions.Player.Dash.performed -= DashPerformed;
    }
}

