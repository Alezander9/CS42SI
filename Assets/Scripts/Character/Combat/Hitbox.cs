// Helper class for attack hitbox collision detection and tracking.
// Encapsulates collision logic while letting attacks control response behavior.

using System.Collections.Generic;
using UnityEngine;

public enum HitboxShape
{
    Capsule,
    Box,
    Circle
}

/// <summary>
/// Helper class that handles hitbox collision detection and tracking.
/// Attack behaviors create and use these to detect hits without writing collision code.
/// </summary>
public class Hitbox
{
    public HitboxShape Shape { get; set; }
    public Vector2 LocalOffset { get; set; }
    public Vector2 Size { get; set; }  // Capsule: x=radius, y=height; Box: width/height; Circle: x=radius
    public LayerMask CollisionLayers { get; set; }
    
    // Static debug flag (can be toggled globally)
    public static bool ShowDebugVisuals = false;
    public static Color DebugVisualColor = new Color(1f, 0f, 0f, 0.5f); // Red with 50% transparency
    
    private Transform _owner;
    private int _facingDirection;
    private HashSet<Collider2D> _alreadyHit = new HashSet<Collider2D>();
    private GameObject _debugVisual;
    
    /// <summary>
    /// Creates a new hitbox attached to an owner transform.
    /// </summary>
    public Hitbox(Transform owner, int facingDirection = 1)
    {
        _owner = owner;
        _facingDirection = facingDirection;
    }
    
    /// <summary>
    /// Updates the facing direction (useful if character turns during attack).
    /// </summary>
    public void SetFacing(int facingDirection)
    {
        _facingDirection = facingDirection;
    }
    
    /// <summary>
    /// Clears the list of already-hit colliders. Call this when reusing hitboxes.
    /// </summary>
    public void ResetHits()
    {
        _alreadyHit.Clear();
    }
    
    /// <summary>
    /// Gets the world position of the hitbox center, accounting for facing direction.
    /// </summary>
    public Vector2 GetWorldPosition()
    {
        Vector2 offset = LocalOffset;
        offset.x *= _facingDirection;
        return (Vector2)_owner.position + offset;
    }
    
    /// <summary>
    /// Checks for collisions and returns NEW hits (excludes already-hit targets).
    /// Returns empty array if nothing new was hit.
    /// </summary>
    public Collider2D[] Check()
    {
        Vector2 worldPos = GetWorldPosition();
        Collider2D[] allHits = GetCollisions(worldPos);
        
        // Filter out owner and already-hit targets
        List<Collider2D> newHits = new List<Collider2D>();
        
        foreach (var hit in allHits)
        {
            // Skip if it's the owner or already hit
            if (hit.transform == _owner || _alreadyHit.Contains(hit))
                continue;
            
            _alreadyHit.Add(hit);
            newHits.Add(hit);
        }
        
        return newHits.ToArray();
    }
    
    private Collider2D[] GetCollisions(Vector2 worldPos)
    {
        switch (Shape)
        {
            case HitboxShape.Capsule:
                return Physics2D.OverlapCapsuleAll(
                    worldPos,
                    Size,
                    CapsuleDirection2D.Vertical,
                    0,
                    CollisionLayers
                );
            
            case HitboxShape.Box:
                return Physics2D.OverlapBoxAll(
                    worldPos,
                    Size,
                    0,
                    CollisionLayers
                );
            
            case HitboxShape.Circle:
                return Physics2D.OverlapCircleAll(
                    worldPos,
                    Size.x,
                    CollisionLayers
                );
            
            default:
                return new Collider2D[0];
        }
    }
    
    /// <summary>
    /// Creates or updates a visible mesh representation of the hitbox for debugging.
    /// Call this each frame while the hitbox should be visible.
    /// </summary>
    public void ShowDebugVisual()
    {
        if (!ShowDebugVisuals)
        {
            DestroyDebugVisual();
            return;
        }
        
        // Create visual if it doesn't exist
        if (_debugVisual == null)
        {
            _debugVisual = CreateDebugMesh();
        }
        
        // Update position (z = -5 to render in front)
        if (_debugVisual != null)
        {
            Vector3 pos = GetWorldPosition();
            pos.z = -5f;
            _debugVisual.transform.position = pos;
        }
    }
    
    /// <summary>
    /// Destroys the debug visual mesh. Call this when the hitbox is no longer active.
    /// </summary>
    public void DestroyDebugVisual()
    {
        if (_debugVisual != null)
        {
            Object.Destroy(_debugVisual);
            _debugVisual = null;
        }
    }
    
    private GameObject CreateDebugMesh()
    {
        GameObject visual = null;
        
        switch (Shape)
        {
            case HitboxShape.Capsule:
                visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.transform.localScale = new Vector3(Size.x * 2, Size.y / 2, Size.x * 2);
                break;
            
            case HitboxShape.Box:
                visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.transform.localScale = new Vector3(Size.x, Size.y, 0.2f);
                break;
            
            case HitboxShape.Circle:
                visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.transform.localScale = Vector3.one * Size.x * 2;
                break;
        }
        
        if (visual != null)
        {
            visual.name = "Hitbox Debug Visual";
            
            // Remove collider (we don't want the visual to interact with physics)
            Object.Destroy(visual.GetComponent<Collider>());
            
            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    Material mat = new Material(shader);
                    mat.color = DebugVisualColor;
                    renderer.material = mat;
                }
            }
        }
        
        return visual;
    }
}

