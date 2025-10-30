// Core tile type definitions and properties

using UnityEngine;
using UnityEngine.Tilemaps;
using Core.ColorSystem;

public enum TileType : byte // byte is 8 bits, so we can have 256 different tile types
{
    Air = 0,
    Dirt = 1,
    Stone = 2,
    Bedrock = 3,
    Portal = 4  // Grid marker only, not rendered as tile
}

[System.Serializable]
public struct TileColorDefinition
{
    public float Hue;           // 0-360 degrees
    public float Chroma;        // Saturation (will be clamped to tier's valid range)
    public ColorManager.ColorTier Tier;  // Background/Foreground/Special
    
    public TileColorDefinition(float hue, float chroma, ColorManager.ColorTier tier)
    {
        Hue = hue;
        Chroma = chroma;
        Tier = tier;
    }
}

[System.Serializable]
public struct TileProperties
{
    public TileType Type;
    public string Name;
    public int MaxHealth;
    public bool IsDestructible;
    public bool HasCollision;
    public TileBase VisualTile;
    public TileColorDefinition ColorDef;
}

