// Interface for character input abstraction.
// Allows different input sources (user, AI, replay, network) to control characters.

using System;

public interface ICharacterInput
{
    // Axis inputs
    float GetHorizontalInput();
    float GetVerticalInput();
    
    // Button state queries
    bool IsJumpPressed();
    bool IsGrabPressed();
    
    // Events for button presses and releases
    event Action onJumpPressed;
    event Action onJumpReleased;
    event Action onDashPressed;
}

