// Player input implementation using Unity's Input System.
// Implements ICharacterInput to allow CharacterController to work with player input.

using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInput : MonoBehaviour, ICharacterInput
{
    public event Action onJumpPressed;
    public event Action onJumpReleased;
    public event Action onDashPressed;

    private PlayerInputActions _inputActions;
    
    // Track one-frame events
    private bool _jumpPressedThisFrame;
    private bool _jumpReleasedThisFrame;
    private bool _dashPressedThisFrame;
    
    // Track if we need to clear flags (set after they've been read)
    private bool _clearFlagsNextFixedUpdate;

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
        _dashPressedThisFrame = true;
        onDashPressed?.Invoke();
    }

    private void JumpCanceled(InputAction.CallbackContext context)
    {
        _jumpReleasedThisFrame = true;
        onJumpReleased?.Invoke();
    }

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        _jumpPressedThisFrame = true;
        onJumpPressed?.Invoke();
    }
    
    private void FixedUpdate()
    {
        // Clear flags from previous frame if they were set
        if (_clearFlagsNextFixedUpdate)
        {
            _jumpPressedThisFrame = false;
            _jumpReleasedThisFrame = false;
            _dashPressedThisFrame = false;
            _clearFlagsNextFixedUpdate = false;
        }
        
        // Mark for clearing next FixedUpdate if any flags are set
        if (_jumpPressedThisFrame || _jumpReleasedThisFrame || _dashPressedThisFrame)
        {
            _clearFlagsNextFixedUpdate = true;
        }
    }
    
    public InputState GetInputState()
    {
        return new InputState
        {
            Horizontal = _inputActions.Player.Horizontal.ReadValue<float>(),
            Vertical = _inputActions.Player.Vertical.ReadValue<float>(),
            JumpHeld = _inputActions.Player.Jump.inProgress,
            GrabHeld = _inputActions.Player.WallGrab.inProgress,
            JumpPressed = _jumpPressedThisFrame,
            JumpReleased = _jumpReleasedThisFrame,
            DashPressed = _dashPressedThisFrame
        };
    }

    private void OnDestroy()
    {
        _inputActions.Player.Jump.performed -= JumpPerformed;
        _inputActions.Player.Jump.canceled -= JumpCanceled;
        _inputActions.Player.Dash.performed -= DashPerformed;
    }
}

