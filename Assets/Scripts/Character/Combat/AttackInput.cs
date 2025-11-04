// Defines attack context and input combinations for the combat system.

using System;

public enum GroundState
{
    Grounded,
    Airborne
}

public enum AttackDirection
{
    Neutral,    // No direction input
    Side,       // Left or Right
    Up,         // Up
    Down        // Down
}

public enum AttackType
{
    Light,
    Heavy
}

/// <summary>
/// Unique identifier for any attack combination.
/// Combines ground state, direction, and attack type to determine which attack to execute.
/// </summary>
[Serializable]
public struct AttackInput
{
    public GroundState GroundState;
    public AttackDirection Direction;
    public AttackType AttackType;
    
    public AttackInput(GroundState groundState, AttackDirection direction, AttackType attackType)
    {
        GroundState = groundState;
        Direction = direction;
        AttackType = attackType;
    }
    
    /// <summary>
    /// Returns a readable name for this attack combination (e.g., "GroundedSideLight").
    /// </summary>
    public string GetAttackName() => $"{GroundState}{Direction}{AttackType}";
    
    public override int GetHashCode() => HashCode.Combine(GroundState, Direction, AttackType);
    
    public override bool Equals(object obj)
    {
        if (obj is AttackInput other)
        {
            return GroundState == other.GroundState && 
                   Direction == other.Direction && 
                   AttackType == other.AttackType;
        }
        return false;
    }
}

