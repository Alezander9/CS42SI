// Single source of truth for all character input data.
// Used by both live input and recording/playback systems.

using System;
using UnityEngine;

/// <summary>
/// Represents the complete input state for a character at a single point in time.
/// This is the ONLY place where input fields are defined.
/// </summary>
[Serializable]
public struct InputState
{
    // Axis inputs (-1 to 1)
    public float Horizontal;
    public float Vertical;
    
    // Button states (held this frame)
    public bool JumpHeld;
    public bool GrabHeld;
    
    // One-frame events (only true on the frame they occur)
    public bool JumpPressed;
    public bool JumpReleased;
    public bool DashPressed;
    
    /// <summary>
    /// Returns a default/zero input state (no input).
    /// </summary>
    public static InputState Zero => new InputState
    {
        Horizontal = 0,
        Vertical = 0,
        JumpHeld = false,
        GrabHeld = false,
        JumpPressed = false,
        JumpReleased = false,
        DashPressed = false
    };
}

/// <summary>
/// Represents a single frame of input data for recording/playback.
/// Includes frame number for synchronization validation.
/// </summary>
[Serializable]
public struct InputFrame
{
    public int FrameNumber;
    public InputState Input;
    
    public InputFrame(int frameNumber, InputState input)
    {
        FrameNumber = frameNumber;
        Input = input;
    }
}

