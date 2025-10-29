// Central registry of all tile types and their properties - hardcoded for version control

using UnityEngine.Tilemaps;

public static class TileDatabase
{
    private static readonly TileProperties[] _tiles = new TileProperties[]
    {
        // Air
        new TileProperties
        {
            Type = TileType.Air,
            Name = "Air",
            MaxHealth = 0,
            IsDestructible = false,
            HasCollision = false,
            VisualTile = null
        },
        
        // Dirt
        new TileProperties
        {
            Type = TileType.Dirt,
            Name = "Dirt",
            MaxHealth = 100,
            IsDestructible = true,
            HasCollision = true,
            VisualTile = null  // Assign in RoomGenerator inspector
        },
        
        // Stone
        new TileProperties
        {
            Type = TileType.Stone,
            Name = "Stone",
            MaxHealth = 200,
            IsDestructible = true,
            HasCollision = true,
            VisualTile = null  // Assign in RoomGenerator inspector
        },
        
        // Bedrock
        new TileProperties
        {
            Type = TileType.Bedrock,
            Name = "Bedrock",
            MaxHealth = 999999,
            IsDestructible = false,
            HasCollision = true,
            VisualTile = null  // Assign in RoomGenerator inspector
        },
        
        // Portal
        new TileProperties
        {
            Type = TileType.Portal,
            Name = "Portal",
            MaxHealth = 0,
            IsDestructible = false,
            HasCollision = false,
            VisualTile = null  // Portals use GameObjects, not tiles
        }
    };
    
    public static TileProperties GetProperties(TileType type)
    {
        return _tiles[(int)type];
    }
    
    public static int GetMaxHealth(TileType type)
    {
        return _tiles[(int)type].MaxHealth;
    }
    
    public static bool IsDestructible(TileType type)
    {
        return _tiles[(int)type].IsDestructible;
    }
    
    public static bool HasCollision(TileType type)
    {
        return _tiles[(int)type].HasCollision;
    }
    
    public static TileBase GetVisualTile(TileType type)
    {
        return _tiles[(int)type].VisualTile;
    }
    
    /// <summary>
    /// Set visual tile references at runtime (called from RoomGenerator)
    /// </summary>
    public static void SetVisualTile(TileType type, TileBase tile)
    {
        _tiles[(int)type].VisualTile = tile;
    }
}

