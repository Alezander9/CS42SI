// Procedural cave room generator using Perlin noise + cellular automata
// Generates a single room with destructible terrain and portal placement

using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Identity")]
    [SerializeField] private int _roomX = 0;
    [SerializeField] private int _roomY = 0;
    [SerializeField] private int _globalSeed = 12345;
    
    /// <summary>
    /// Set room coordinates and seed (called by WorldManager/RoomInstance).
    /// </summary>
    public void SetRoomCoordinates(int roomX, int roomY, int globalSeed)
    {
        _roomX = roomX;
        _roomY = roomY;
        _globalSeed = globalSeed;
    }
    
    [Header("Room Dimensions")]
    [SerializeField] private int _roomWidth = 50;
    [SerializeField] private int _roomHeight = 40;
    [SerializeField] private float _blockSize = 1f;
    
    [Header("Generation Settings")]
    [SerializeField] private float _noiseScale = 0.1f;
    [SerializeField] private float _fillThreshold = 0.5f;
    [SerializeField] private int _smoothingIterations = 5;
    [SerializeField] private int _portalClearRadius = 3;
    
    [Header("References")]
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private TileBase _groundTile;
    [SerializeField] private GameObject _portalPrefab;
    
    // Tile types
    private const int AIR = 0;
    private const int GROUND = 1;
    private const int PORTAL = 2;
    
    private int[,] _grid;
    private System.Random _random;
    private Vector2Int[] _portalPositions;
    
    public void GenerateRoom()
    {
        // Initialize random with combined seed
        int combinedSeed = _globalSeed + _roomX * 1000 + _roomY;
        _random = new System.Random(combinedSeed);
        
        Debug.Log($"Generating room ({_roomX}, {_roomY}) with seed {combinedSeed}");
        
        // Phase 1: Initialize grid with Perlin noise
        InitializeGridWithNoise(combinedSeed);
        
        // Phase 2: Place portals
        PlacePortals();
        
        // Phase 3: Cellular automata smoothing
        for (int i = 0; i < _smoothingIterations; i++)
        {
            SmoothWithCellularAutomata();
        }
        
        // Phase 4: Clear areas around portals
        ClearAroundPortals();
        
        // Phase 5: Build the visual mesh/tilemap
        BuildTilemap();
        
        // Phase 6: Spawn portal sprites
        SpawnPortalSprites();
        
        Debug.Log($"Room generation complete: {CountTiles(GROUND)} ground tiles, {CountTiles(AIR)} air tiles");
    }
    
    private void InitializeGridWithNoise(int seed)
    {
        _grid = new int[_roomWidth, _roomHeight];
        
        // Use Perlin noise for initial distribution
        float offsetX = seed * 0.1f;
        float offsetY = seed * 0.2f;
        
        for (int x = 0; x < _roomWidth; x++)
        {
            for (int y = 0; y < _roomHeight; y++)
            {
                float noiseValue = Mathf.PerlinNoise(
                    (x + offsetX) * _noiseScale,
                    (y + offsetY) * _noiseScale
                );
                
                // Add vertical gradient (more solid at bottom)
                float verticalGradient = 1f - (y / (float)_roomHeight);
                noiseValue = noiseValue * 0.7f + verticalGradient * 0.3f;
                
                _grid[x, y] = noiseValue > _fillThreshold ? GROUND : AIR;
            }
        }
    }
    
    private void PlacePortals()
    {
        _portalPositions = new Vector2Int[3];
        
        // Left edge portal (random Y in middle 60% of height)
        int leftY = _random.Next((int)(_roomHeight * 0.2f), (int)(_roomHeight * 0.8f));
        _portalPositions[0] = new Vector2Int(2, leftY);
        
        // Right edge portal (random Y in middle 60% of height)
        int rightY = _random.Next((int)(_roomHeight * 0.2f), (int)(_roomHeight * 0.8f));
        _portalPositions[1] = new Vector2Int(_roomWidth - 3, rightY);
        
        // Center portal (random position in middle 50% of room)
        int centerX = _random.Next((int)(_roomWidth * 0.25f), (int)(_roomWidth * 0.75f));
        int centerY = _random.Next((int)(_roomHeight * 0.3f), (int)(_roomHeight * 0.7f));
        _portalPositions[2] = new Vector2Int(centerX, centerY);
        
        // Mark portal positions in grid
        foreach (var pos in _portalPositions)
        {
            if (IsValidPosition(pos.x, pos.y))
            {
                _grid[pos.x, pos.y] = PORTAL;
            }
        }
    }
    
    private void SmoothWithCellularAutomata()
    {
        int[,] newGrid = new int[_roomWidth, _roomHeight];
        
        for (int x = 0; x < _roomWidth; x++)
        {
            for (int y = 0; y < _roomHeight; y++)
            {
                // Skip portal tiles - they're protected
                if (_grid[x, y] == PORTAL)
                {
                    newGrid[x, y] = PORTAL;
                    continue;
                }
                
                int groundNeighbors = CountGroundNeighbors(x, y);
                
                // Standard cellular automata rules
                // If 5+ ground neighbors, become ground
                // If 4 or fewer, become air
                if (groundNeighbors >= 5)
                {
                    newGrid[x, y] = GROUND;
                }
                else if (groundNeighbors <= 3)
                {
                    newGrid[x, y] = AIR;
                }
                else
                {
                    // Keep current state for boundary cases
                    newGrid[x, y] = _grid[x, y];
                }
                
                // Edge tiles are always walls
                if (x == 0 || x == _roomWidth - 1 || y == 0 || y == _roomHeight - 1)
                {
                    newGrid[x, y] = GROUND;
                }
            }
        }
        
        _grid = newGrid;
    }
    
    private int CountGroundNeighbors(int x, int y)
    {
        int count = 0;
        
        // Check 8 neighbors (including diagonals)
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                if (offsetX == 0 && offsetY == 0)
                    continue;
                
                int neighborX = x + offsetX;
                int neighborY = y + offsetY;
                
                // Out of bounds counts as ground (encourages walls at edges)
                if (!IsValidPosition(neighborX, neighborY))
                {
                    count++;
                    continue;
                }
                
                if (_grid[neighborX, neighborY] == GROUND)
                {
                    count++;
                }
            }
        }
        
        return count;
    }
    
    private void ClearAroundPortals()
    {
        foreach (var portalPos in _portalPositions)
        {
            // Clear a sphere of air around each portal
            for (int offsetX = -_portalClearRadius; offsetX <= _portalClearRadius; offsetX++)
            {
                for (int offsetY = -_portalClearRadius; offsetY <= _portalClearRadius; offsetY++)
                {
                    int x = portalPos.x + offsetX;
                    int y = portalPos.y + offsetY;
                    
                    if (!IsValidPosition(x, y))
                        continue;
                    
                    // Calculate distance from portal
                    float distance = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);
                    
                    if (distance <= _portalClearRadius)
                    {
                        // Don't overwrite the portal itself
                        if (_grid[x, y] != PORTAL)
                        {
                            _grid[x, y] = AIR;
                        }
                    }
                }
            }
        }
    }
    
    private void BuildTilemap()
    {
        if (_tilemap == null)
        {
            Debug.LogError("Tilemap reference is missing! Please assign in inspector.");
            return;
        }
        
        if (_groundTile == null)
        {
            Debug.LogError("Ground tile reference is missing! Please assign in inspector.");
            return;
        }
        
        // Clear existing tiles
        _tilemap.ClearAllTiles();
        
        // Build tilemap from grid
        for (int x = 0; x < _roomWidth; x++)
        {
            for (int y = 0; y < _roomHeight; y++)
            {
                if (_grid[x, y] == GROUND)
                {
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);
                    _tilemap.SetTile(tilePosition, _groundTile);
                }
            }
        }
        
        Debug.Log($"Tilemap built with {_roomWidth}x{_roomHeight} tiles");
    }
    
    private void SpawnPortalSprites()
    {
        if (_portalPrefab == null)
        {
            Debug.LogWarning("Portal prefab is missing! Skipping portal spawn.");
            return;
        }
        
        if (_tilemap == null)
        {
            Debug.LogError("Cannot spawn portals: Tilemap reference is missing!");
            return;
        }
        
        // Clear existing portals
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Portal"))
            {
                Destroy(child.gameObject);
            }
        }
        
        // Spawn new portals using tilemap coordinate conversion
        for (int i = 0; i < _portalPositions.Length; i++)
        {
            Vector2Int gridPos = _portalPositions[i];
            
            // Convert grid cell coordinates to world space using Tilemap
            Vector3Int cellPos = new Vector3Int(gridPos.x, gridPos.y, 0);
            Vector3 worldPos = _tilemap.GetCellCenterWorld(cellPos);
            
            GameObject portal = Instantiate(_portalPrefab, worldPos, Quaternion.identity, transform);
            portal.name = $"Portal_{i}_{(i == 0 ? "Left" : i == 1 ? "Right" : "Center")}";
            
            Debug.Log($"Spawned portal {i} at grid ({gridPos.x}, {gridPos.y}) -> world {worldPos}");
        }
    }
    
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < _roomWidth && y >= 0 && y < _roomHeight;
    }
    
    private int CountTiles(int tileType)
    {
        int count = 0;
        for (int x = 0; x < _roomWidth; x++)
        {
            for (int y = 0; y < _roomHeight; y++)
            {
                if (_grid[x, y] == tileType)
                    count++;
            }
        }
        return count;
    }
    
    // Public API for future terrain destruction
    public int GetTileAt(int x, int y)
    {
        if (!IsValidPosition(x, y))
            return GROUND; // Out of bounds is solid
        
        return _grid[x, y];
    }
    
    public void SetTileAt(int x, int y, int tileType)
    {
        if (!IsValidPosition(x, y))
            return;
        
        _grid[x, y] = tileType;
        
        // Update visual representation
        Vector3Int tilePosition = new Vector3Int(x, y, 0);
        if (tileType == GROUND && _groundTile != null)
        {
            _tilemap.SetTile(tilePosition, _groundTile);
        }
        else
        {
            _tilemap.SetTile(tilePosition, null);
        }
    }
    
    /// <summary>
    /// Returns all three portal world positions. [0]=Left, [1]=Right, [2]=Center
    /// </summary>
    public Vector3[] GetPortalWorldPositions()
    {
        if (_portalPositions == null || _portalPositions.Length < 3)
        {
            Debug.LogWarning("Room not yet generated or portal positions not set");
            return new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
        }
        
        if (_tilemap == null)
        {
            Debug.LogError("Tilemap reference missing!");
            return new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
        }
        
        Vector3[] worldPositions = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            Vector2Int gridPos = _portalPositions[i];
            Vector3Int cellPos = new Vector3Int(gridPos.x, gridPos.y, 0);
            worldPositions[i] = _tilemap.GetCellCenterWorld(cellPos);
        }
        
        return worldPositions;
    }
    
    /// <summary>
    /// Get the world-space bounds of this room based on its tilemap.
    /// </summary>
    public Bounds GetWorldBounds()
    {
        if (_tilemap == null)
        {
            Debug.LogError("Cannot get bounds: Tilemap reference missing!");
            return new Bounds(transform.position, Vector3.one);
        }
        
        // Calculate bounds from grid dimensions using tilemap coordinate conversion
        Vector3 min = _tilemap.GetCellCenterWorld(new Vector3Int(0, 0, 0));
        Vector3 max = _tilemap.GetCellCenterWorld(new Vector3Int(_roomWidth - 1, _roomHeight - 1, 0));
        
        Vector3 center = (min + max) / 2f;
        Vector3 size = max - min + new Vector3(_blockSize, _blockSize, 0); // Add one cell size for full coverage
        
        return new Bounds(center, size);
    }
    
    private void OnDrawGizmos()
    {
        if (_tilemap == null || _portalPositions == null)
            return;
        
        // Draw portal positions and clear radius using tilemap coordinates
        Gizmos.color = Color.cyan;
        foreach (var pos in _portalPositions)
        {
            Vector3Int cellPos = new Vector3Int(pos.x, pos.y, 0);
            Vector3 worldPos = _tilemap.GetCellCenterWorld(cellPos);
            Gizmos.DrawWireSphere(worldPos, _portalClearRadius * _blockSize);
        }
    }
}


