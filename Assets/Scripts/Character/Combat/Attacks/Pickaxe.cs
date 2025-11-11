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
        int frame = context.CurrentFrame;
        _hitbox.SetFacing(context.FacingDirection);
        
        if (frame >= PickaxeData.HitboxStartFrame && frame <= PickaxeData.HitboxEndFrame)
        {
            Collider2D[] hits = _hitbox.Check();
            foreach (var hit in hits)
            {
                OnHit(hit, context, _hitbox);
            }
            _hitbox.ShowDebugVisual();
        }
        else
        {
            _hitbox.DestroyDebugVisual();
        }
        
        if (frame >= DurationFrames - 1)
        {
            _hitbox.DestroyDebugVisual();
        }
    }
    
    private void OnHit(Collider2D target, AttackContext context, Hitbox hitbox)
    {
        var tilemap = target.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        
        if (tilemap != null)
        {
            WorldManager worldManager = UnityEngine.Object.FindObjectOfType<WorldManager>();
            if (worldManager != null)
            {
                worldManager.InflictTileDamage(hitbox.GetWorldBounds(), 10);
            }
        }
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
            CollisionLayers = LayerMask.GetMask("Default")
        };
        
        return true;
    }
    
    public void FixedTick(AttackContext context)
    {
        int frame = context.CurrentFrame;
        _hitbox.SetFacing(context.FacingDirection);
        
        if (frame >= PickaxeData.HitboxStartFrame && frame <= PickaxeData.HitboxEndFrame)
        {
            Collider2D[] hits = _hitbox.Check();
            foreach (var hit in hits)
            {
                OnHit(hit, context);
            }
            _hitbox.ShowDebugVisual();
        }
        else
        {
            _hitbox.DestroyDebugVisual();
        }
        
        if (frame >= DurationFrames - 1)
        {
            _hitbox.DestroyDebugVisual();
        }
    }
    
    private void OnHit(Collider2D target, AttackContext context)
    {
        // TODO: Apply damage
        // TODO: Apply knockback
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
            CollisionLayers = LayerMask.GetMask("Default")
        };
        
        return true;
    }
    
    public void FixedTick(AttackContext context)
    {
        int frame = context.CurrentFrame;
        _hitbox.SetFacing(context.FacingDirection);
        
        if (frame >= PickaxeData.HitboxStartFrame && frame <= PickaxeData.HitboxEndFrame)
        {
            Collider2D[] hits = _hitbox.Check();
            foreach (var hit in hits)
            {
                OnHit(hit, context);
            }
            _hitbox.ShowDebugVisual();
        }
        else
        {
            _hitbox.DestroyDebugVisual();
        }
        
        if (frame >= DurationFrames - 1)
        {
            _hitbox.DestroyDebugVisual();
        }
    }
    
    private void OnHit(Collider2D target, AttackContext context)
    {
        // TODO: Apply damage
        // TODO: Apply knockback
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
            LocalOffset = new Vector2(0.6f, -0.6f),
            Size = new Vector2(PickaxeData.HitboxRadius, 0.8f),
            CollisionLayers = LayerMask.GetMask("Default")
        };
        
        return true;
    }
    
    public void FixedTick(AttackContext context)
    {
        int frame = context.CurrentFrame;
        _hitbox.SetFacing(context.FacingDirection);
        
        if (frame >= PickaxeData.HitboxStartFrame && frame <= PickaxeData.HitboxEndFrame)
        {
            Collider2D[] hits = _hitbox.Check();
            foreach (var hit in hits)
            {
                OnHit(hit, context);
            }
            _hitbox.ShowDebugVisual();
        }
        else
        {
            _hitbox.DestroyDebugVisual();
        }
        
        if (frame >= DurationFrames - 1)
        {
            _hitbox.DestroyDebugVisual();
        }
    }
    
    private void OnHit(Collider2D target, AttackContext context)
    {
        // TODO: Apply damage
        // TODO: Apply knockback
    }
}

