// Procedural cave room generator using Perlin noise + cellular automata
// Generates a single room with destructible terrain and portal placement

using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour
{
    // Room identity - set at runtime by WorldManager via SetRoomCoordinates()
    private int _roomX;
    private int _roomY;
    private int _globalSeed;
    
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
    
    [Header("Tile References")]
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private TileBase _dirtTile;
    [SerializeField] private TileBase _stoneTile;
    [SerializeField] private TileBase _bedrockTile;
    [SerializeField] private GameObject _portalPrefab;
    
    // Grid data
    private TileType[,] _originalGrid;  // As-generated (for delta comparison)
    private TileType[,] _grid;          // Current state (with modifications)
    private System.Random _random;
    private Vector2Int[] _portalPositions;
    private TerrainDamageSystem _damageSystem;
    
    // Public accessors for serialization
    public int RoomX => _roomX;
    public int RoomY => _roomY;
    public int Seed => _globalSeed;
    public int Width => _roomWidth;
    public int Height => _roomHeight;
    public TileType[,] OriginalGrid => _originalGrid;
    public TileType[,] CurrentGrid => _grid;
    
    public void GenerateRoom(SerializedRoomState savedState = null)
    {
        // Initialize random with combined seed
        int combinedSeed = _globalSeed + _roomX * 1000 + _roomY;
        _random = new System.Random(combinedSeed);
        
        // Setup tile database visual references
        TileDatabase.SetVisualTile(TileType.Dirt, _dirtTile);
        TileDatabase.SetVisualTile(TileType.Stone, _stoneTile);
        TileDatabase.SetVisualTile(TileType.Bedrock, _bedrockTile);
        
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
        
        // Phase 5: Store original state (before any modifications)
        _originalGrid = (TileType[,])_grid.Clone();
        
        // Phase 6: Apply saved state if provided
        if (savedState != null)
        {
            RoomSerializer.ApplyRoomState(savedState, _grid);
        }
        
        // Phase 7: Initialize damage system
        _damageSystem = new TerrainDamageSystem(_grid, _tilemap, _roomWidth, _roomHeight);
        
        if (savedState != null && savedState.DamagedTiles != null)
        {
            _damageSystem.RestoreDamagedTiles(savedState.DamagedTiles);
        }
        
        // Phase 8: Build the visual mesh/tilemap
        BuildTilemap();
        
        // Phase 9: Spawn portal sprites
        SpawnPortalSprites();
        
        Debug.Log($"Room generation complete: {CountTiles(TileType.Stone)} stone, {CountTiles(TileType.Dirt)} dirt, {CountTiles(TileType.Air)} air");
    }
    
    private void InitializeGridWithNoise(int seed)
    {
        _grid = new TileType[_roomWidth, _roomHeight];
        
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
                
                // Determine tile type based on noise and depth
                if (noiseValue > _fillThreshold)
                {
                    // Bedrock at bottom 2 rows
                    if (y <= 1)
                    {
                        _grid[x, y] = TileType.Bedrock;
                    }
                    // Stone deeper down (below 30% height)
                    else if (y < _roomHeight * 0.3f)
                    {
                        _grid[x, y] = TileType.Stone;
                    }
                    // Mix of dirt and stone in middle
                    else if (y < _roomHeight * 0.6f)
                    {
                        _grid[x, y] = _random.NextDouble() < 0.6 ? TileType.Dirt : TileType.Stone;
                    }
                    // Mostly dirt higher up
                    else
                    {
                        _grid[x, y] = TileType.Dirt;
                    }
                }
                else
                {
                    _grid[x, y] = TileType.Air;
                }
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
                _grid[pos.x, pos.y] = TileType.Portal;
            }
        }
    }
    
    private void SmoothWithCellularAutomata()
    {
        TileType[,] newGrid = new TileType[_roomWidth, _roomHeight];
        
        for (int x = 0; x < _roomWidth; x++)
        {
            for (int y = 0; y < _roomHeight; y++)
            {
                // Skip portal tiles - they're protected
                if (_grid[x, y] == TileType.Portal)
                {
                    newGrid[x, y] = TileType.Portal;
                    continue;
                }
                
                int groundNeighbors = CountGroundNeighbors(x, y);
                
                // Standard cellular automata rules
                if (groundNeighbors >= 5)
                {
                    // Inherit type from majority neighbor
                    newGrid[x, y] = GetMajorityTileType(x, y);
                }
                else if (groundNeighbors <= 3)
                {
                    newGrid[x, y] = TileType.Air;
                }
                else
                {
                    // Keep current state for boundary cases
                    newGrid[x, y] = _grid[x, y];
                }
                
                // Edge tiles are always bedrock
                if (x == 0 || x == _roomWidth - 1 || y == 0 || y == _roomHeight - 1)
                {
                    newGrid[x, y] = TileType.Bedrock;
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
                
                TileType type = _grid[neighborX, neighborY];
                if (type != TileType.Air && type != TileType.Portal)
                {
                    count++;
                }
            }
        }
        
        return count;
    }
    
    private TileType GetMajorityTileType(int x, int y)
    {
        int dirtCount = 0;
        int stoneCount = 0;
        int bedrockCount = 0;
        
        // Check neighbors to determine what type to become
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                if (offsetX == 0 && offsetY == 0)
                    continue;
                
                int neighborX = x + offsetX;
                int neighborY = y + offsetY;
                
                if (!IsValidPosition(neighborX, neighborY))
                    continue;
                
                TileType type = _grid[neighborX, neighborY];
                if (type == TileType.Dirt) dirtCount++;
                else if (type == TileType.Stone) stoneCount++;
                else if (type == TileType.Bedrock) bedrockCount++;
            }
        }
        
        // Return most common type, default to stone
        if (bedrockCount > dirtCount && bedrockCount > stoneCount)
            return TileType.Bedrock;
        if (dirtCount > stoneCount)
            return TileType.Dirt;
        return TileType.Stone;
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
                        if (_grid[x, y] != TileType.Portal)
                        {
                            _grid[x, y] = TileType.Air;
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
        
        // Clear existing tiles
        _tilemap.ClearAllTiles();
        
        // Build tilemap from grid
        for (int x = 0; x < _roomWidth; x++)
        {
            for (int y = 0; y < _roomHeight; y++)
            {
                TileType type = _grid[x, y];
                
                // Skip air and portals
                if (type == TileType.Air || type == TileType.Portal)
                    continue;
                
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                TileBase visualTile = TileDatabase.GetVisualTile(type);
                
                if (visualTile != null)
                {
                    _tilemap.SetTile(tilePosition, visualTile);
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
    
    private int CountTiles(TileType tileType)
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
    
    // Public API for terrain interaction
    public TileType GetTileAt(int x, int y)
    {
        if (!IsValidPosition(x, y))
            return TileType.Bedrock; // Out of bounds is solid
        
        return _grid[x, y];
    }
    
    public void SetTileAt(int x, int y, TileType tileType)
    {
        if (!IsValidPosition(x, y))
            return;
        
        _grid[x, y] = tileType;
        
        // Update visual representation
        Vector3Int tilePosition = new Vector3Int(x, y, 0);
        if (tileType == TileType.Air || tileType == TileType.Portal)
        {
            _tilemap.SetTile(tilePosition, null);
        }
        else
        {
            TileBase visualTile = TileDatabase.GetVisualTile(tileType);
            if (visualTile != null)
            {
                _tilemap.SetTile(tilePosition, visualTile);
            }
        }
    }
    
    /// <summary>
    /// Damage a tile at grid position. Returns true if destroyed.
    /// </summary>
    public bool DamageTile(int x, int y, int damageAmount)
    {
        if (_damageSystem == null)
            return false;
        
        return _damageSystem.DamageTile(x, y, damageAmount);
    }
    
    /// <summary>
    /// Get damage percentage for visual effects (0.0 to 1.0)
    /// </summary>
    public float GetTileDamagePercent(int x, int y)
    {
        if (_damageSystem == null)
            return 0f;
        
        return _damageSystem.GetDamagePercent(x, y);
    }
    
    /// <summary>
    /// Serialize this room's state for saving
    /// </summary>
    public SerializedRoomState SerializeState()
    {
        TileDamage[] damagedTiles = _damageSystem?.GetDamagedTiles() ?? new TileDamage[0];
        
        return RoomSerializer.SerializeRoom(_roomX, _roomY, _globalSeed, 
            _originalGrid, _grid, damagedTiles, _roomWidth, _roomHeight);
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


