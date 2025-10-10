// Interface for character input abstraction.
// Allows different input sources (user, AI, replay, network) to control characters.

using System;

public interface ICharacterInput
{
    /// <summary>
    /// Gets the current input state. Single source of truth for all input data.
    /// </summary>
    InputState GetInputState();
    
    // Events for button presses and releases
    event Action onJumpPressed;
    event Action onJumpReleased;
    event Action onDashPressed;
}

