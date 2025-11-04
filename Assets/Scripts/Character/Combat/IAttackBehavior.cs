// Interface for attack behavior implementations.
// Keeps attack logic modular and testable.

public interface IAttackBehavior
{
    /// <summary>
    /// Called when the attack is triggered. Returns true if attack executed successfully.
    /// </summary>
    bool Execute(AttackContext context);
    
    /// <summary>
    /// Called every FixedUpdate while the attack is active.
    /// </summary>
    void FixedTick(AttackContext context);
    
    /// <summary>
    /// Can this attack be executed right now? (Check cooldowns, resources, etc.)
    /// </summary>
    bool CanExecute(AttackContext context);
    
    /// <summary>
    /// Duration of this attack in frames (for determinism).
    /// </summary>
    int DurationFrames { get; }
    
    /// <summary>
    /// Can movement or other attacks interrupt this attack?
    /// </summary>
    bool CanBeInterrupted { get; }
}

