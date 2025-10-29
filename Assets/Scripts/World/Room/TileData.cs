// Core tile type definitions and properties

using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType : byte // byte is 8 bits, so we can have 256 different tile types
{
    Air = 0,
    Dirt = 1,
    Stone = 2,
    Bedrock = 3,
    Portal = 4  // Grid marker only, not rendered as tile
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
}

