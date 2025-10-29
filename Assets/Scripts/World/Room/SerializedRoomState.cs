// Data structures for room serialization - stores only changes from procedural generation

using UnityEngine;

[System.Serializable]
public class SerializedRoomState
{
    public int RoomX;
    public int RoomY;
    public int Seed;
    
    // Only tiles that differ from original generation
    public TileChange[] ModifiedTiles;
    
    // Only tiles with damage
    public TileDamage[] DamagedTiles;
}

[System.Serializable]
public struct TileChange
{
    public int X;
    public int Y;
    public TileType NewType;
}

[System.Serializable]
public struct TileDamage
{
    public int X;
    public int Y;
    public int CurrentDamage;
    public TileType OriginalType;
}

