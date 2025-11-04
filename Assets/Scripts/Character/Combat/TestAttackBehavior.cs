// Simple test attack behavior that logs to console.
// Used for proof of concept testing before implementing real attacks.

using UnityEngine;

public class TestAttackBehavior : IAttackBehavior
{
    private readonly string _attackName;
    private readonly int _durationFrames;
    private readonly bool _canBeInterrupted;
    
    public int DurationFrames => _durationFrames;
    public bool CanBeInterrupted => _canBeInterrupted;
    
    public TestAttackBehavior(string attackName, int durationFrames = 30, bool canBeInterrupted = true)
    {
        _attackName = attackName;
        _durationFrames = durationFrames;
        _canBeInterrupted = canBeInterrupted;
    }
    
    public bool CanExecute(AttackContext context)
    {
        return true; // Test attacks can always execute
    }
    
    public bool Execute(AttackContext context)
    {
        Debug.Log($"[ATTACK] Executing: {_attackName} | Facing: {context.FacingDirection} | Frame: {context.CurrentFrame}");
        return true;
    }
    
    public void FixedTick(AttackContext context)
    {
        // Log every 15 frames to avoid spam
        if (context.CurrentFrame % 15 == 0)
        {
            Debug.Log($"[ATTACK] {_attackName} in progress... Frame {context.CurrentFrame}/{_durationFrames}");
        }
    }
}

