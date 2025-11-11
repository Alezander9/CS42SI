// Pickaxe attacks - quick, short-range melee weapon.

using UnityEngine;

/// <summary>
/// Shared constants and helpers for all pickaxe attacks.
/// </summary>
public static class PickaxeData
{
    public const int HitboxStartFrame = 2;
    public const int HitboxEndFrame = 10;
    public const int AttackDuration = 14;
    public const float HitboxRadius = 0.4f;
    public const int TileDamage = 50;
    
    public static void DamageTiles(Collider2D target, Hitbox hitbox)
    {
        var tilemap = target.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        
        if (tilemap != null)
        {
            WorldManager worldManager = UnityEngine.Object.FindObjectOfType<WorldManager>();
            if (worldManager != null)
            {
                worldManager.InflictTileDamage(hitbox.GetWorldBounds(), TileDamage);
            }
        }
    }
    
    public static void TickHitbox(Hitbox hitbox, AttackContext context, int durationFrames)
    {
        int frame = context.CurrentFrame;
        hitbox.SetFacing(context.FacingDirection);
        
        if (frame >= HitboxStartFrame && frame <= HitboxEndFrame)
        {
            Collider2D[] hits = hitbox.Check();
            foreach (var hit in hits)
            {
                DamageTiles(hit, hitbox);
            }
            hitbox.ShowDebugVisual();
        }
        else
        {
            hitbox.DestroyDebugVisual();
        }
        
        if (frame >= durationFrames - 1)
        {
            hitbox.DestroyDebugVisual();
        }
    }
}

public class PickaxeGroundedNeutral : IAttackBehavior
{
    private Hitbox _hitbox;
    
    public int DurationFrames => PickaxeData.AttackDuration;
    public bool CanBeInterrupted => false;
    
    public bool CanExecute(AttackContext context)
    {
        return true;
    }
    
    public bool Execute(AttackContext context)
    {
        _hitbox = new Hitbox(context.Transform, context.FacingDirection)
        {
            Shape = HitboxShape.Capsule,
            LocalOffset = new Vector2(0.8f, 0.2f),
            Size = new Vector2(PickaxeData.HitboxRadius, 1.2f),
            CollisionLayers = LayerMask.GetMask("Ground")
        };
        
        return true;
    }
    
    public void FixedTick(AttackContext context)
    {
        PickaxeData.TickHitbox(_hitbox, context, DurationFrames);
    }
}

public class PickaxeGroundedSide : IAttackBehavior
{
    private Hitbox _hitbox;
    
    public int DurationFrames => PickaxeData.AttackDuration;
    public bool CanBeInterrupted => false;
    
    public bool CanExecute(AttackContext context)
    {
        return true;
    }
    
    public bool Execute(AttackContext context)
    {
        _hitbox = new Hitbox(context.Transform, context.FacingDirection)
        {
            Shape = HitboxShape.Capsule,
            LocalOffset = new Vector2(1.0f, 0.0f),
            Size = new Vector2(PickaxeData.HitboxRadius, 1.0f),
            CollisionLayers = LayerMask.GetMask("Ground")
        };
        
        return true;
    }
    
    public void FixedTick(AttackContext context)
    {
        PickaxeData.TickHitbox(_hitbox, context, DurationFrames);
    }
}

public class PickaxeGroundedUp : IAttackBehavior
{
    private Hitbox _hitbox;
    
    public int DurationFrames => PickaxeData.AttackDuration;
    public bool CanBeInterrupted => false;
    
    public bool CanExecute(AttackContext context)
    {
        return true;
    }
    
    public bool Execute(AttackContext context)
    {
        _hitbox = new Hitbox(context.Transform, context.FacingDirection)
        {
            Shape = HitboxShape.Capsule,
            LocalOffset = new Vector2(0.3f, 1.2f),
            Size = new Vector2(PickaxeData.HitboxRadius, 1.0f),
            CollisionLayers = LayerMask.GetMask("Ground")
        };
        
        return true;
    }
    
    public void FixedTick(AttackContext context)
    {
        PickaxeData.TickHitbox(_hitbox, context, DurationFrames);
    }
}

public class PickaxeGroundedDown : IAttackBehavior
{
    private Hitbox _hitbox;
    
    public int DurationFrames => PickaxeData.AttackDuration;
    public bool CanBeInterrupted => false;
    
    public bool CanExecute(AttackContext context)
    {
        return true;
    }
    
    public bool Execute(AttackContext context)
    {
        _hitbox = new Hitbox(context.Transform, context.FacingDirection)
        {
            Shape = HitboxShape.Capsule,
            LocalOffset = new Vector2(0.45f, -1.0f),
            Size = new Vector2(PickaxeData.HitboxRadius, 0.8f),
            CollisionLayers = LayerMask.GetMask("Ground")
        };
        
        return true;
    }
    
    public void FixedTick(AttackContext context)
    {
        PickaxeData.TickHitbox(_hitbox, context, DurationFrames);
    }
}
