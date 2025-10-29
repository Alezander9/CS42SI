// Central registry of all tile types and their properties - hardcoded for version control

using UnityEngine;
using UnityEngine.Tilemaps;
using Core.ColorSystem;

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
            VisualTile = null,
            ColorDef = new TileColorDefinition(0f, ColorManager.ColorTier.Background)
        },
        
        // Dirt - warm brown
        new TileProperties
        {
            Type = TileType.Dirt,
            Name = "Dirt",
            MaxHealth = 100,
            IsDestructible = true,
            HasCollision = true,
            VisualTile = null,  // Assign in RoomGenerator
            ColorDef = new TileColorDefinition(30f, ColorManager.ColorTier.Foreground)  // Warm brown/orange
        },
        
        // Stone - cool gray
        new TileProperties
        {
            Type = TileType.Stone,
            Name = "Stone",
            MaxHealth = 200,
            IsDestructible = true,
            HasCollision = true,
            VisualTile = null,  // Assign in RoomGenerator
            ColorDef = new TileColorDefinition(220f, ColorManager.ColorTier.Foreground)  // Cool blue-gray
        },
        
        // Bedrock - dark desaturated
        new TileProperties
        {
            Type = TileType.Bedrock,
            Name = "Bedrock",
            MaxHealth = 999999,
            IsDestructible = false,
            HasCollision = true,
            VisualTile = null,  // Assign in RoomGenerator
            ColorDef = new TileColorDefinition(240f, ColorManager.ColorTier.Background)  // Very dark blue-gray
        },
        
        // Portal
        new TileProperties
        {
            Type = TileType.Portal,
            Name = "Portal",
            MaxHealth = 0,
            IsDestructible = false,
            HasCollision = false,
            VisualTile = null,  // Portals use GameObjects, not tiles
            ColorDef = new TileColorDefinition(280f, ColorManager.ColorTier.Special)  // Purple/magenta
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
    
    /// <summary>
    /// Get the color for a tile type using ColorManager
    /// </summary>
    public static Color GetTileColor(TileType type)
    {
        TileColorDefinition colorDef = _tiles[(int)type].ColorDef;
        return ColorManager.CreateColorFromTier(colorDef.Tier, colorDef.Hue);
    }
}

