// Pickaxe swing attack - basic melee attack with forward-facing hitbox.

using UnityEngine;

public class PickaxeSwing : IAttackBehavior
{
    private Hitbox _hitbox;
    private int _hitboxStartFrame = 2;
    private int _hitboxEndFrame = 5;
    
    public int DurationFrames => 8;
    public bool CanBeInterrupted => false;
    
    public bool CanExecute(AttackContext context)
    {
        return true;
    }
    
    public bool Execute(AttackContext context)
    {
        // Create hitbox when attack starts
        _hitbox = new Hitbox(context.Transform, context.FacingDirection)
        {
            Shape = HitboxShape.Capsule,
            LocalOffset = new Vector2(0.8f, 0.2f),   // In front of character
            Size = new Vector2(0.4f, 1.2f),          // radius=0.4, height=1.2
            CollisionLayers = LayerMask.GetMask("Default")
        };
        
        Debug.Log("[ATTACK] Pickaxe Swing started!");
        return true;
    }
    
    public void FixedTick(AttackContext context)
    {
        int frame = context.CurrentFrame;
        
        // Update hitbox facing in case character turned
        _hitbox.SetFacing(context.FacingDirection);
        
        // Check hitbox during active frames
        if (frame >= _hitboxStartFrame && frame <= _hitboxEndFrame)
        {
            Collider2D[] hits = _hitbox.Check();
            
            foreach (var hit in hits)
            {
                OnHit(hit, context);
            }
            
            // Show debug visualization
            _hitbox.ShowDebugVisual(new Color(1f, 0f, 0f, 1f));
        }
        else
        {
            // Clean up visual when hitbox is not active
            _hitbox.DestroyDebugVisual();
        }
        
        // Clean up on final frame
        if (frame >= DurationFrames - 1)
        {
            _hitbox.DestroyDebugVisual();
        }
    }
    
    private void OnHit(Collider2D target, AttackContext context)
    {
        Debug.Log($"[PICKAXE] Hit {target.name}!");
        
        // TODO: Apply damage
        // var damageable = target.GetComponent<IDamageable>();
        // damageable?.TakeDamage(10);
        
        // TODO: Apply knockback
        // var rb = target.GetComponent<Rigidbody2D>();
        // if (rb != null)
        // {
        //     Vector2 knockback = new Vector2(5f * context.FacingDirection, 3f);
        //     rb.AddForce(knockback, ForceMode2D.Impulse);
        // }
    }
}

