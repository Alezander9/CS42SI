// Serializes and deserializes room state to disk

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class RoomSerializer
{
    /// <summary>
    /// Serialize room to state object (compares current to original)
    /// </summary>
    public static SerializedRoomState SerializeRoom(int roomX, int roomY, int seed, 
        TileType[,] originalGrid, TileType[,] currentGrid, TileDamage[] damagedTiles, int width, int height)
    {
        SerializedRoomState state = new SerializedRoomState
        {
            RoomX = roomX,
            RoomY = roomY,
            Seed = seed,
            ModifiedTiles = GetModifiedTiles(originalGrid, currentGrid, width, height),
            DamagedTiles = damagedTiles
        };
        
        return state;
    }
    
    /// <summary>
    /// Apply saved state to a freshly generated room grid
    /// </summary>
    public static void ApplyRoomState(SerializedRoomState state, TileType[,] grid)
    {
        if (state == null || state.ModifiedTiles == null)
            return;
        
        foreach (TileChange change in state.ModifiedTiles)
        {
            grid[change.X, change.Y] = change.NewType;
        }
    }
    
    /// <summary>
    /// Save room state to disk as JSON
    /// </summary>
    public static void SaveRoomToDisk(SerializedRoomState state, string savePath)
    {
        string directory = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        string json = JsonUtility.ToJson(state, true);
        File.WriteAllText(savePath, json);
    }
    
    /// <summary>
    /// Load room state from disk
    /// </summary>
    public static SerializedRoomState LoadRoomFromDisk(string savePath)
    {
        if (!File.Exists(savePath))
            return null;
        
        string json = File.ReadAllText(savePath);
        return JsonUtility.FromJson<SerializedRoomState>(json);
    }
    
    /// <summary>
    /// Check if room has saved state on disk
    /// </summary>
    public static bool HasSavedState(string savePath)
    {
        return File.Exists(savePath);
    }
    
    private static TileChange[] GetModifiedTiles(TileType[,] originalGrid, TileType[,] currentGrid, int width, int height)
    {
        List<TileChange> changes = new List<TileChange>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (currentGrid[x, y] != originalGrid[x, y])
                {
                    changes.Add(new TileChange 
                    { 
                        X = x,
                        Y = y,
                        NewType = currentGrid[x, y]
                    });
                }
            }
        }
        
        return changes.ToArray();
    }
}

