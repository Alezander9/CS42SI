// Handles tile damage and destruction

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainDamageSystem
{
    private TileType[,] _grid;
    private Dictionary<Vector2Int, TileDamage> _damagedTiles;
    private Tilemap _tilemap;
    private int _width;
    private int _height;
    
    public TerrainDamageSystem(TileType[,] grid, Tilemap tilemap, int width, int height)
    {
        _grid = grid;
        _tilemap = tilemap;
        _width = width;
        _height = height;
        _damagedTiles = new Dictionary<Vector2Int, TileDamage>();
    }
    
    /// <summary>
    /// Damage a tile at grid position. Returns true if tile was destroyed.
    /// </summary>
    public bool DamageTile(int x, int y, int damageAmount)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return false;
        
        TileType tileType = _grid[x, y];
        
        if (!TileDatabase.IsDestructible(tileType))
            return false;
        
        Vector2Int pos = new Vector2Int(x, y);
        
        // Get or create damage entry
        if (!_damagedTiles.TryGetValue(pos, out TileDamage damage))
        {
            damage = new TileDamage 
            { 
                X = x,
                Y = y,
                CurrentDamage = 0, 
                OriginalType = tileType 
            };
        }
        
        damage.CurrentDamage += damageAmount;
        int maxHealth = TileDatabase.GetMaxHealth(tileType);
        
        if (damage.CurrentDamage >= maxHealth)
        {
            // Destroy tile
            _grid[x, y] = TileType.Air;
            _damagedTiles.Remove(pos);
            UpdateTileVisual(x, y, TileType.Air);
            return true;
        }
        else
        {
            // Update damage
            _damagedTiles[pos] = damage;
            return false;
        }
    }
    
    /// <summary>
    /// Get damage percentage for a tile (0.0 to 1.0)
    /// </summary>
    public float GetDamagePercent(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        if (_damagedTiles.TryGetValue(pos, out TileDamage damage))
        {
            int maxHealth = TileDatabase.GetMaxHealth(damage.OriginalType);
            return (float)damage.CurrentDamage / maxHealth;
        }
        return 0f;
    }
    
    /// <summary>
    /// Get all damaged tiles for serialization
    /// </summary>
    public TileDamage[] GetDamagedTiles()
    {
        TileDamage[] result = new TileDamage[_damagedTiles.Count];
        _damagedTiles.Values.CopyTo(result, 0);
        return result;
    }
    
    /// <summary>
    /// Restore damaged tiles from saved state
    /// </summary>
    public void RestoreDamagedTiles(TileDamage[] damagedTiles)
    {
        _damagedTiles.Clear();
        
        if (damagedTiles == null)
            return;
        
        foreach (TileDamage damage in damagedTiles)
        {
            Vector2Int pos = new Vector2Int(damage.X, damage.Y);
            _damagedTiles[pos] = damage;
        }
    }
    
    private void UpdateTileVisual(int x, int y, TileType type)
    {
        Vector3Int tilePosition = new Vector3Int(x, y, 0);
        
        if (type == TileType.Air || type == TileType.Portal)
        {
            _tilemap.SetTile(tilePosition, null);
        }
        else
        {
            TileBase visualTile = TileDatabase.GetVisualTile(type);
            if (visualTile != null)
            {
                _tilemap.SetTile(tilePosition, visualTile);
            }
        }
    }
}

