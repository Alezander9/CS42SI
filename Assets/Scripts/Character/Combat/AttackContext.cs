// Context information passed to attack behaviors.
// Provides access to character state and methods for attacks to influence movement.

using UnityEngine;

/// <summary>
/// Contains all the information an attack behavior needs to execute.
/// Passed to attack behaviors during Execute() and FixedTick().
/// </summary>
public class AttackContext
{
    public CharacterController Character { get; set; }
    public CharacterCollision Collision { get; set; }
    public Transform Transform { get; set; }
    public int CurrentFrame { get; set; }
    public Vector2 InputDirection { get; set; }
    public int FacingDirection { get; set; } // -1 for left, 1 for right
}

