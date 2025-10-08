// Temporary debug script to diagnose input issues.
// Add this to your Player, check Console for debug output.

using UnityEngine;

public class InputDebugger : MonoBehaviour
{
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        
        if (_playerInput == null)
        {
            Debug.LogError("PlayerInput component not found!");
        }
        else
        {
            Debug.Log("PlayerInput component found!");
        }
    }

    private void Start()
    {
        if (_playerInput != null)
        {
            _playerInput.onJumpPressed += () => Debug.Log("Jump Pressed!");
            _playerInput.onJumpReleased += () => Debug.Log("Jump Released!");
            _playerInput.onDashPressed += () => Debug.Log("Dash Pressed!");
        }
    }

    private void Update()
    {
        if (_playerInput != null)
        {
            float horizontal = _playerInput.GetHorizontalInput();
            float vertical = _playerInput.GetVerticalInput();
            bool jump = _playerInput.IsJumpPressed();
            bool grab = _playerInput.IsGrabPressed();

            // Only log when there's input
            if (horizontal != 0)
                Debug.Log($"Horizontal: {horizontal}");
            if (vertical != 0)
                Debug.Log($"Vertical: {vertical}");
            if (jump)
                Debug.Log("Jump is pressed (inProgress)");
            if (grab)
                Debug.Log("Grab is pressed (inProgress)");
        }
    }
}

